using FitnessCorrector.Domain.Enums;

namespace FitnessCorrector.Domain.Entities;

public class WorkoutSession
{
    private WorkoutSession() { }

    public static WorkoutSession Create(Guid userId, Guid exerciseId, string videoFilePath)
    {
        if (userId == Guid.Empty) throw new ArgumentException("Invalid User ID");
        if (exerciseId == Guid.Empty) throw new ArgumentException("Invalid Exercise ID");
        if (string.IsNullOrWhiteSpace(videoFilePath)) throw new ArgumentNullException(nameof(videoFilePath));

        return new WorkoutSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ExerciseId = exerciseId,
            VideoFilePath = videoFilePath,
            Status = WorkoutSessionStatus.Processing,
            CreatedAt = DateTime.UtcNow
        };
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid ExerciseId { get; private set; }
    public string VideoFilePath { get; private set; } = null!;
    public WorkoutSessionStatus Status { get; private set; }
    public string? AiFeedback { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public string? OutputPath { get; private set; }

    // DDD Method to update the entity after Python finishes
    public void MarkAsCompleted(string feedback, string outputPath)
    {
        Status = WorkoutSessionStatus.Completed;
        AiFeedback = feedback;
        OutputPath = outputPath;
    }
    
    public void MarkAsFailed(string error)
    {
        Status = WorkoutSessionStatus.Failed;
        AiFeedback = error;
    }
}