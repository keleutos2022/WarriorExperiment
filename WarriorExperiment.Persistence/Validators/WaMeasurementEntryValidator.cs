using FluentValidation;
using WarriorExperiment.Persistence.Models;

namespace WarriorExperiment.Persistence.Validators;

/// <summary>
/// Validator for WaMeasurementEntry entity
/// </summary>
public class WaMeasurementEntryValidator : AbstractValidator<WaMeasurementEntry>
{
    private const int MaxBase64Length = 6_990_507; // ~5MB when decoded (5MB * 4/3 for base64)
    
    /// <summary>
    /// Initializes a new instance of the <see cref="WaMeasurementEntryValidator"/> class
    /// </summary>
    public WaMeasurementEntryValidator()
    {
        // Foreign key validation
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("Valid user ID is required");
        
        RuleFor(x => x.MeasurementMethodId)
            .GreaterThan(0).WithMessage("Valid measurement method ID is required");
        
        // Date validation
        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required")
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Date cannot be in the future");
        
        // Photo validations (optional, max 5MB each)
        RuleFor(x => x.FrontPhoto)
            .Must(BeValidBase64Image).When(x => !string.IsNullOrEmpty(x.FrontPhoto))
            .WithMessage("Front photo must be a valid Base64 image (max 5MB)");
        
        RuleFor(x => x.BackPhoto)
            .Must(BeValidBase64Image).When(x => !string.IsNullOrEmpty(x.BackPhoto))
            .WithMessage("Back photo must be a valid Base64 image (max 5MB)");
        
        RuleFor(x => x.SidePhoto)
            .Must(BeValidBase64Image).When(x => !string.IsNullOrEmpty(x.SidePhoto))
            .WithMessage("Side photo must be a valid Base64 image (max 5MB)");
        
        // Body composition measurements (all optional)
        RuleFor(x => x.Weight)
            .InclusiveBetween(20m, 500m).When(x => x.Weight.HasValue)
            .WithMessage("Weight must be between 20 and 500 kg");
        
        RuleFor(x => x.BodyFat)
            .InclusiveBetween(1m, 80m).When(x => x.BodyFat.HasValue)
            .WithMessage("Body fat percentage must be between 1 and 80%");
        
        RuleFor(x => x.MuscleMass)
            .InclusiveBetween(10m, 150m).When(x => x.MuscleMass.HasValue)
            .WithMessage("Muscle mass must be between 10 and 150 kg");
        
        RuleFor(x => x.WaterPercentage)
            .InclusiveBetween(20m, 80m).When(x => x.WaterPercentage.HasValue)
            .WithMessage("Water percentage must be between 20 and 80%");
        
        RuleFor(x => x.BoneMass)
            .InclusiveBetween(0.5m, 10m).When(x => x.BoneMass.HasValue)
            .WithMessage("Bone mass must be between 0.5 and 10 kg");
        
        RuleFor(x => x.BMI)
            .InclusiveBetween(10m, 60m).When(x => x.BMI.HasValue)
            .WithMessage("BMI must be between 10 and 60");
        
        RuleFor(x => x.VisceralFat)
            .InclusiveBetween(1, 60).When(x => x.VisceralFat.HasValue)
            .WithMessage("Visceral fat level must be between 1 and 60");
        
        RuleFor(x => x.MetabolicAge)
            .InclusiveBetween(10, 100).When(x => x.MetabolicAge.HasValue)
            .WithMessage("Metabolic age must be between 10 and 100 years");
        
        RuleFor(x => x.BasalMetabolicRate)
            .InclusiveBetween(500, 5000).When(x => x.BasalMetabolicRate.HasValue)
            .WithMessage("Basal metabolic rate must be between 500 and 5000 calories");
        
        // Circumference measurements (all optional, in cm)
        RuleFor(x => x.ChestCircumference)
            .InclusiveBetween(50m, 200m).When(x => x.ChestCircumference.HasValue)
            .WithMessage("Chest circumference must be between 50 and 200 cm");
        
        RuleFor(x => x.WaistCircumference)
            .InclusiveBetween(40m, 200m).When(x => x.WaistCircumference.HasValue)
            .WithMessage("Waist circumference must be between 40 and 200 cm");
        
        RuleFor(x => x.HipCircumference)
            .InclusiveBetween(50m, 200m).When(x => x.HipCircumference.HasValue)
            .WithMessage("Hip circumference must be between 50 and 200 cm");
        
        RuleFor(x => x.BicepCircumference)
            .InclusiveBetween(15m, 60m).When(x => x.BicepCircumference.HasValue)
            .WithMessage("Bicep circumference must be between 15 and 60 cm");
        
        RuleFor(x => x.ThighCircumference)
            .InclusiveBetween(30m, 100m).When(x => x.ThighCircumference.HasValue)
            .WithMessage("Thigh circumference must be between 30 and 100 cm");
        
        RuleFor(x => x.CalfCircumference)
            .InclusiveBetween(20m, 60m).When(x => x.CalfCircumference.HasValue)
            .WithMessage("Calf circumference must be between 20 and 60 cm");
        
        // Notes validation (optional)
        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters");
    }
    
    /// <summary>
    /// Validates that a Base64 string is a valid image and within size limits
    /// </summary>
    private bool BeValidBase64Image(string? base64String)
    {
        if (string.IsNullOrEmpty(base64String))
            return true;
        
        // Check length (max ~5MB)
        if (base64String.Length > MaxBase64Length)
            return false;
        
        try
        {
            // Check if it's valid Base64
            var bytes = Convert.FromBase64String(base64String);
            return bytes.Length > 0;
        }
        catch
        {
            return false;
        }
    }
}