using MediatR;

namespace FitnessCorrector.Application.WorkoutSessions.Commands;

public record AnalyzeWorkoutCommand(Guid ExerciseId, string FilePath) : IRequest<Guid>;