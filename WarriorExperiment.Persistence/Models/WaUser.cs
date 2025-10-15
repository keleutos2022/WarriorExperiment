namespace WarriorExperiment.Persistence.Models;

/// <summary>
/// Represents a user in the system
/// </summary>
public class WaUser : WaBase
{
    /// <summary>
    /// Gets or sets the username
    /// </summary>
    public string UserName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the user's height in centimeters
    /// </summary>
    public decimal? Height { get; set; }
    
    /// <summary>
    /// Gets or sets the user's birth date
    /// </summary>
    public DateTime? BirthDate { get; set; }
    
    /// <summary>
    /// Gets or sets the date when the user started the program
    /// </summary>
    public DateTime? DateOfStart { get; set; }
    
    /// <summary>
    /// Gets or sets whether this user is the default user
    /// </summary>
    public bool IsDefault { get; set; }
    
    /// <summary>
    /// Gets or sets the collection of daily surveys for this user
    /// </summary>
    public ICollection<WaDailySurveyEntry> DailySurveys { get; set; } = new List<WaDailySurveyEntry>();
    
    /// <summary>
    /// Gets or sets the collection of measurement entries for this user
    /// </summary>
    public ICollection<WaMeasurementEntry> MeasurementEntries { get; set; } = new List<WaMeasurementEntry>();
    
    /// <summary>
    /// Gets or sets the collection of Rite of Passage practice entries for this user
    /// </summary>
    public ICollection<WaRiteOfPassagePracticeEntry> RiteOfPassagePracticeEntries { get; set; } = new List<WaRiteOfPassagePracticeEntry>();
    
    /// <summary>
    /// Gets or sets the collection of variety practices for this user
    /// </summary>
    public ICollection<WaVarietyPracticeEntry> VarietyPractices { get; set; } = new List<WaVarietyPracticeEntry>();
}