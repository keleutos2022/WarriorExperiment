using WarriorExperiment.Persistence.Enums;

namespace WarriorExperiment.Persistence.Entities;

/// <summary>
/// Represents a daily task that can be tracked in daily surveys
/// </summary>
public class WaDailyTask : WaBaseEntity
{
    /// <summary>
    /// Gets or sets the name of the task
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets an optional description of the task
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Gets or sets whether this task is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the sort order for displaying tasks
    /// </summary>
    public int SortOrder { get; set; }
    
    /// <summary>
    /// Gets or sets the preferred time of day for this task
    /// </summary>
    public WaDaySection PreferredTime { get; set; } = WaDaySection.Other;
    
    /// <summary>
    /// Gets or sets the collection of task entries for this task
    /// </summary>
    public ICollection<WaDailyTaskEntry> TaskEntries { get; set; } = new List<WaDailyTaskEntry>();
}