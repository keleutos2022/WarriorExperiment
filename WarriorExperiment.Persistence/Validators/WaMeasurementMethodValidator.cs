using FluentValidation;
using WarriorExperiment.Persistence.Models;

namespace WarriorExperiment.Persistence.Validators;

/// <summary>
/// Validator for WaMeasurementMethod entity
/// </summary>
public class WaMeasurementMethodValidator : AbstractValidator<WaMeasurementMethod>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WaMeasurementMethodValidator"/> class
    /// </summary>
    public WaMeasurementMethodValidator()
    {
        // Name validation
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Measurement method name is required")
            .Length(3, 100).WithMessage("Name must be between 3 and 100 characters")
            .Matches(@"^[\w\s\-\.]+$").WithMessage("Name contains invalid characters");
        
        // Description validation (optional)
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
    }
}