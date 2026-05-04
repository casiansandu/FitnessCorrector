using MediatR;

namespace FitnessCorrector.Application.WorkoutSessions.Queries.GetUserWorkoutSessionsQuery;

public record GetUserWorkoutSessionsQuery(Guid UserId) : IRequest<IEnumerable<WorkoutSessionDto>>;
