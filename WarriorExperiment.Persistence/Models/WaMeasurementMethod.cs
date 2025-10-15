namespace WarriorExperiment.Persistence.Models;

/// <summary>
/// Represents a measurement method or scale type used for body measurements
/// </summary>
public class WaMeasurementMethod : WaBase
{
    /// <summary>
    /// Gets or sets the name of the measurement method
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets an optional description of the measurement method
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Gets or sets the collection of measurement entries using this method
    /// </summary>
    public ICollection<WaMeasurementEntry> MeasurementEntries { get; set; } = new List<WaMeasurementEntry>();
}