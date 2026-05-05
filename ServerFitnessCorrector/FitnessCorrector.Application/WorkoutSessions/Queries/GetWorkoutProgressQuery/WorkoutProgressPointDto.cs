namespace FitnessCorrector.Application.WorkoutSessions.Queries.GetWorkoutProgressQuery;

public record WorkoutProgressPointDto(
    DateTime Date,
    double AverageDepth,
    double AverageTempoSeconds,
    double AverageSymmetry,
    int RepCount
);
