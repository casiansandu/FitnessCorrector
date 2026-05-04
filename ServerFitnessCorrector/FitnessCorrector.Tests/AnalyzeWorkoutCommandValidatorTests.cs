using FitnessCorrector.Application.Exercises.Commands.AnalyzeWorkoutCommand;

namespace FitnessCorrector.Tests;

public class AnalyzeWorkoutCommandValidatorTests
{
    [Fact]
    public void AnalyzeWorkoutCommandValidator_Should_Pass_For_Valid_Command()
    {
        var validator = new AnalyzeWorkoutCommandValidator();
        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var command = new AnalyzeWorkoutCommand(
            UserId: Guid.NewGuid(),
            ExerciseId: Guid.NewGuid(),
            Slug: "push-up",
            VideoStream: stream,
            FileName: "video.mp4"
        );

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void AnalyzeWorkoutCommandValidator_Should_Fail_When_UserId_Is_Empty()
    {
        var validator = new AnalyzeWorkoutCommandValidator();
        using var stream = new MemoryStream(new byte[] { 1 });
        var command = new AnalyzeWorkoutCommand(
            UserId: Guid.Empty,
            ExerciseId: Guid.NewGuid(),
            Slug: "push-up",
            VideoStream: stream,
            FileName: "video.mp4"
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "UserId");
    }

    [Fact]
    public void AnalyzeWorkoutCommandValidator_Should_Fail_When_ExerciseId_Is_Empty()
    {
        var validator = new AnalyzeWorkoutCommandValidator();
        using var stream = new MemoryStream(new byte[] { 1 });
        var command = new AnalyzeWorkoutCommand(
            UserId: Guid.NewGuid(),
            ExerciseId: Guid.Empty,
            Slug: "push-up",
            VideoStream: stream,
            FileName: "video.mp4"
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ExerciseId");
    }

    [Fact]
    public void AnalyzeWorkoutCommandValidator_Should_Fail_When_Slug_Is_Empty()
    {
        var validator = new AnalyzeWorkoutCommandValidator();
        using var stream = new MemoryStream(new byte[] { 1 });
        var command = new AnalyzeWorkoutCommand(
            UserId: Guid.NewGuid(),
            ExerciseId: Guid.NewGuid(),
            Slug: "",
            VideoStream: stream,
            FileName: "video.mp4"
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Slug");
    }

    [Fact]
    public void AnalyzeWorkoutCommandValidator_Should_Fail_When_VideoStream_Is_Null()
    {
        var validator = new AnalyzeWorkoutCommandValidator();
        var command = new AnalyzeWorkoutCommand(
            UserId: Guid.NewGuid(),
            ExerciseId: Guid.NewGuid(),
            Slug: "push-up",
            VideoStream: null!,
            FileName: "video.mp4"
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "VideoStream");
    }

    [Fact]
    public void AnalyzeWorkoutCommandValidator_Should_Fail_When_FileName_Is_Empty()
    {
        var validator = new AnalyzeWorkoutCommandValidator();
        using var stream = new MemoryStream(new byte[] { 1 });
        var command = new AnalyzeWorkoutCommand(
            UserId: Guid.NewGuid(),
            ExerciseId: Guid.NewGuid(),
            Slug: "push-up",
            VideoStream: stream,
            FileName: ""
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "FileName");
    }
}
