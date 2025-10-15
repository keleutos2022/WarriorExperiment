namespace WarriorExperiment.Persistence.Models;

/// <summary>
/// Represents a variety practice session with multiple exercises
/// </summary>
public class WaVarietyPracticeEntry : WaBase
{
    /// <summary>
    /// Gets or sets the user ID (foreign key)
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Gets or sets the date of the practice
    /// </summary>
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Gets or sets optional notes about the practice session
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Gets or sets the navigation property to the user
    /// </summary>
    public WaUser User { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the collection of exercises in this practice session
    /// </summary>
    public ICollection<WaExercise> Exercises { get; set; } = new List<WaExercise>();
}