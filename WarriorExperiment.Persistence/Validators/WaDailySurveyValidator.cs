using FluentValidation;
using WarriorExperiment.Persistence.Entities;

namespace WarriorExperiment.Persistence.Validators;

/// <summary>
/// Validator for WaDailySurveyEntry entity
/// </summary>
public class WaDailySurveyValidator : AbstractValidator<WaDailySurveyEntry>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WaDailySurveyValidator"/> class
    /// </summary>
    public WaDailySurveyValidator()
    {
        // Date validation
        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required")
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Date cannot be in the future");
        
        // Rating fields validation (1-10 scale)
        RuleFor(x => x.SleepQuality)
            .InclusiveBetween(1, 10).WithMessage("Sleep quality must be between 1 and 10");
        
        RuleFor(x => x.Energy)
            .InclusiveBetween(1, 10).WithMessage("Energy level must be between 1 and 10");
        
        RuleFor(x => x.Mood)
            .InclusiveBetween(1, 10).WithMessage("Mood rating must be between 1 and 10");
        
        RuleFor(x => x.MuscleSoreness)
            .InclusiveBetween(1, 10).WithMessage("Muscle soreness must be between 1 and 10");
        
        RuleFor(x => x.StressLevel)
            .InclusiveBetween(1, 10).WithMessage("Stress level must be between 1 and 10");
        
        RuleFor(x => x.HungerFeelingDuringUndereatingPhase)
            .InclusiveBetween(1, 10).WithMessage("Hunger feeling must be between 1 and 10");
        
        // BowelMovement validation
        RuleFor(x => x.BowelMovement)
            .IsInEnum().WithMessage("Invalid bowel movement time");
        
        // Comment validation (optional)
        RuleFor(x => x.Comment)
            .MaximumLength(1000).WithMessage("Comment cannot exceed 1000 characters");
        
        // UserId validation
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("Valid user ID is required");
    }
}