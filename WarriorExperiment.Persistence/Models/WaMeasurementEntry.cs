namespace WarriorExperiment.Persistence.Models;

/// <summary>
/// Represents a body measurement entry with photos and various metrics
/// </summary>
public class WaMeasurementEntry : WaBase
{
    /// <summary>
    /// Gets or sets the user ID (foreign key)
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Gets or sets the measurement method ID (foreign key)
    /// </summary>
    public int MeasurementMethodId { get; set; }
    
    /// <summary>
    /// Gets or sets the date of the measurement
    /// </summary>
    public DateTime Date { get; set; }
    
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
    /// Gets or sets the muscle mass in kilograms
    /// </summary>
    public decimal? MuscleMass { get; set; }
    
    /// <summary>
    /// Gets or sets the water percentage
    /// </summary>
    public decimal? WaterPercentage { get; set; }
    
    /// <summary>
    /// Gets or sets the bone mass in kilograms
    /// </summary>
    public decimal? BoneMass { get; set; }
    
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
}