namespace WarriorExperiment.Persistence.Models;

/// <summary>
/// Represents an individual exercise within a variety practice session
/// </summary>
public class WaExercise : WaBase
{
    /// <summary>
    /// Gets or sets the variety practice ID (foreign key)
    /// </summary>
    public int VarietyPracticeId { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the exercise
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the number of sets performed
    /// </summary>
    public int Sets { get; set; }
    
    /// <summary>
    /// Gets or sets the number of repetitions per set
    /// </summary>
    public int Reps { get; set; }
    
    /// <summary>
    /// Gets or sets the weight used in kilograms (optional)
    /// </summary>
    public decimal? Weight { get; set; }
    
    /// <summary>
    /// Gets or sets the navigation property to the variety practice
    /// </summary>
    public WaVarietyPracticeEntry VarietyPractice { get; set; } = null!;
}