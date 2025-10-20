namespace WarriorExperiment.Persistence.Enums;

/// <summary>
/// Represents different sections of the day for task scheduling
/// </summary>
public enum WaDaySection
{
    /// <summary>
    /// Morning time (early hours)
    /// </summary>
    Morning = 0,
    
    /// <summary>
    /// Midday time (noon hours)
    /// </summary>
    Midday = 1,
    
    /// <summary>
    /// Evening time (late hours)
    /// </summary>
    Evening = 2,
    
    /// <summary>
    /// Other/unspecified time or flexible timing
    /// </summary>
    Other = 3
}