using FitnessCorrector.Application.WorkoutSessions.Queries.GetUserWorkoutSessionsQuery;
using MediatR;

namespace FitnessCorrector.Application.WorkoutSessions.Queries.GetAllWorkoutSessionsQuery;

public record GetAllWorkoutSessionsQuery(int Take) : IRequest<IEnumerable<WorkoutSessionDto>>;
