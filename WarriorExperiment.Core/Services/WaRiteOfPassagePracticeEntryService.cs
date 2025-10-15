using Microsoft.EntityFrameworkCore;
using WarriorExperiment.Persistence.Data;
using WarriorExperiment.Persistence.Models;
using WarriorExperiment.Persistence.Enums;

namespace WarriorExperiment.Core.Services;

/// <summary>
/// Service for managing rite of passage practice entries
/// </summary>
public class WaRiteOfPassagePracticeEntryService
{
    private readonly WaDbContext _context;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="WaRiteOfPassagePracticeEntryService"/> class
    /// </summary>
    /// <param name="context">The database context</param>
    public WaRiteOfPassagePracticeEntryService(WaDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Gets a rite of passage practice entry by ID
    /// </summary>
    /// <param name="id">The entry ID</param>
    /// <returns>The practice entry if found, null otherwise</returns>
    public async Task<WaRiteOfPassagePracticeEntry?> GetByIdAsync(int id)
    {
        return await _context.RiteOfPassagePracticeEntries
            .Include(rpe => rpe.User)
            .FirstOrDefaultAsync(rpe => rpe.Id == id);
    }
    
    /// <summary>
    /// Gets all rite of passage practice entries for a specific user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>List of practice entries ordered by date descending</returns>
    public async Task<List<WaRiteOfPassagePracticeEntry>> GetAllAsync(int userId)
    {
        return await _context.RiteOfPassagePracticeEntries
            .Where(rpe => rpe.UserId == userId)
            .OrderByDescending(rpe => rpe.Date)
            .ToListAsync();
    }
    
    /// <summary>
    /// Gets paginated rite of passage practice entries for a specific user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="pageIndex">Zero-based page index</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated list of practice entries</returns>
    public async Task<List<WaRiteOfPassagePracticeEntry>> GetPagedAsync(int userId, int pageIndex, int pageSize)
    {
        return await _context.RiteOfPassagePracticeEntries
            .Where(rpe => rpe.UserId == userId)
            .OrderByDescending(rpe => rpe.Date)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    /// <summary>
    /// Gets the total count of rite of passage practice entries for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>Total count of entries</returns>
    public async Task<int> GetCountAsync(int userId)
    {
        return await _context.RiteOfPassagePracticeEntries
            .CountAsync(rpe => rpe.UserId == userId);
    }
    
    /// <summary>
    /// Creates a new rite of passage practice entry
    /// </summary>
    /// <param name="entry">The practice entry to create</param>
    /// <returns>The created entry with generated ID</returns>
    public async Task<WaRiteOfPassagePracticeEntry> CreateAsync(WaRiteOfPassagePracticeEntry entry)
    {
        entry.CreatedAt = DateTime.UtcNow;
        entry.UpdatedAt = DateTime.UtcNow;
        entry.EnteredAt = DateTime.UtcNow;
        
        _context.RiteOfPassagePracticeEntries.Add(entry);
        await _context.SaveChangesAsync();
        
        return entry;
    }
    
    /// <summary>
    /// Updates an existing rite of passage practice entry
    /// </summary>
    /// <param name="entry">The practice entry to update</param>
    /// <returns>The updated entry</returns>
    public async Task<WaRiteOfPassagePracticeEntry> UpdateAsync(WaRiteOfPassagePracticeEntry entry)
    {
        entry.UpdatedAt = DateTime.UtcNow;
        
        _context.RiteOfPassagePracticeEntries.Update(entry);
        await _context.SaveChangesAsync();
        
        return entry;
    }
    
    /// <summary>
    /// Deletes a rite of passage practice entry by ID
    /// </summary>
    /// <param name="id">The entry ID to delete</param>
    /// <returns>True if deleted successfully, false if not found</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var entry = await _context.RiteOfPassagePracticeEntries.FindAsync(id);
        if (entry == null)
            return false;
        
        _context.RiteOfPassagePracticeEntries.Remove(entry);
        await _context.SaveChangesAsync();
        
        return true;
    }
    
    /// <summary>
    /// Gets a rite of passage practice entry for a specific user and date
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="date">The date</param>
    /// <returns>The practice entry if found, null otherwise</returns>
    public async Task<WaRiteOfPassagePracticeEntry?> GetByUserAndDateAsync(int userId, DateTime date)
    {
        return await _context.RiteOfPassagePracticeEntries
            .FirstOrDefaultAsync(rpe => rpe.UserId == userId && rpe.Date.Date == date.Date);
    }
    
    /// <summary>
    /// Gets the last successful heavy day entry for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>The last successful heavy practice entry if found, null otherwise</returns>
    public async Task<WaRiteOfPassagePracticeEntry?> GetLastSuccessfulHeavyDayAsync(int userId)
    {
        return await _context.RiteOfPassagePracticeEntries
            .Where(rpe => rpe.UserId == userId && 
                         rpe.PracticeIntensity == WaPracticeIntensity.Heavy && 
                         rpe.Success)
            .OrderByDescending(rpe => rpe.Date)
            .FirstOrDefaultAsync();
    }
    
    /// <summary>
    /// Gets the last entry (any intensity) for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>The most recent practice entry if found, null otherwise</returns>
    public async Task<WaRiteOfPassagePracticeEntry?> GetLastEntryAsync(int userId)
    {
        return await _context.RiteOfPassagePracticeEntries
            .Where(rpe => rpe.UserId == userId)
            .OrderByDescending(rpe => rpe.Date)
            .FirstOrDefaultAsync();
    }
    
    /// <summary>
    /// Gets the suggested next intensity based on the last entry
    /// Following the cycle: Heavy → Light → Medium → Heavy...
    /// </summary>
    /// <param name="lastEntry">The last practice entry</param>
    /// <returns>The suggested next intensity</returns>
    public WaPracticeIntensity GetSuggestedNextIntensity(WaRiteOfPassagePracticeEntry? lastEntry)
    {
        if (lastEntry == null)
            return WaPracticeIntensity.Heavy; // Start with heavy
        
        return lastEntry.PracticeIntensity switch
        {
            WaPracticeIntensity.Heavy => WaPracticeIntensity.Light,
            WaPracticeIntensity.Light => WaPracticeIntensity.Medium,
            WaPracticeIntensity.Medium => WaPracticeIntensity.Heavy,
            _ => WaPracticeIntensity.Heavy
        };
    }
    
    /// <summary>
    /// Calculates suggested ladder values based on last successful heavy day and intensity
    /// </summary>
    /// <param name="lastHeavyEntry">The last successful heavy day entry</param>
    /// <param name="intensity">The intensity for the new entry</param>
    /// <returns>Tuple containing the 5 ladder values</returns>
    public (int ladder1, int ladder2, int ladder3, int ladder4, int ladder5) CalculateSuggestedLadderValues(
        WaRiteOfPassagePracticeEntry? lastHeavyEntry, 
        WaPracticeIntensity intensity)
    {
        // Default starting values for first time
        if (lastHeavyEntry == null)
        {
            return intensity switch
            {
                WaPracticeIntensity.Heavy => (3, 3, 3, 0, 0),
                WaPracticeIntensity.Medium => (2, 2, 2, 0, 0),
                WaPracticeIntensity.Light => (1, 1, 1, 0, 0),
                _ => (3, 3, 3, 0, 0)
            };
        }
        
        // Determine current ladder configuration from last heavy entry
        var activeLadders = 0;
        if (lastHeavyEntry.Ladder5Sets > 0) activeLadders = 5;
        else if (lastHeavyEntry.Ladder4Sets > 0) activeLadders = 4;
        else if (lastHeavyEntry.Ladder3Sets > 0) activeLadders = 3;
        else if (lastHeavyEntry.Ladder2Sets > 0) activeLadders = 2;
        else if (lastHeavyEntry.Ladder1Sets > 0) activeLadders = 1;
        
        // Get the maximum ladder height (steps) from last entry
        var maxSteps = Math.Max(
            lastHeavyEntry.Ladder1Sets,
            Math.Max(lastHeavyEntry.Ladder2Sets,
            Math.Max(lastHeavyEntry.Ladder3Sets,
            Math.Max(lastHeavyEntry.Ladder4Sets, lastHeavyEntry.Ladder5Sets))));
        
        // Progress logic for heavy days
        if (intensity == WaPracticeIntensity.Heavy)
        {
            // If using 3 ladders (1,2,3), add a 4th ladder
            if (activeLadders == 3)
            {
                return (maxSteps, maxSteps, maxSteps, maxSteps, 0);
            }
            // If using 4 ladders (1,2,3,4), add a 5th ladder
            else if (activeLadders == 4)
            {
                return (maxSteps, maxSteps, maxSteps, maxSteps, maxSteps);
            }
            // If using 5 ladders and steps < 5, increment steps
            else if (activeLadders == 5 && maxSteps < 5)
            {
                var newSteps = maxSteps + 1;
                return (newSteps, newSteps, newSteps, newSteps, newSteps);
            }
            // If at maximum (5 ladders with 5 steps each), stay there
            else if (activeLadders == 5 && maxSteps >= 5)
            {
                return (5, 5, 5, 5, 5);
            }
            // For other cases (1 or 2 ladders), progress to 3 ladders
            else
            {
                return (3, 3, 3, 0, 0);
            }
        }
        
        // For medium and light days, reduce from the heavy day values
        var reduction = intensity == WaPracticeIntensity.Medium ? 1 : 2;
        
        return (
            Math.Max(0, lastHeavyEntry.Ladder1Sets - reduction),
            Math.Max(0, lastHeavyEntry.Ladder2Sets - reduction),
            Math.Max(0, lastHeavyEntry.Ladder3Sets - reduction),
            Math.Max(0, lastHeavyEntry.Ladder4Sets - reduction),
            Math.Max(0, lastHeavyEntry.Ladder5Sets - reduction)
        );
    }
    
    /// <summary>
    /// Gets total pull count from successful practices for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>Total pulls from successful practices</returns>
    public async Task<int> GetTotalSuccessfulPullsAsync(int userId)
    {
        return await _context.RiteOfPassagePracticeEntries
            .Where(rpe => rpe.UserId == userId && rpe.Success)
            .SumAsync(rpe => rpe.PullCount);
    }
    
    /// <summary>
    /// Gets total push count from ladder calculations for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>Total pushes calculated using ladder formula: n*(n+1)/2 for each ladder</returns>
    public async Task<int> GetTotalLadderPushesAsync(int userId)
    {
        var entries = await _context.RiteOfPassagePracticeEntries
            .Where(rpe => rpe.UserId == userId)
            .Select(rpe => new
            {
                rpe.Ladder1Sets,
                rpe.Ladder2Sets,
                rpe.Ladder3Sets,
                rpe.Ladder4Sets,
                rpe.Ladder5Sets
            })
            .ToListAsync();
            
        var totalPushes = 0;
        
        foreach (var entry in entries)
        {
            totalPushes += CalculateLadderPushes(entry.Ladder1Sets);
            totalPushes += CalculateLadderPushes(entry.Ladder2Sets);
            totalPushes += CalculateLadderPushes(entry.Ladder3Sets);
            totalPushes += CalculateLadderPushes(entry.Ladder4Sets);
            totalPushes += CalculateLadderPushes(entry.Ladder5Sets);
        }
        
        return totalPushes;
    }
    
    /// <summary>
    /// Calculates pushes for a single ladder using the formula: n*(n+1)/2
    /// </summary>
    /// <param name="ladderSets">Number of sets in the ladder</param>
    /// <returns>Total pushes for the ladder</returns>
    private static int CalculateLadderPushes(int ladderSets)
    {
        return ladderSets * (ladderSets + 1) / 2;
    }
}