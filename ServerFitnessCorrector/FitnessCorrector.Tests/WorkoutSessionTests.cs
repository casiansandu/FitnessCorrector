using FitnessCorrector.Domain.Entities;
using FitnessCorrector.Domain.Enums;

namespace FitnessCorrector.Tests;

public class WorkoutSessionTests
{
    [Fact]
    public void Create_Should_Throw_When_UserId_Is_Empty()
    {
        Assert.Throws<ArgumentException>(
            () => WorkoutSession.Create(Guid.Empty, Guid.NewGuid(), "video.mp4"));
    }

    [Fact]
    public void Create_Should_Throw_When_ExerciseId_Is_Empty()
    {
        Assert.Throws<ArgumentException>(
            () => WorkoutSession.Create(Guid.NewGuid(), Guid.Empty, "video.mp4"));
    }

    [Fact]
    public void Create_Should_Throw_When_VideoFilePath_Is_Empty()
    {
        Assert.Throws<ArgumentNullException>(
            () => WorkoutSession.Create(Guid.NewGuid(), Guid.NewGuid(), " "));
    }

    [Fact]
    public void Create_Should_Set_Processing_Status_And_Properties()
    {
        var userId = Guid.NewGuid();
        var exerciseId = Guid.NewGuid();
        var session = WorkoutSession.Create(userId, exerciseId, "video.mp4");

        Assert.NotEqual(Guid.Empty, session.Id);
        Assert.Equal(userId, session.UserId);
        Assert.Equal(exerciseId, session.ExerciseId);
        Assert.Equal("video.mp4", session.VideoFilePath);
        Assert.Equal(WorkoutSessionStatus.Processing, session.Status);
        Assert.True(session.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void MarkAsCompleted_Should_Set_Status_And_Output()
    {
        var session = WorkoutSession.Create(Guid.NewGuid(), Guid.NewGuid(), "video.mp4");

        session.MarkAsCompleted("ok", "output.json");

        Assert.Equal(WorkoutSessionStatus.Completed, session.Status);
        Assert.Equal("ok", session.AiFeedback);
        Assert.Equal("output.json", session.OutputPath);
    }

    [Fact]
    public void MarkAsFailed_Should_Set_Status_And_Error()
    {
        var session = WorkoutSession.Create(Guid.NewGuid(), Guid.NewGuid(), "video.mp4");

        session.MarkAsFailed("error");

        Assert.Equal(WorkoutSessionStatus.Failed, session.Status);
        Assert.Equal("error", session.AiFeedback);
    }
}
