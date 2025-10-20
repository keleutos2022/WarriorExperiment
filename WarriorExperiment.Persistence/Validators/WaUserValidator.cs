using FluentValidation;
using WarriorExperiment.Persistence.Entities;

namespace WarriorExperiment.Persistence.Validators;

/// <summary>
/// Validator for WaUser entity
/// </summary>
public class WaUserValidator : AbstractValidator<WaUser>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WaUserValidator"/> class
    /// </summary>
    public WaUserValidator()
    {
        // UserName validation
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Username is required")
            .Length(3, 50).WithMessage("Username must be between 3 and 50 characters")
            .Matches(@"^[a-zA-Z0-9_-]+$").WithMessage("Username can only contain letters, numbers, underscores and hyphens");
        
        // Height validation (optional)
        RuleFor(x => x.Height)
            .InclusiveBetween(50, 300)
            .When(x => x.Height.HasValue)
            .WithMessage("Height must be between 50 and 300 cm");
        
        // BirthDate validation (optional)
        RuleFor(x => x.BirthDate)
            .LessThanOrEqualTo(DateTime.Today)
            .When(x => x.BirthDate.HasValue)
            .WithMessage("Birth date cannot be in the future")
            .GreaterThan(DateTime.Today.AddYears(-150))
            .When(x => x.BirthDate.HasValue)
            .WithMessage("Birth date cannot be more than 150 years ago");
        
        // DateOfStart validation (optional)
        RuleFor(x => x.DateOfStart)
            .LessThanOrEqualTo(DateTime.Today)
            .When(x => x.DateOfStart.HasValue)
            .WithMessage("Start date cannot be in the future")
            .GreaterThan(DateTime.Today.AddYears(-10))
            .When(x => x.DateOfStart.HasValue)
            .WithMessage("Start date cannot be more than 10 years ago");
    }
}