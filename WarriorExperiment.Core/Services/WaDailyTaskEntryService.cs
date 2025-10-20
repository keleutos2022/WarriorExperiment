using Microsoft.EntityFrameworkCore;
using WarriorExperiment.Persistence.Data;
using WarriorExperiment.Persistence.Entities;

namespace WarriorExperiment.Core.Services;

/// <summary>
/// Service for managing daily task entries (task completion tracking)
/// </summary>
public class WaDailyTaskEntryService
{
    private readonly WaDbContext _context;
    private readonly WaDailyTaskService _dailyTaskService;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="WaDailyTaskEntryService"/> class
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="dailyTaskService">The daily task service</param>
    public WaDailyTaskEntryService(WaDbContext context, WaDailyTaskService dailyTaskService)
    {
        _context = context;
        _dailyTaskService = dailyTaskService;
    }
    
    /// <summary>
    /// Gets all task entries for a specific daily survey
    /// </summary>
    /// <param name="dailySurveyEntryId">The daily survey entry ID</param>
    /// <returns>List of task entries with task details</returns>
    public async Task<List<WaDailyTaskEntry>> GetByDailySurveyIdAsync(int dailySurveyEntryId)
    {
        return await _context.DailyTaskEntries
            .Include(dte => dte.DailyTask)
            .Where(dte => dte.DailySurveyEntryId == dailySurveyEntryId)
            .OrderBy(dte => dte.DailyTask.SortOrder)
            .ThenBy(dte => dte.DailyTask.Name)
            .ToListAsync();
    }
    
    /// <summary>
    /// Creates or updates task entries for a daily survey based on active tasks
    /// </summary>
    /// <param name="dailySurveyEntryId">The daily survey entry ID</param>
    /// <returns>List of created/updated task entries</returns>
    public async Task<List<WaDailyTaskEntry>> EnsureTaskEntriesAsync(int dailySurveyEntryId)
    {
        // Get all active tasks
        var activeTasks = await _dailyTaskService.GetActiveAsync();
        
        // Get existing task entries for this survey
        var existingEntries = await GetByDailySurveyIdAsync(dailySurveyEntryId);
        
        var result = new List<WaDailyTaskEntry>();
        
        foreach (var task in activeTasks)
        {
            var existingEntry = existingEntries.FirstOrDefault(e => e.DailyTaskId == task.Id);
            
            if (existingEntry == null)
            {
                // Create new entry
                var newEntry = new WaDailyTaskEntry
                {
                    DailyTaskId = task.Id,
                    DailySurveyEntryId = dailySurveyEntryId,
                    Done = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    EnteredAt = DateTime.UtcNow
                };
                
                _context.DailyTaskEntries.Add(newEntry);
                result.Add(newEntry);
            }
            else
            {
                result.Add(existingEntry);
            }
        }
        
        // Remove entries for tasks that are no longer active
        var inactiveEntries = existingEntries
            .Where(e => !activeTasks.Any(t => t.Id == e.DailyTaskId))
            .ToList();
            
        if (inactiveEntries.Any())
        {
            _context.DailyTaskEntries.RemoveRange(inactiveEntries);
        }
        
        await _context.SaveChangesAsync();
        
        // Reload with task details
        return await GetByDailySurveyIdAsync(dailySurveyEntryId);
    }
    
    /// <summary>
    /// Updates the completion status of a task entry
    /// </summary>
    /// <param name="taskEntryId">The task entry ID</param>
    /// <param name="done">The completion status</param>
    /// <returns>The updated task entry</returns>
    public async Task<WaDailyTaskEntry?> UpdateCompletionAsync(int taskEntryId, bool done)
    {
        var entry = await _context.DailyTaskEntries
            .Include(dte => dte.DailyTask)
            .FirstOrDefaultAsync(dte => dte.Id == taskEntryId);
            
        if (entry == null)
            return null;
            
        entry.Done = done;
        entry.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        return entry;
    }
    
    /// <summary>
    /// Bulk updates task completion statuses
    /// </summary>
    /// <param name="updates">Dictionary of task entry ID to completion status</param>
    /// <returns>List of updated task entries</returns>
    public async Task<List<WaDailyTaskEntry>> BulkUpdateCompletionAsync(Dictionary<int, bool> updates)
    {
        var entryIds = updates.Keys.ToList();
        var entries = await _context.DailyTaskEntries
            .Include(dte => dte.DailyTask)
            .Where(dte => entryIds.Contains(dte.Id))
            .ToListAsync();
            
        foreach (var entry in entries)
        {
            if (updates.ContainsKey(entry.Id))
            {
                entry.Done = updates[entry.Id];
                entry.UpdatedAt = DateTime.UtcNow;
            }
        }
        
        await _context.SaveChangesAsync();
        
        return entries;
    }
    
    /// <summary>
    /// Gets task completion statistics for a user within a date range
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <returns>Dictionary of task ID to completion percentage</returns>
    public async Task<Dictionary<int, double>> GetCompletionStatsAsync(int userId, DateTime startDate, DateTime endDate)
    {
        var stats = await _context.DailyTaskEntries
            .Include(dte => dte.DailySurveyEntry)
            .Include(dte => dte.DailyTask)
            .Where(dte => dte.DailySurveyEntry.UserId == userId 
                       && dte.DailySurveyEntry.Date >= startDate 
                       && dte.DailySurveyEntry.Date <= endDate)
            .GroupBy(dte => dte.DailyTaskId)
            .Select(g => new
            {
                TaskId = g.Key,
                CompletionRate = g.Count() > 0 ? (double)g.Count(dte => dte.Done) / g.Count() : 0.0
            })
            .ToListAsync();
            
        return stats.ToDictionary(s => s.TaskId, s => s.CompletionRate);
    }
    
    /// <summary>
    /// Deletes all task entries for a specific daily survey
    /// </summary>
    /// <param name="dailySurveyEntryId">The daily survey entry ID</param>
    /// <returns>Number of deleted entries</returns>
    public async Task<int> DeleteByDailySurveyIdAsync(int dailySurveyEntryId)
    {
        var entries = await _context.DailyTaskEntries
            .Where(dte => dte.DailySurveyEntryId == dailySurveyEntryId)
            .ToListAsync();
            
        if (entries.Any())
        {
            _context.DailyTaskEntries.RemoveRange(entries);
            await _context.SaveChangesAsync();
        }
        
        return entries.Count;
    }
    
    /// <summary>
    /// Updates reflection questions for a daily survey entry
    /// </summary>
    /// <param name="dailySurveyEntryId">The daily survey entry ID</param>
    /// <param name="gratefulFor">What the user is grateful for</param>
    /// <param name="funActivity">What fun activity the user did</param>
    /// <param name="lookingForwardTo">What the user is looking forward to</param>
    /// <returns>The updated daily survey entry</returns>
    public async Task<WaDailySurveyEntry?> UpdateReflectionQuestionsAsync(
        int dailySurveyEntryId, 
        string? gratefulFor, 
        string? funActivity, 
        string? lookingForwardTo)
    {
        var dailySurvey = await _context.DailySurveys
            .FirstOrDefaultAsync(ds => ds.Id == dailySurveyEntryId);
            
        if (dailySurvey == null)
            return null;
            
        dailySurvey.GratefulFor = gratefulFor;
        dailySurvey.FunActivity = funActivity;
        dailySurvey.LookingForwardTo = lookingForwardTo;
        dailySurvey.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        return dailySurvey;
    }
    
    /// <summary>
    /// Gets reflection questions for a daily survey entry
    /// </summary>
    /// <param name="dailySurveyEntryId">The daily survey entry ID</param>
    /// <returns>Tuple containing the three reflection answers</returns>
    public async Task<(string? GratefulFor, string? FunActivity, string? LookingForwardTo)?> GetReflectionQuestionsAsync(int dailySurveyEntryId)
    {
        var dailySurvey = await _context.DailySurveys
            .FirstOrDefaultAsync(ds => ds.Id == dailySurveyEntryId);
            
        if (dailySurvey == null)
            return null;
            
        return (dailySurvey.GratefulFor, dailySurvey.FunActivity, dailySurvey.LookingForwardTo);
    }
}