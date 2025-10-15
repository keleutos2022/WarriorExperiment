using Microsoft.EntityFrameworkCore;
using WarriorExperiment.Core.Dtos;
using WarriorExperiment.Core.Enums;
using WarriorExperiment.Core.Interfaces;
using WarriorExperiment.Persistence.Data;

namespace WarriorExperiment.Core.Services;

/// <summary>
/// Service for managing and retrieving calendar entries from all entry types
/// </summary>
public class WaEntryService
{
    private readonly WaDbContext _context;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="WaEntryService"/> class
    /// </summary>
    /// <param name="context">The database context</param>
    public WaEntryService(WaDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Gets all calendar entries for a specific user within a date range
    /// </summary>
    /// <param name="userId">The user ID to filter by</param>
    /// <param name="startDate">The start date (inclusive)</param>
    /// <param name="endDate">The end date (inclusive)</param>
    /// <param name="includeTypes">Optional array of entry types to include</param>
    /// <returns>List of calendar entries</returns>
    public async Task<List<IWaCalendarEntry>> GetCalendarEntriesAsync(
        int userId, 
        DateTime startDate, 
        DateTime endDate,
        WaEntryType[]? includeTypes = null)
    {
        var entries = new List<IWaCalendarEntry>();
        
        // Get all types if none specified
        var types = includeTypes ?? Enum.GetValues<WaEntryType>();
        
        // Fetch Daily Surveys
        if (types.Contains(WaEntryType.DailySurvey))
        {
            var dailySurveys = await _context.DailySurveys
                .Where(ds => ds.UserId == userId && 
                             ds.Date >= startDate && 
                             ds.Date <= endDate)
                .Select(ds => new WaCalendarEntryDto
                {
                    Id = ds.Id,
                    Date = ds.Date,
                    Title = "Daily Survey",
                    Description = $"Energy: {ds.Energy}/10, Mood: {ds.Mood}/10, Sleep: {ds.SleepQuality}/10",
                    EntryType = WaEntryType.DailySurvey,
                    UserId = ds.UserId,
                    AdditionalData = new
                    {
                        ds.Energy,
                        ds.Mood,
                        ds.SleepQuality,
                        ds.StressLevel,
                        ds.MuscleSoreness,
                        ds.HungerFeelingDuringUndereatingPhase
                    }
                })
                .ToListAsync();
            
            entries.AddRange(dailySurveys);
        }
        
        // Fetch Measurement Entries
        if (types.Contains(WaEntryType.Measurement))
        {
            var measurements = await _context.MeasurementEntries
                .Include(m => m.MeasurementMethod)
                .Where(m => m.UserId == userId && 
                           m.Date >= startDate && 
                           m.Date <= endDate)
                .Select(m => new WaCalendarEntryDto
                {
                    Id = m.Id,
                    Date = m.Date,
                    Title = $"Measurement - {m.MeasurementMethod.Name}",
                    Description = m.Weight.HasValue ? $"Weight: {m.Weight}kg" : "Body measurements recorded",
                    EntryType = WaEntryType.Measurement,
                    UserId = m.UserId,
                    AdditionalData = new
                    {
                        m.Weight,
                        m.BodyFat,
                        m.MuscleMass,
                        MethodName = m.MeasurementMethod.Name
                    }
                })
                .ToListAsync();
            
            entries.AddRange(measurements);
        }
        
        // Fetch Rite of Passage Practice Entries
        if (types.Contains(WaEntryType.RiteOfPassagePractice))
        {
            var riteOfPassagePractices = await _context.RiteOfPassagePracticeEntries
                .Where(r => r.UserId == userId && 
                           r.Date >= startDate && 
                           r.Date <= endDate)
                .Select(r => new WaCalendarEntryDto
                {
                    Id = r.Id,
                    Date = r.Date,
                    Title = $"Rite of Passage - {r.PracticeIntensity}",
                    Description = $"Pulls: {r.PullCount}, Time: {r.Dice}min, {(r.Success ? "✓" : "✗")}",
                    EntryType = WaEntryType.RiteOfPassagePractice,
                    UserId = r.UserId,
                    AdditionalData = new
                    {
                        r.PracticeIntensity,
                        r.PullCount,
                        r.Dice,
                        r.Success,
                        PullsPerMinute = r.Dice > 0 ? Math.Round((decimal)r.PullCount / r.Dice, 2) : 0
                    }
                })
                .ToListAsync();
            
            entries.AddRange(riteOfPassagePractices);
        }
        
        // Fetch Variety Practice Entries
        if (types.Contains(WaEntryType.VarietyPractice))
        {
            var varietyPractices = await _context.VarietyPractices
                .Include(v => v.Exercises)
                .Where(v => v.UserId == userId && 
                           v.Date >= startDate && 
                           v.Date <= endDate)
                .Select(v => new WaCalendarEntryDto
                {
                    Id = v.Id,
                    Date = v.Date,
                    Title = "Variety Practice",
                    Description = $"{v.Exercises.Count} exercises",
                    EntryType = WaEntryType.VarietyPractice,
                    UserId = v.UserId,
                    AdditionalData = new
                    {
                        ExerciseCount = v.Exercises.Count,
                        Exercises = v.Exercises.Select(e => e.Name).ToList()
                    }
                })
                .ToListAsync();
            
            entries.AddRange(varietyPractices);
        }
        
        return entries.OrderBy(e => e.Date).ToList();
    }
    
    /// <summary>
    /// Gets entry counts by type for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>Dictionary of entry types and their counts</returns>
    public async Task<Dictionary<WaEntryType, int>> GetEntryCountsAsync(int userId)
    {
        var counts = new Dictionary<WaEntryType, int>();
        
        counts[WaEntryType.DailySurvey] = await _context.DailySurveys
            .CountAsync(ds => ds.UserId == userId);
            
        counts[WaEntryType.Measurement] = await _context.MeasurementEntries
            .CountAsync(m => m.UserId == userId);
            
        counts[WaEntryType.RiteOfPassagePractice] = await _context.RiteOfPassagePracticeEntries
            .CountAsync(r => r.UserId == userId);
            
        counts[WaEntryType.VarietyPractice] = await _context.VarietyPractices
            .CountAsync(v => v.UserId == userId);
        
        return counts;
    }
}