using Microsoft.AspNetCore.Identity;

namespace WarriorExperiment.Persistence.Entities;

/// <summary>
/// Represents a user in the system
/// </summary>
public class WaUser : IdentityUser<int>
{
    /// <summary>
    /// Gets or sets the display name for the user
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
    
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
    /// Gets or sets the date when the user accepted the rules of engagement
    /// </summary>
    public DateTime? RulesAcceptedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the name the user typed to confirm acceptance of the rules
    /// </summary>
    public string? RulesAcceptedName { get; set; }
    
    /// <summary>
    /// Gets or sets whether this user is the default user
    /// </summary>
    public bool IsDefault { get; set; }
    
    /// <summary>
    /// Gets or sets the creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the last update timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets when this entry was actually entered into the system
    /// </summary>
    public DateTime EnteredAt { get; set; }
    
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