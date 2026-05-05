namespace FitnessCorrector.Domain.Entities;

public class WorkoutSessionMetrics
{
    private WorkoutSessionMetrics() { }

    public static WorkoutSessionMetrics Create(
        Guid workoutSessionId,
        Guid userId,
        Guid exerciseId,
        string exerciseSlug,
        double averageDepth,
        double averageTempoSeconds,
        double averageSymmetry,
        int repCount)
    {
        if (workoutSessionId == Guid.Empty) throw new ArgumentException("Invalid workout session ID");
        if (userId == Guid.Empty) throw new ArgumentException("Invalid user ID");
        if (exerciseId == Guid.Empty) throw new ArgumentException("Invalid exercise ID");
        if (string.IsNullOrWhiteSpace(exerciseSlug)) throw new ArgumentNullException(nameof(exerciseSlug));

        return new WorkoutSessionMetrics
        {
            Id = Guid.NewGuid(),
            WorkoutSessionId = workoutSessionId,
            UserId = userId,
            ExerciseId = exerciseId,
            ExerciseSlug = exerciseSlug,
            AverageDepth = averageDepth,
            AverageTempoSeconds = averageTempoSeconds,
            AverageSymmetry = averageSymmetry,
            RepCount = repCount,
            CreatedAt = DateTime.UtcNow
        };
    }

    public Guid Id { get; private set; }
    public Guid WorkoutSessionId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid ExerciseId { get; private set; }
    public string ExerciseSlug { get; private set; } = null!;
    public double AverageDepth { get; private set; }
    public double AverageTempoSeconds { get; private set; }
    public double AverageSymmetry { get; private set; }
    public int RepCount { get; private set; }
    public DateTime CreatedAt { get; private set; }
}
