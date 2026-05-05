namespace FitnessCorrector.Application.WorkoutSessions.Queries.GetUserWorkoutSessionCountQuery;

public record GetUserWorkoutSessionCountDto(int TotalCount, int RemainingCount, int Limit);
