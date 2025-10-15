using WarriorExperiment.Core.Enums;

namespace WarriorExperiment.Core.Interfaces;

/// <summary>
/// Common interface for entries that can be displayed on a calendar
/// </summary>
public interface IWaCalendarEntry
{
    /// <summary>
    /// Gets the unique identifier for the entry
    /// </summary>
    int Id { get; }
    
    /// <summary>
    /// Gets the date/time for the calendar entry
    /// </summary>
    DateTime Date { get; }
    
    /// <summary>
    /// Gets the title to display on the calendar
    /// </summary>
    string Title { get; }
    
    /// <summary>
    /// Gets the description or summary for the calendar entry
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// Gets the entry type for color coding and filtering
    /// </summary>
    WaEntryType EntryType { get; }
    
    /// <summary>
    /// Gets the user ID associated with this entry
    /// </summary>
    int UserId { get; }
}