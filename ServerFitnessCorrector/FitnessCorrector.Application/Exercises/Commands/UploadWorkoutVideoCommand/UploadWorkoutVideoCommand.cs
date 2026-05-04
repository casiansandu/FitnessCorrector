using FitnessCorrector.Domain.Entities;
using MediatR;

namespace FitnessCorrector.Application.Exercises.Commands.UploadWorkoutVideoCommand;

public record UploadWorkoutVideoCommand(
    Guid UserId,
    Guid ExerciseId,
    string Slug,
    Stream VideoStream,
    string FileName
) : IRequest<WorkoutSession>;