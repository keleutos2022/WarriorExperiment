namespace WarriorExperiment.Persistence.Entities;

/// <summary>
/// Base entity for all domain models
/// </summary>
public abstract class WaBaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier
    /// </summary>
    public int Id { get; set; }
    
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
}