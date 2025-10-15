using Microsoft.EntityFrameworkCore;
using WarriorExperiment.Persistence.Data;
using WarriorExperiment.Persistence.Models;

namespace WarriorExperiment.Core.Services;

/// <summary>
/// Service for managing daily survey entries
/// </summary>
public class WaDailySurveyEntryService
{
    private readonly WaDbContext _context;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="WaDailySurveyEntryService"/> class
    /// </summary>
    /// <param name="context">The database context</param>
    public WaDailySurveyEntryService(WaDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Gets a daily survey entry by ID
    /// </summary>
    /// <param name="id">The entry ID</param>
    /// <returns>The daily survey entry if found, null otherwise</returns>
    public async Task<WaDailySurveyEntry?> GetByIdAsync(int id)
    {
        return await _context.DailySurveys
            .Include(ds => ds.User)
            .FirstOrDefaultAsync(ds => ds.Id == id);
    }
    
    /// <summary>
    /// Gets all daily survey entries for a specific user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>List of daily survey entries ordered by date descending</returns>
    public async Task<List<WaDailySurveyEntry>> GetAllAsync(int userId)
    {
        return await _context.DailySurveys
            .Where(ds => ds.UserId == userId)
            .OrderByDescending(ds => ds.Date)
            .ToListAsync();
    }
    
    /// <summary>
    /// Gets paginated daily survey entries for a specific user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="pageIndex">Zero-based page index</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated list of daily survey entries</returns>
    public async Task<List<WaDailySurveyEntry>> GetPagedAsync(int userId, int pageIndex, int pageSize)
    {
        return await _context.DailySurveys
            .Where(ds => ds.UserId == userId)
            .OrderByDescending(ds => ds.Date)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    /// <summary>
    /// Gets the total count of daily survey entries for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>Total count of entries</returns>
    public async Task<int> GetCountAsync(int userId)
    {
        return await _context.DailySurveys
            .CountAsync(ds => ds.UserId == userId);
    }
    
    /// <summary>
    /// Creates a new daily survey entry
    /// </summary>
    /// <param name="entry">The daily survey entry to create</param>
    /// <returns>The created entry with generated ID</returns>
    public async Task<WaDailySurveyEntry> CreateAsync(WaDailySurveyEntry entry)
    {
        entry.CreatedAt = DateTime.UtcNow;
        entry.UpdatedAt = DateTime.UtcNow;
        entry.EnteredAt = DateTime.UtcNow;
        
        _context.DailySurveys.Add(entry);
        await _context.SaveChangesAsync();
        
        return entry;
    }
    
    /// <summary>
    /// Updates an existing daily survey entry
    /// </summary>
    /// <param name="entry">The daily survey entry to update</param>
    /// <returns>The updated entry</returns>
    public async Task<WaDailySurveyEntry> UpdateAsync(WaDailySurveyEntry entry)
    {
        entry.UpdatedAt = DateTime.UtcNow;
        
        _context.DailySurveys.Update(entry);
        await _context.SaveChangesAsync();
        
        return entry;
    }
    
    /// <summary>
    /// Deletes a daily survey entry by ID
    /// </summary>
    /// <param name="id">The entry ID to delete</param>
    /// <returns>True if deleted successfully, false if not found</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var entry = await _context.DailySurveys.FindAsync(id);
        if (entry == null)
            return false;
        
        _context.DailySurveys.Remove(entry);
        await _context.SaveChangesAsync();
        
        return true;
    }
    
    /// <summary>
    /// Gets a daily survey entry for a specific user and date
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="date">The date</param>
    /// <returns>The daily survey entry if found, null otherwise</returns>
    public async Task<WaDailySurveyEntry?> GetByUserAndDateAsync(int userId, DateTime date)
    {
        var dateUtc = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        
        return await _context.DailySurveys
            .FirstOrDefaultAsync(ds => ds.UserId == userId && ds.Date.Date == dateUtc);
    }
    
    /// <summary>
    /// Gets daily survey entries for a specific user within a date range
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="startDate">The start date (inclusive)</param>
    /// <param name="endDate">The end date (inclusive)</param>
    /// <returns>List of daily survey entries ordered by date ascending</returns>
    public async Task<List<WaDailySurveyEntry>> GetByUserAndDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
    {
        var startDateUtc = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
        var endDateUtc = DateTime.SpecifyKind(endDate.Date, DateTimeKind.Utc);
        
        return await _context.DailySurveys
            .Where(ds => ds.UserId == userId && ds.Date.Date >= startDateUtc && ds.Date.Date <= endDateUtc)
            .OrderBy(ds => ds.Date)
            .ToListAsync();
    }
}