using Microsoft.EntityFrameworkCore;
using WarriorExperiment.Persistence.Data;
using WarriorExperiment.Persistence.Entities;
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
        
        // Progress logic for heavy days
        if (intensity == WaPracticeIntensity.Heavy)
        {
            // Define the 13-step progression pattern
            var progressionSteps = new (int, int, int, int, int)[]
            {
                (3, 3, 3, 0, 0), // W1
                (3, 3, 3, 3, 0), // W2
                (3, 3, 3, 3, 3), // W3
                (4, 3, 3, 3, 3), // W4
                (4, 4, 3, 3, 3), // W5
                (4, 4, 4, 3, 3), // W6
                (4, 4, 4, 4, 3), // W7
                (4, 4, 4, 4, 4), // W8
                (5, 4, 4, 4, 4), // W9
                (5, 5, 4, 4, 4), // W10
                (5, 5, 5, 4, 4), // W11
                (5, 5, 5, 5, 4), // W12
                (5, 5, 5, 5, 5)  // W13
            };
            
            // Find current step based on last heavy entry configuration
            var currentConfig = (lastHeavyEntry.Ladder1Sets, lastHeavyEntry.Ladder2Sets, 
                               lastHeavyEntry.Ladder3Sets, lastHeavyEntry.Ladder4Sets, lastHeavyEntry.Ladder5Sets);
            
            var currentStepIndex = -1;
            for (int i = 0; i < progressionSteps.Length; i++)
            {
                if (progressionSteps[i] == currentConfig)
                {
                    currentStepIndex = i;
                    break;
                }
            }
            
            // If current configuration matches a step, advance to next step
            if (currentStepIndex >= 0 && currentStepIndex < progressionSteps.Length - 1)
            {
                return progressionSteps[currentStepIndex + 1];
            }
            // If at maximum step or configuration not found, stay at maximum
            else if (currentStepIndex == progressionSteps.Length - 1)
            {
                return progressionSteps[currentStepIndex]; // Stay at W13
            }
            // If configuration doesn't match any step, start from W1
            else
            {
                return progressionSteps[0];
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
    
    /// <summary>
    /// Gets the current weekly streak for rite of passage practices
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>Number of consecutive weeks with 3+ practices (including current week if applicable)</returns>
    public async Task<int> GetCurrentWeeklyStreakAsync(int userId)
    {
        var entries = await _context.RiteOfPassagePracticeEntries
            .Where(rpe => rpe.UserId == userId)
            .OrderByDescending(rpe => rpe.Date)
            .Select(rpe => rpe.Date.Date)
            .ToListAsync();
        
        if (!entries.Any())
            return 0;
        
        var today = DateTime.UtcNow.Date;
        var currentWeekStart = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
        if (currentWeekStart > today)
            currentWeekStart = currentWeekStart.AddDays(-7); // Handle Sunday edge case
        
        // Group entries by week
        var weekGroups = entries
            .GroupBy(date => 
            {
                var weekStart = date.AddDays(-(int)date.DayOfWeek + (int)DayOfWeek.Monday);
                if (weekStart > date)
                    weekStart = weekStart.AddDays(-7); // Handle Sunday edge case
                return weekStart;
            })
            .Select(g => new { WeekStart = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.WeekStart)
            .ToList();
        
        if (!weekGroups.Any())
            return 0;
        
        var streak = 0;
        var expectedWeekStart = currentWeekStart;
        
        foreach (var week in weekGroups)
        {
            if (week.WeekStart == expectedWeekStart && week.Count >= 3)
            {
                streak++;
                expectedWeekStart = expectedWeekStart.AddDays(-7);
            }
            else if (week.WeekStart < expectedWeekStart)
            {
                // We've gone past the expected week, check if we need to break
                break;
            }
        }
        
        return streak;
    }
    
    /// <summary>
    /// Gets the longest weekly streak ever achieved for rite of passage practices
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>Longest streak of consecutive weeks with 3+ practices</returns>
    public async Task<int> GetLongestWeeklyStreakAsync(int userId)
    {
        var entries = await _context.RiteOfPassagePracticeEntries
            .Where(rpe => rpe.UserId == userId)
            .OrderBy(rpe => rpe.Date)
            .Select(rpe => rpe.Date.Date)
            .ToListAsync();
        
        if (!entries.Any())
            return 0;
        
        // Group entries by week
        var weekGroups = entries
            .GroupBy(date => 
            {
                var weekStart = date.AddDays(-(int)date.DayOfWeek + (int)DayOfWeek.Monday);
                if (weekStart > date)
                    weekStart = weekStart.AddDays(-7); // Handle Sunday edge case
                return weekStart;
            })
            .Where(g => g.Count() >= 3) // Only consider weeks with 3+ practices
            .Select(g => g.Key)
            .OrderBy(weekStart => weekStart)
            .ToList();
        
        if (!weekGroups.Any())
            return 0;
        
        var longestStreak = 1;
        var currentStreak = 1;
        
        for (int i = 1; i < weekGroups.Count; i++)
        {
            if (weekGroups[i] == weekGroups[i - 1].AddDays(7))
            {
                currentStreak++;
                longestStreak = Math.Max(longestStreak, currentStreak);
            }
            else
            {
                currentStreak = 1;
            }
        }
        
        return longestStreak;
    }
}