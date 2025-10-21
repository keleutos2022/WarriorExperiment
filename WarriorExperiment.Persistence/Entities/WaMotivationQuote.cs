namespace WarriorExperiment.Persistence.Entities;

/// <summary>
/// Represents a motivational quote to inspire users
/// </summary>
public class WaMotivationQuote : WaBaseEntity
{
    /// <summary>
    /// Gets or sets the author of the quote
    /// </summary>
    public string Author { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the quote text
    /// </summary>
    public string Quote { get; set; } = string.Empty;
}