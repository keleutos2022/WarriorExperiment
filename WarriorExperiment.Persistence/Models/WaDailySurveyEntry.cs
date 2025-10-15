using WarriorExperiment.Persistence.Enums;

namespace WarriorExperiment.Persistence.Models;

/// <summary>
/// Represents a daily wellness survey entry
/// </summary>
public class WaDailySurveyEntry : WaBase
{
    /// <summary>
    /// Gets or sets the date of the survey
    /// </summary>
    public DateTime Date { get; set; }
    
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
    /// Gets or sets the user ID (foreign key)
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Gets or sets the navigation property to the user
    /// </summary>
    public WaUser User { get; set; } = null!;
}