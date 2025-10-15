namespace WarriorExperiment.App.Components.Grids.Helpers;

/// <summary>
/// Helper class for mapping scale values to colors for visual feedback
/// </summary>
public static class ScaleColorHelper
{
    /// <summary>
    /// Gets the background color for a scale value (1-10)
    /// </summary>
    /// <param name="value">The scale value (1-10)</param>
    /// <returns>Hex color code for the background</returns>
    public static string GetColor(double value)
    {
        return value switch
        {
            >= 9 => "#198754",   // Dark Green - Excellent
            >= 8 => "#28a745",   // Light Green - Good  
            >= 6 => "#ffc107",   // Yellow - Average
            >= 4 => "#fd7e14",   // Orange - Below Average
            _ => "#dc3545"       // Red - Poor
        };
    }
    
    /// <summary>
    /// Gets the text color for optimal contrast with the background color
    /// </summary>
    /// <param name="value">The scale value (1-10)</param>
    /// <returns>Hex color code for the text</returns>
    public static string GetTextColor(double value)
    {
        // Use white text for darker backgrounds (green), black for lighter (yellow/orange/red)
        return value >= 8 ? "#fff" : "#000";
    }
    
    /// <summary>
    /// Gets a CSS style string for scale value display
    /// </summary>
    /// <param name="value">The scale value (1-10)</param>
    /// <returns>CSS style string with background and text color</returns>
    public static string GetStyle(double value)
    {
        return $"background-color: {GetColor(value)}; color: {GetTextColor(value)}; padding: 4px 8px; border-radius: 4px; text-align: center; font-weight: 500;";
    }
    
    /// <summary>
    /// Gets a descriptive label for the scale value
    /// </summary>
    /// <param name="value">The scale value (1-10)</param>
    /// <returns>Descriptive text for the value</returns>
    public static string GetLabel(double value)
    {
        return value switch
        {
            >= 9 => "Excellent",
            >= 8 => "Good",
            >= 6 => "Average",
            >= 4 => "Below Average",
            _ => "Poor"
        };
    }
}