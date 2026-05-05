using MediatR;

namespace FitnessCorrector.Application.WorkoutSessions.Queries.GetUserWorkoutSessionCountQuery;

public record GetUserWorkoutSessionCountQuery(Guid UserId, int Limit) : IRequest<GetUserWorkoutSessionCountDto>;
