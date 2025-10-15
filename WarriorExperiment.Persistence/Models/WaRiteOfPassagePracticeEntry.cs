using WarriorExperiment.Persistence.Enums;

namespace WarriorExperiment.Persistence.Models;

/// <summary>
/// Represents a Rite of Passage practice session entry
/// </summary>
public class WaRiteOfPassagePracticeEntry : WaBase
{
    /// <summary>
    /// Gets or sets the user ID (foreign key)
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Gets or sets the practice intensity
    /// </summary>
    public WaPracticeIntensity PracticeIntensity { get; set; }
    
    /// <summary>
    /// Gets or sets the number of sets for ladder 1
    /// </summary>
    public int Ladder1Sets { get; set; }
    
    /// <summary>
    /// Gets or sets the number of sets for ladder 2
    /// </summary>
    public int Ladder2Sets { get; set; }
    
    /// <summary>
    /// Gets or sets the number of sets for ladder 3
    /// </summary>
    public int Ladder3Sets { get; set; }
    
    /// <summary>
    /// Gets or sets the number of sets for ladder 4
    /// </summary>
    public int Ladder4Sets { get; set; }
    
    /// <summary>
    /// Gets or sets the number of sets for ladder 5
    /// </summary>
    public int Ladder5Sets { get; set; }
    
    /// <summary>
    /// Gets or sets the dice roll value (2-12) representing time in minutes
    /// </summary>
    public int Dice { get; set; }
    
    /// <summary>
    /// Gets or sets the total number of pulls completed
    /// </summary>
    public int PullCount { get; set; }
    
    /// <summary>
    /// Gets or sets the weight used in kilograms (optional)
    /// </summary>
    public decimal? Weight { get; set; }
    
    /// <summary>
    /// Gets or sets whether the practice was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Gets or sets the date of the practice
    /// </summary>
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Gets the calculated pulls per minute
    /// </summary>
    public decimal PullsPerMinute => Dice > 0 ? Math.Round((decimal)PullCount / Dice, 2) : 0;
    
    /// <summary>
    /// Gets or sets the navigation property to the user
    /// </summary>
    public WaUser User { get; set; } = null!;
}