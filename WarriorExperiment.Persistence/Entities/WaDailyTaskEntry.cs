namespace WarriorExperiment.Persistence.Entities;

/// <summary>
/// Represents the completion status of a daily task for a specific daily survey entry
/// </summary>
public class WaDailyTaskEntry : WaBaseEntity
{
    /// <summary>
    /// Gets or sets the daily task ID (foreign key)
    /// </summary>
    public int DailyTaskId { get; set; }
    
    /// <summary>
    /// Gets or sets the daily survey entry ID (foreign key)
    /// </summary>
    public int DailySurveyEntryId { get; set; }
    
    /// <summary>
    /// Gets or sets whether the task was completed
    /// </summary>
    public bool Done { get; set; }
    
    /// <summary>
    /// Gets or sets the navigation property to the daily task
    /// </summary>
    public WaDailyTask DailyTask { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the navigation property to the daily survey entry
    /// </summary>
    public WaDailySurveyEntry DailySurveyEntry { get; set; } = null!;
}