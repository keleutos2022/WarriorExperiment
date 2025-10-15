using Microsoft.EntityFrameworkCore;
using WarriorExperiment.Persistence.Data;
using WarriorExperiment.Persistence.Models;

namespace WarriorExperiment.Core.Services;

/// <summary>
/// Service for managing variety practice entries
/// </summary>
public class WaVarietyPracticeEntryService
{
    private readonly WaDbContext _context;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="WaVarietyPracticeEntryService"/> class
    /// </summary>
    /// <param name="context">The database context</param>
    public WaVarietyPracticeEntryService(WaDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Gets a variety practice entry by ID
    /// </summary>
    /// <param name="id">The entry ID</param>
    /// <returns>The practice entry if found, null otherwise</returns>
    public async Task<WaVarietyPracticeEntry?> GetByIdAsync(int id)
    {
        return await _context.VarietyPractices
            .Include(vp => vp.User)
            .Include(vp => vp.Exercises)
            .FirstOrDefaultAsync(vp => vp.Id == id);
    }
    
    /// <summary>
    /// Gets all variety practice entries for a specific user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>List of practice entries ordered by date descending</returns>
    public async Task<List<WaVarietyPracticeEntry>> GetAllAsync(int userId)
    {
        return await _context.VarietyPractices
            .Include(vp => vp.Exercises)
            .Where(vp => vp.UserId == userId)
            .OrderByDescending(vp => vp.Date)
            .ToListAsync();
    }
    
    /// <summary>
    /// Gets paginated variety practice entries for a specific user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="pageIndex">Zero-based page index</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated list of practice entries</returns>
    public async Task<List<WaVarietyPracticeEntry>> GetPagedAsync(int userId, int pageIndex, int pageSize)
    {
        return await _context.VarietyPractices
            .Include(vp => vp.Exercises)
            .Where(vp => vp.UserId == userId)
            .OrderByDescending(vp => vp.Date)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    /// <summary>
    /// Gets the total count of variety practice entries for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>Total count of entries</returns>
    public async Task<int> GetCountAsync(int userId)
    {
        return await _context.VarietyPractices
            .CountAsync(vp => vp.UserId == userId);
    }
    
    /// <summary>
    /// Creates a new variety practice entry
    /// </summary>
    /// <param name="entry">The practice entry to create</param>
    /// <returns>The created entry with generated ID</returns>
    public async Task<WaVarietyPracticeEntry> CreateAsync(WaVarietyPracticeEntry entry)
    {
        entry.CreatedAt = DateTime.UtcNow;
        entry.UpdatedAt = DateTime.UtcNow;
        entry.EnteredAt = DateTime.UtcNow;
        
        // Set foreign key for exercises
        foreach (var exercise in entry.Exercises)
        {
            exercise.CreatedAt = DateTime.UtcNow;
            exercise.UpdatedAt = DateTime.UtcNow;
            exercise.EnteredAt = DateTime.UtcNow;
        }
        
        _context.VarietyPractices.Add(entry);
        await _context.SaveChangesAsync();
        
        return entry;
    }
    
    /// <summary>
    /// Updates an existing variety practice entry
    /// </summary>
    /// <param name="entry">The practice entry to update</param>
    /// <returns>The updated entry</returns>
    public async Task<WaVarietyPracticeEntry> UpdateAsync(WaVarietyPracticeEntry entry)
    {
        entry.UpdatedAt = DateTime.UtcNow;
        
        // Get existing entry with exercises
        var existingEntry = await _context.VarietyPractices
            .Include(vp => vp.Exercises)
            .FirstOrDefaultAsync(vp => vp.Id == entry.Id);
        
        if (existingEntry != null)
        {
            // Remove existing exercises
            _context.Exercises.RemoveRange(existingEntry.Exercises);
            
            // Update entry properties
            _context.Entry(existingEntry).CurrentValues.SetValues(entry);
            
            // Add new exercises
            foreach (var exercise in entry.Exercises)
            {
                exercise.VarietyPracticeId = entry.Id;
                exercise.CreatedAt = DateTime.UtcNow;
                exercise.UpdatedAt = DateTime.UtcNow;
                exercise.EnteredAt = DateTime.UtcNow;
                existingEntry.Exercises.Add(exercise);
            }
            
            await _context.SaveChangesAsync();
            return existingEntry;
        }
        
        throw new InvalidOperationException($"Variety practice entry with ID {entry.Id} not found");
    }
    
    /// <summary>
    /// Deletes a variety practice entry by ID
    /// </summary>
    /// <param name="id">The entry ID to delete</param>
    /// <returns>True if deleted successfully, false if not found</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var entry = await _context.VarietyPractices
            .Include(vp => vp.Exercises)
            .FirstOrDefaultAsync(vp => vp.Id == id);
        
        if (entry == null)
            return false;
        
        _context.VarietyPractices.Remove(entry);
        await _context.SaveChangesAsync();
        
        return true;
    }
    
    /// <summary>
    /// Gets a variety practice entry for a specific user and date
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="date">The date</param>
    /// <returns>The practice entry if found, null otherwise</returns>
    public async Task<WaVarietyPracticeEntry?> GetByUserAndDateAsync(int userId, DateTime date)
    {
        return await _context.VarietyPractices
            .Include(vp => vp.Exercises)
            .FirstOrDefaultAsync(vp => vp.UserId == userId && vp.Date.Date == date.Date);
    }
}