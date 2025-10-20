namespace WarriorExperiment.Persistence.Entities;

/// <summary>
/// Base entity for all entry-type domain models
/// </summary>
public abstract class WaEntryEntity : WaBaseEntity
{
    /// <summary>
    /// Gets or sets the date of the entry
    /// </summary>
    public DateTime Date { get; set; }
}