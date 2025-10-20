using FluentValidation;
using WarriorExperiment.Persistence.Entities;

namespace WarriorExperiment.Persistence.Validators;

/// <summary>
/// Validator for WaVarietyPracticeEntry entity
/// </summary>
public class WaVarietyPracticeValidator : AbstractValidator<WaVarietyPracticeEntry>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WaVarietyPracticeValidator"/> class
    /// </summary>
    public WaVarietyPracticeValidator()
    {
        // User validation
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("Valid user ID is required");
        
        // Date validation
        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required")
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Date cannot be in the future");
        
        // Notes validation (optional)
        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters");
        
        // Ensure at least one exercise exists
        RuleFor(x => x.Exercises)
            .NotEmpty().WithMessage("At least one exercise must be added to the practice session");
    }
}