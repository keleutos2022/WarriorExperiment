using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WarriorExperiment.Persistence.Data;
using WarriorExperiment.Persistence.Entities;

namespace WarriorExperiment.Core.Services;

/// <summary>
/// Service for managing users and current user selection
/// </summary>
public class WaUserService
{
    private readonly WaDbContext _context;
    private readonly UserManager<WaUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private int? _selectedUserId;
    
    /// <summary>
    /// Event that fires when the current user selection changes
    /// </summary>
    public event Action<WaUser?>? CurrentUserChanged;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="WaUserService"/> class
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="userManager">The Identity user manager</param>
    /// <param name="httpContextAccessor">The HTTP context accessor</param>
    public WaUserService(WaDbContext context, UserManager<WaUser> userManager, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }
    
    /// <summary>
    /// Gets a user by ID
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <returns>The user if found, null otherwise</returns>
    public async Task<WaUser?> GetByIdAsync(int id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }
    
    /// <summary>
    /// Gets all users
    /// </summary>
    /// <returns>List of all users ordered by username</returns>
    public async Task<List<WaUser>> GetAllAsync()
    {
        return await _context.Users
            .OrderBy(u => u.UserName)
            .ToListAsync();
    }
    
    /// <summary>
    /// Gets paginated users
    /// </summary>
    /// <param name="pageIndex">Zero-based page index</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated list of users</returns>
    public async Task<List<WaUser>> GetPagedAsync(int pageIndex, int pageSize)
    {
        return await _context.Users
            .OrderBy(u => u.UserName)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    /// <summary>
    /// Gets the total count of users
    /// </summary>
    /// <returns>Total count of users</returns>
    public async Task<int> GetCountAsync()
    {
        return await _context.Users.CountAsync();
    }
    
    /// <summary>
    /// Creates a new user
    /// </summary>
    /// <param name="user">The user to create</param>
    /// <returns>The created user with generated ID</returns>
    public async Task<WaUser> CreateAsync(WaUser user)
    {
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        user.EnteredAt = DateTime.UtcNow;
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        // Handle default user logic
        if (user.IsDefault)
        {
            await EnsureSingleDefaultUserAsync(user.Id);
        }
        
        return user;
    }
    
    /// <summary>
    /// Updates an existing user
    /// </summary>
    /// <param name="user">The user to update</param>
    /// <returns>The updated user</returns>
    public async Task<WaUser> UpdateAsync(WaUser user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        
        // Handle default user logic
        if (user.IsDefault)
        {
            await EnsureSingleDefaultUserAsync(user.Id);
        }
        
        return user;
    }
    
    /// <summary>
    /// Deletes a user by ID
    /// </summary>
    /// <param name="id">The user ID to delete</param>
    /// <returns>True if deleted successfully, false if not found</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return false;
        
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        
        return true;
    }
    
    /// <summary>
    /// Gets the current authenticated user
    /// </summary>
    /// <returns>The current authenticated user</returns>
    public async Task<WaUser?> GetCurrentUserAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(httpContext.User);
            return user;
        }
        
        return null;
    }
    
    
    /// <summary>
    /// Gets the default user, or first user if no default is set
    /// </summary>
    /// <returns>The default user or first user</returns>
    public async Task<WaUser?> GetDefaultUserAsync()
    {
        // First try to get the user marked as default
        var defaultUser = await _context.Users
            .FirstOrDefaultAsync(u => u.IsDefault);
            
        if (defaultUser != null)
        {
            return defaultUser;
        }
        
        // If no default user, return first user and mark it as default
        var firstUser = await _context.Users
            .OrderBy(u => u.Id)
            .FirstOrDefaultAsync();
            
        if (firstUser != null)
        {
            firstUser.IsDefault = true;
            firstUser.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        
        return firstUser;
    }
    
    
    /// <summary>
    /// Ensures only one user is marked as default
    /// </summary>
    /// <param name="defaultUserId">The ID of the user that should be default</param>
    /// <returns>Task</returns>
    public async Task EnsureSingleDefaultUserAsync(int defaultUserId)
    {
        // Set all users to not default
        var users = await _context.Users.ToListAsync();
        foreach (var user in users)
        {
            user.IsDefault = user.Id == defaultUserId;
            user.UpdatedAt = DateTime.UtcNow;
        }
        
        await _context.SaveChangesAsync();
    }
    
    /// <summary>
    /// Sets the current user by ID
    /// </summary>
    /// <param name="userId">The user ID to select</param>
    /// <returns>The selected user, or null if not found</returns>
    public async Task<WaUser?> SetCurrentUserAsync(int userId)
    {
        var user = await GetByIdAsync(userId);
        if (user != null)
        {
            _selectedUserId = userId;
            CurrentUserChanged?.Invoke(user);
            return user;
        }
        return null;
    }
    
    /// <summary>
    /// Gets the ID of the currently selected user
    /// </summary>
    /// <returns>The selected user ID, or null if none selected</returns>
    public int? GetSelectedUserId()
    {
        return _selectedUserId;
    }
    
    /// <summary>
    /// Checks if a username already exists
    /// </summary>
    /// <param name="userName">The username to check</param>
    /// <param name="excludeUserId">Optional user ID to exclude from the check (for updates)</param>
    /// <returns>True if username exists, false otherwise</returns>
    public async Task<bool> UserNameExistsAsync(string userName, int? excludeUserId = null)
    {
        var query = _context.Users.Where(u => u.UserName.ToLower() == userName.ToLower());
        
        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.Id != excludeUserId.Value);
        }
        
        return await query.AnyAsync();
    }
}