using FluentValidation;
using WarriorExperiment.Persistence.Entities;

namespace WarriorExperiment.Persistence.Validators;

/// <summary>
/// Validator for WaExercise entity
/// </summary>
public class WaExerciseValidator : AbstractValidator<WaExercise>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WaExerciseValidator"/> class
    /// </summary>
    public WaExerciseValidator()
    {
        // Variety practice validation
        RuleFor(x => x.VarietyPracticeId)
            .GreaterThan(0).WithMessage("Valid variety practice ID is required");
        
        // Name validation
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Exercise name is required")
            .Length(1, 100).WithMessage("Exercise name must be between 1 and 100 characters")
            .Matches(@"^[\w\s\-\.\(\)]+$").WithMessage("Exercise name contains invalid characters");
        
        // Sets validation
        RuleFor(x => x.Sets)
            .GreaterThanOrEqualTo(0).WithMessage("Sets must be 0 or greater");
        
        // Reps validation
        RuleFor(x => x.Reps)
            .GreaterThanOrEqualTo(0).WithMessage("Reps must be 0 or greater");
        
        // Weight validation (optional)
        RuleFor(x => x.Weight)
            .InclusiveBetween(0m, 500m).When(x => x.Weight.HasValue)
            .WithMessage("Weight must be between 0 and 500 kg");
        
        // At least sets or reps should be greater than 0
        RuleFor(x => x)
            .Must(x => x.Sets > 0 || x.Reps > 0)
            .WithMessage("Either sets or reps must be greater than 0")
            .WithName("ExerciseActivity");
    }
}