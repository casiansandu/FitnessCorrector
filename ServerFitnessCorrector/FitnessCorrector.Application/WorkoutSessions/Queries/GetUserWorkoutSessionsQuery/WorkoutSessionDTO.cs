using FitnessCorrector.Domain.Enums;

namespace FitnessCorrector.Application.WorkoutSessions.Queries.GetUserWorkoutSessionsQuery;

public record WorkoutSessionDto(
    Guid Id,
    Guid UserId,
    Guid ExerciseId,
    string VideoFilePath,
    WorkoutSessionStatus Status,
    string? AiFeedback,
    string? OutputPath,
    DateTime CreatedAt
);
