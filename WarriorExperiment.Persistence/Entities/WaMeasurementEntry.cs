namespace WarriorExperiment.Persistence.Entities;

/// <summary>
/// Represents a body measurement entry with photos and various metrics
/// </summary>
public class WaMeasurementEntry : WaEntryEntity
{
    /// <summary>
    /// Gets or sets the user ID (foreign key)
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Gets or sets the measurement method ID (foreign key)
    /// </summary>
    public int MeasurementMethodId { get; set; }
    
    // Photos (stored as Base64)
    /// <summary>
    /// Gets or sets the front photo as Base64 string
    /// </summary>
    public string? FrontPhoto { get; set; }
    
    /// <summary>
    /// Gets or sets the back photo as Base64 string
    /// </summary>
    public string? BackPhoto { get; set; }
    
    /// <summary>
    /// Gets or sets the side photo as Base64 string
    /// </summary>
    public string? SidePhoto { get; set; }
    
    // Body composition measurements
    /// <summary>
    /// Gets or sets the weight in kilograms
    /// </summary>
    public decimal? Weight { get; set; }
    
    /// <summary>
    /// Gets or sets the body fat percentage
    /// </summary>
    public decimal? BodyFat { get; set; }
    
    /// <summary>
    /// Gets or sets the muscle mass percentage from Omron scale
    /// </summary>
    public decimal? MuscleMassPercentage { get; set; }
    
    /// <summary>
    /// Gets or sets the calculated muscle mass in kilograms
    /// </summary>
    public decimal? MuscleMass { get; set; }
    
    /// <summary>
    /// Gets or sets the BMI (Body Mass Index)
    /// </summary>
    public decimal? BMI { get; set; }
    
    /// <summary>
    /// Gets or sets the visceral fat level
    /// </summary>
    public int? VisceralFat { get; set; }
    
    /// <summary>
    /// Gets or sets the metabolic age in years
    /// </summary>
    public int? MetabolicAge { get; set; }
    
    /// <summary>
    /// Gets or sets the basal metabolic rate in calories
    /// </summary>
    public int? BasalMetabolicRate { get; set; }
    
    // Body circumference measurements (in centimeters)
    /// <summary>
    /// Gets or sets the chest circumference in cm
    /// </summary>
    public decimal? ChestCircumference { get; set; }
    
    /// <summary>
    /// Gets or sets the waist circumference in cm
    /// </summary>
    public decimal? WaistCircumference { get; set; }
    
    /// <summary>
    /// Gets or sets the hip circumference in cm
    /// </summary>
    public decimal? HipCircumference { get; set; }
    
    /// <summary>
    /// Gets or sets the bicep circumference in cm
    /// </summary>
    public decimal? BicepCircumference { get; set; }
    
    /// <summary>
    /// Gets or sets the thigh circumference in cm
    /// </summary>
    public decimal? ThighCircumference { get; set; }
    
    /// <summary>
    /// Gets or sets the calf circumference in cm
    /// </summary>
    public decimal? CalfCircumference { get; set; }
    
    /// <summary>
    /// Gets or sets optional notes about the measurement
    /// </summary>
    public string? Notes { get; set; }
    
    // Navigation properties
    /// <summary>
    /// Gets or sets the navigation property to the user
    /// </summary>
    public WaUser User { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the navigation property to the measurement method
    /// </summary>
    public WaMeasurementMethod MeasurementMethod { get; set; } = null!;
    
    /// <summary>
    /// Calculates muscle mass in kg from percentage and weight
    /// </summary>
    /// <returns>Muscle mass in kg, or null if data is incomplete</returns>
    public decimal? CalculateMuscleMassKg()
    {
        if (Weight.HasValue && MuscleMassPercentage.HasValue)
        {
            return Weight.Value * MuscleMassPercentage.Value / 100;
        }
        return null;
    }
    
    /// <summary>
    /// Gets the visceral fat category based on the level
    /// </summary>
    /// <returns>Category description</returns>
    public string GetVisceralFatCategory()
    {
        if (!VisceralFat.HasValue) return "Unknown";
        
        return VisceralFat.Value switch
        {
            >= 1 and <= 9 => "Normal",
            >= 10 and <= 14 => "High", 
            >= 15 and <= 30 => "Very High",
            _ => "Out of Range"
        };
    }
    
    /// <summary>
    /// Gets the color associated with the visceral fat category
    /// </summary>
    /// <returns>CSS color class or hex color</returns>
    public string GetVisceralFatCategoryColor()
    {
        if (!VisceralFat.HasValue) return "#6c757d"; // gray
        
        return VisceralFat.Value switch
        {
            >= 1 and <= 9 => "#28a745", // green
            >= 10 and <= 14 => "#ffc107", // orange
            >= 15 and <= 30 => "#dc3545", // red
            _ => "#6c757d" // gray
        };
    }
}