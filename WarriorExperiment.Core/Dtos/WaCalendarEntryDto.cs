using WarriorExperiment.Core.Enums;
using WarriorExperiment.Core.Interfaces;

namespace WarriorExperiment.Core.Dtos;

/// <summary>
/// Data transfer object for calendar entries
/// </summary>
public class WaCalendarEntryDto : IWaCalendarEntry
{
    /// <summary>
    /// Gets or sets the unique identifier for the entry
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Gets or sets the date/time for the calendar entry
    /// </summary>
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Gets or sets the title to display on the calendar
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the description or summary for the calendar entry
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the entry type for color coding and filtering
    /// </summary>
    public WaEntryType EntryType { get; set; }
    
    /// <summary>
    /// Gets or sets the user ID associated with this entry
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Gets or sets additional data specific to the entry type
    /// </summary>
    public object? AdditionalData { get; set; }
}