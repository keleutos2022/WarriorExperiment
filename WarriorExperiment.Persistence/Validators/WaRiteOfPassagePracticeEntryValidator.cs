using FluentValidation;
using WarriorExperiment.Persistence.Models;

namespace WarriorExperiment.Persistence.Validators;

/// <summary>
/// Validator for WaRiteOfPassagePracticeEntry entity
/// </summary>
public class WaRiteOfPassagePracticeEntryValidator : AbstractValidator<WaRiteOfPassagePracticeEntry>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WaRiteOfPassagePracticeEntryValidator"/> class
    /// </summary>
    public WaRiteOfPassagePracticeEntryValidator()
    {
        // User validation
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("Valid user ID is required");
        
        // Practice intensity validation
        RuleFor(x => x.PracticeIntensity)
            .IsInEnum().WithMessage("Invalid practice intensity");
        
        // Ladder sets validation (all must be >= 0)
        RuleFor(x => x.Ladder1Sets)
            .GreaterThanOrEqualTo(0).WithMessage("Ladder 1 sets must be 0 or greater");
        
        RuleFor(x => x.Ladder2Sets)
            .GreaterThanOrEqualTo(0).WithMessage("Ladder 2 sets must be 0 or greater");
        
        RuleFor(x => x.Ladder3Sets)
            .GreaterThanOrEqualTo(0).WithMessage("Ladder 3 sets must be 0 or greater");
        
        RuleFor(x => x.Ladder4Sets)
            .GreaterThanOrEqualTo(0).WithMessage("Ladder 4 sets must be 0 or greater");
        
        RuleFor(x => x.Ladder5Sets)
            .GreaterThanOrEqualTo(0).WithMessage("Ladder 5 sets must be 0 or greater");
        
        // Dice validation (2-12)
        RuleFor(x => x.Dice)
            .InclusiveBetween(2, 12).WithMessage("Dice value must be between 2 and 12");
        
        // Pull count validation
        RuleFor(x => x.PullCount)
            .GreaterThanOrEqualTo(0).WithMessage("Pull count must be 0 or greater");
        
        // Weight validation (optional)
        RuleFor(x => x.Weight)
            .InclusiveBetween(0m, 200m).When(x => x.Weight.HasValue)
            .WithMessage("Weight must be between 0 and 200 kg");
        
        // Date validation
        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required")
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Date cannot be in the future");
        
        // At least one ladder set or pull count should be greater than 0
        RuleFor(x => x)
            .Must(x => x.Ladder1Sets > 0 || x.Ladder2Sets > 0 || x.Ladder3Sets > 0 || 
                      x.Ladder4Sets > 0 || x.Ladder5Sets > 0 || x.PullCount > 0)
            .WithMessage("At least one ladder set or pull count must be greater than 0")
            .WithName("PracticeActivity");
        
        // Ladder progression validation - can't have higher ladder without lower ones
        RuleFor(x => x)
            .Must(x => !(x.Ladder2Sets > 0 && x.Ladder1Sets == 0))
            .WithMessage("Cannot have Ladder 2 sets without Ladder 1 sets")
            .WithName("LadderProgression");
        
        RuleFor(x => x)
            .Must(x => !(x.Ladder3Sets > 0 && (x.Ladder1Sets == 0 || x.Ladder2Sets == 0)))
            .WithMessage("Cannot have Ladder 3 sets without Ladder 1 and 2 sets")
            .WithName("LadderProgression");
        
        RuleFor(x => x)
            .Must(x => !(x.Ladder4Sets > 0 && (x.Ladder1Sets == 0 || x.Ladder2Sets == 0 || x.Ladder3Sets == 0)))
            .WithMessage("Cannot have Ladder 4 sets without Ladder 1, 2, and 3 sets")
            .WithName("LadderProgression");
        
        RuleFor(x => x)
            .Must(x => !(x.Ladder5Sets > 0 && (x.Ladder1Sets == 0 || x.Ladder2Sets == 0 || 
                                               x.Ladder3Sets == 0 || x.Ladder4Sets == 0)))
            .WithMessage("Cannot have Ladder 5 sets without all lower ladder sets")
            .WithName("LadderProgression");
        
        // Validate ladder values follow pattern (all active ladders should have same value)
        RuleFor(x => x)
            .Must(ValidateLadderPattern)
            .WithMessage("Active ladder values should all be the same (e.g., all 3s or all 4s)")
            .WithName("LadderPattern");
    }
    
    /// <summary>
    /// Validates that active ladder values follow the expected pattern
    /// </summary>
    /// <param name="entry">The practice entry to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    private bool ValidateLadderPattern(WaRiteOfPassagePracticeEntry entry)
    {
        // Collect all non-zero ladder values
        var activeValues = new List<int>();
        if (entry.Ladder1Sets > 0) activeValues.Add(entry.Ladder1Sets);
        if (entry.Ladder2Sets > 0) activeValues.Add(entry.Ladder2Sets);
        if (entry.Ladder3Sets > 0) activeValues.Add(entry.Ladder3Sets);
        if (entry.Ladder4Sets > 0) activeValues.Add(entry.Ladder4Sets);
        if (entry.Ladder5Sets > 0) activeValues.Add(entry.Ladder5Sets);
        
        // If no active ladders, it's valid
        if (activeValues.Count == 0) return true;
        
        // All active ladders should have the same value
        return activeValues.All(v => v == activeValues[0]);
    }
}