using FluentValidation;

namespace FitnessCorrector.Application.Exercises.Commands.AnalyzeWorkoutCommand;

public class AnalyzeWorkoutCommandValidator : AbstractValidator<AnalyzeWorkoutCommand>
{
    public AnalyzeWorkoutCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.ExerciseId)
            .NotEmpty().WithMessage("ExerciseId is required");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required");

        RuleFor(x => x.VideoStream)
            .NotNull().WithMessage("Video stream is required");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required");
    }
}
