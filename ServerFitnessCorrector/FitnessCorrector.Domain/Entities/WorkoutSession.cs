using FitnessCorrector.Domain.Enums;

namespace FitnessCorrector.Domain.Entities;

public class WorkoutSession
{
    private WorkoutSession() { }

    public static WorkoutSession Create(Guid exerciseId, string videoFilePath)
    {
        if (exerciseId == Guid.Empty) throw new ArgumentException("Invalid Exercise ID");
        if (string.IsNullOrWhiteSpace(videoFilePath)) throw new ArgumentNullException(nameof(videoFilePath));

        return new WorkoutSession
        {
            Id = Guid.NewGuid(),
            ExerciseId = exerciseId,
            VideoFilePath = videoFilePath,
            Status = WorkoutSessionStatus.Processing,
            CreatedAt = DateTime.UtcNow
        };
    }

    public Guid Id { get; private set; }
    public Guid ExerciseId { get; private set; }
    public string VideoFilePath { get; private set; } = null!;
    public WorkoutSessionStatus Status { get; private set; }
    public string? AiFeedback { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // DDD Method to update the entity after Python finishes
    public void MarkAsCompleted(string feedback)
    {
        Status = WorkoutSessionStatus.Completed;
        AiFeedback = feedback;
    }
    
    public void MarkAsFailed(string error)
    {
        Status = WorkoutSessionStatus.Failed;
        AiFeedback = error;
    }
}