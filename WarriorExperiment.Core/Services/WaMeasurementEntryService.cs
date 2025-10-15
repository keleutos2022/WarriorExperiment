using Microsoft.EntityFrameworkCore;
using WarriorExperiment.Persistence.Data;
using WarriorExperiment.Persistence.Models;

namespace WarriorExperiment.Core.Services;

/// <summary>
/// Service for managing measurement entries
/// </summary>
public class WaMeasurementEntryService
{
    private readonly WaDbContext _context;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="WaMeasurementEntryService"/> class
    /// </summary>
    /// <param name="context">The database context</param>
    public WaMeasurementEntryService(WaDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Gets a measurement entry by ID
    /// </summary>
    /// <param name="id">The entry ID</param>
    /// <returns>The measurement entry if found, null otherwise</returns>
    public async Task<WaMeasurementEntry?> GetByIdAsync(int id)
    {
        return await _context.MeasurementEntries
            .Include(me => me.User)
            .Include(me => me.MeasurementMethod)
            .FirstOrDefaultAsync(me => me.Id == id);
    }
    
    /// <summary>
    /// Gets all measurement entries for a specific user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>List of measurement entries ordered by date descending</returns>
    public async Task<List<WaMeasurementEntry>> GetAllAsync(int userId)
    {
        return await _context.MeasurementEntries
            .Include(me => me.MeasurementMethod)
            .Where(me => me.UserId == userId)
            .OrderByDescending(me => me.Date)
            .ToListAsync();
    }
    
    /// <summary>
    /// Gets paginated measurement entries for a specific user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="pageIndex">Zero-based page index</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated list of measurement entries</returns>
    public async Task<List<WaMeasurementEntry>> GetPagedAsync(int userId, int pageIndex, int pageSize)
    {
        return await _context.MeasurementEntries
            .Include(me => me.MeasurementMethod)
            .Where(me => me.UserId == userId)
            .OrderByDescending(me => me.Date)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    /// <summary>
    /// Gets the total count of measurement entries for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>Total count of entries</returns>
    public async Task<int> GetCountAsync(int userId)
    {
        return await _context.MeasurementEntries
            .CountAsync(me => me.UserId == userId);
    }
    
    /// <summary>
    /// Creates a new measurement entry
    /// </summary>
    /// <param name="entry">The measurement entry to create</param>
    /// <returns>The created entry with generated ID</returns>
    public async Task<WaMeasurementEntry> CreateAsync(WaMeasurementEntry entry)
    {
        entry.CreatedAt = DateTime.UtcNow;
        entry.UpdatedAt = DateTime.UtcNow;
        entry.EnteredAt = DateTime.UtcNow;
        
        _context.MeasurementEntries.Add(entry);
        await _context.SaveChangesAsync();
        
        return entry;
    }
    
    /// <summary>
    /// Updates an existing measurement entry
    /// </summary>
    /// <param name="entry">The measurement entry to update</param>
    /// <returns>The updated entry</returns>
    public async Task<WaMeasurementEntry> UpdateAsync(WaMeasurementEntry entry)
    {
        entry.UpdatedAt = DateTime.UtcNow;
        
        _context.MeasurementEntries.Update(entry);
        await _context.SaveChangesAsync();
        
        return entry;
    }
    
    /// <summary>
    /// Deletes a measurement entry by ID
    /// </summary>
    /// <param name="id">The entry ID to delete</param>
    /// <returns>True if deleted successfully, false if not found</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var entry = await _context.MeasurementEntries.FindAsync(id);
        if (entry == null)
            return false;
        
        _context.MeasurementEntries.Remove(entry);
        await _context.SaveChangesAsync();
        
        return true;
    }
    
    /// <summary>
    /// Gets measurement entries for a specific user within a date range
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="startDate">The start date (inclusive)</param>
    /// <param name="endDate">The end date (inclusive)</param>
    /// <param name="measurementMethodId">Optional measurement method ID filter</param>
    /// <returns>List of measurement entries ordered by date ascending</returns>
    public async Task<List<WaMeasurementEntry>> GetByUserAndDateRangeAsync(int userId, DateTime startDate, DateTime endDate, int? measurementMethodId = null)
    {
        var startDateUtc = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
        var endDateUtc = DateTime.SpecifyKind(endDate.Date, DateTimeKind.Utc);
        
        var query = _context.MeasurementEntries
            .Include(me => me.MeasurementMethod)
            .Where(me => me.UserId == userId && me.Date.Date >= startDateUtc && me.Date.Date <= endDateUtc);
            
        if (measurementMethodId.HasValue)
        {
            query = query.Where(me => me.MeasurementMethodId == measurementMethodId.Value);
        }
        
        return await query
            .OrderBy(me => me.Date)
            .ToListAsync();
    }
    
    /// <summary>
    /// Gets circumference and photo data for a specific user within a date range (cross-method)
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="startDate">The start date (inclusive)</param>
    /// <param name="endDate">The end date (inclusive)</param>
    /// <returns>List of measurement entries with circumferences/photos, ordered by date ascending</returns>
    public async Task<List<WaMeasurementEntry>> GetCircumferencesAndPhotosAsync(int userId, DateTime startDate, DateTime endDate)
    {
        var startDateUtc = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
        var endDateUtc = DateTime.SpecifyKind(endDate.Date, DateTimeKind.Utc);
        
        return await _context.MeasurementEntries
            .Include(me => me.MeasurementMethod)
            .Where(me => me.UserId == userId && me.Date.Date >= startDateUtc && me.Date.Date <= endDateUtc)
            .Where(me => me.ChestCircumference.HasValue || me.WaistCircumference.HasValue || 
                        me.HipCircumference.HasValue || me.BicepCircumference.HasValue || 
                        me.ThighCircumference.HasValue || me.CalfCircumference.HasValue ||
                        me.FrontPhoto != null || me.BackPhoto != null || me.SidePhoto != null)
            .OrderBy(me => me.Date)
            .ToListAsync();
    }
    
    /// <summary>
    /// Gets all available measurement methods
    /// </summary>
    /// <returns>List of measurement methods</returns>
    public async Task<List<WaMeasurementMethod>> GetMeasurementMethodsAsync()
    {
        return await _context.MeasurementMethods
            .OrderBy(mm => mm.Name)
            .ToListAsync();
    }
    
    /// <summary>
    /// Gets the latest body composition changes for a user (latest measurement by method compared to previous)
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>Body composition changes (negative body fat, positive muscle mass)</returns>
    public async Task<(decimal? BodyFatChange, decimal? MuscleMassChange)> GetLatestBodyCompositionChangesAsync(int userId)
    {
        decimal? bodyFatChange = null;
        decimal? muscleMassChange = null;
        
        var methods = await GetMeasurementMethodsAsync();
        
        foreach (var method in methods)
        {
            var latestTwo = await _context.MeasurementEntries
                .Where(me => me.UserId == userId && me.MeasurementMethodId == method.Id)
                .Where(me => me.BodyFat.HasValue || me.MuscleMass.HasValue)
                .OrderByDescending(me => me.Date)
                .Take(2)
                .ToListAsync();
                
            if (latestTwo.Count == 2)
            {
                var latest = latestTwo[0];
                var previous = latestTwo[1];
                
                if (latest.BodyFat.HasValue && previous.BodyFat.HasValue)
                {
                    var change = latest.BodyFat.Value - previous.BodyFat.Value;
                    if (change < 0 && (!bodyFatChange.HasValue || change < bodyFatChange.Value))
                    {
                        bodyFatChange = change;
                    }
                }
                
                if (latest.MuscleMass.HasValue && previous.MuscleMass.HasValue)
                {
                    var change = latest.MuscleMass.Value - previous.MuscleMass.Value;
                    if (change > 0 && (!muscleMassChange.HasValue || change > muscleMassChange.Value))
                    {
                        muscleMassChange = change;
                    }
                }
            }
        }
        
        return (bodyFatChange, muscleMassChange);
    }
}