using Microsoft.EntityFrameworkCore;
using WarriorExperiment.Persistence.Data;
using WarriorExperiment.Persistence.Models;

namespace WarriorExperiment.Core.Services;

/// <summary>
/// Service for managing users and current user selection
/// </summary>
public class WaUserService
{
    private readonly WaDbContext _context;
    private int? _selectedUserId;
    
    /// <summary>
    /// Event that fires when the current user selection changes
    /// </summary>
    public event Action<WaUser?>? CurrentUserChanged;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="WaUserService"/> class
    /// </summary>
    /// <param name="context">The database context</param>
    public WaUserService(WaDbContext context)
    {
        _context = context;
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
    /// Gets the current user based on selection, or first user if none selected
    /// </summary>
    /// <returns>The current user</returns>
    public async Task<WaUser?> GetCurrentUserAsync()
    {
        if (_selectedUserId.HasValue)
        {
            var selectedUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == _selectedUserId.Value);
            if (selectedUser != null)
            {
                return selectedUser;
            }
        }
        
        // If no selection or selected user not found, return first user
        var firstUser = await _context.Users
            .OrderBy(u => u.Id)
            .FirstOrDefaultAsync();
            
        // Auto-select the first user if we found one
        if (firstUser != null && !_selectedUserId.HasValue)
        {
            _selectedUserId = firstUser.Id;
        }
        
        return firstUser;
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