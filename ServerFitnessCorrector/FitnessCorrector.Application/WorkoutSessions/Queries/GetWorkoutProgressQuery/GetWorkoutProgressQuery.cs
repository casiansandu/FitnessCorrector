using MediatR;

namespace FitnessCorrector.Application.WorkoutSessions.Queries.GetWorkoutProgressQuery;

public record GetWorkoutProgressQuery(Guid UserId, Guid ExerciseId, int RangeDays) : IRequest<List<WorkoutProgressPointDto>>;
