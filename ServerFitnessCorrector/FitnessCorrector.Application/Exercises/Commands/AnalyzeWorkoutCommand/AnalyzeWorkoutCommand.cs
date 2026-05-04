using FitnessCorrector.Domain.Entities;
using FitnessCorrector.Domain.Enums;
using MediatR;

namespace FitnessCorrector.Application.Exercises.Commands.AnalyzeWorkoutCommand;

public record AnalyzeWorkoutCommand(
    Guid UserId,
    Guid ExerciseId,
    string Slug,
    Stream VideoStream,
    string FileName
) : IRequest<WorkoutSession>;