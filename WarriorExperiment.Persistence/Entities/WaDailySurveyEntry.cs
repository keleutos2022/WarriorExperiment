using WarriorExperiment.Persistence.Enums;

namespace WarriorExperiment.Persistence.Entities;

/// <summary>
/// Represents a daily wellness survey entry
/// </summary>
public class WaDailySurveyEntry : WaEntryEntity
{
    /// <summary>
    /// Gets or sets the sleep quality rating (1-10)
    /// </summary>
    public int SleepQuality { get; set; }
    
    /// <summary>
    /// Gets or sets the energy level rating (1-10)
    /// </summary>
    public int Energy { get; set; }
    
    /// <summary>
    /// Gets or sets the mood rating (1-10)
    /// </summary>
    public int Mood { get; set; }
    
    /// <summary>
    /// Gets or sets the muscle soreness rating (1-10)
    /// </summary>
    public int MuscleSoreness { get; set; }
    
    /// <summary>
    /// Gets or sets the bowel movement time
    /// </summary>
    public WaBowelMovementTime BowelMovement { get; set; }
    
    /// <summary>
    /// Gets or sets the stress level rating (1-10)
    /// </summary>
    public int StressLevel { get; set; }
    
    /// <summary>
    /// Gets or sets the hunger feeling during undereating phase rating (1-10)
    /// </summary>
    public int HungerFeelingDuringUndereatingPhase { get; set; }
    
    /// <summary>
    /// Gets or sets optional comments
    /// </summary>
    public string? Comment { get; set; }
    
    /// <summary>
    /// Gets or sets three things the user is grateful for
    /// </summary>
    public string? GratefulFor { get; set; }
    
    /// <summary>
    /// Gets or sets what the user did today that was fun
    /// </summary>
    public string? FunActivity { get; set; }
    
    /// <summary>
    /// Gets or sets what the user is looking forward to tomorrow
    /// </summary>
    public string? LookingForwardTo { get; set; }
    
    /// <summary>
    /// Gets or sets what went well today (Where did I act virtuously?)
    /// </summary>
    public string? WhatWentWell { get; set; }
    
    /// <summary>
    /// Gets or sets what didn't go so well (Where did I deviate from my principles?)
    /// </summary>
    public string? WhatDidNotGoWell { get; set; }
    
    /// <summary>
    /// Gets or sets what will be done differently tomorrow (How can I improve?)
    /// </summary>
    public string? WhatToChangeTomorrow { get; set; }
    
    /// <summary>
    /// Gets or sets the user ID (foreign key)
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Gets or sets the navigation property to the user
    /// </summary>
    public WaUser User { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the collection of task entries for this daily survey
    /// </summary>
    public ICollection<WaDailyTaskEntry> TaskEntries { get; set; } = new List<WaDailyTaskEntry>();
}