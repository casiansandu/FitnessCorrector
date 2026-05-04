using FitnessCorrector.Application.Abstractions;
using FitnessCorrector.Application.Exercises.Commands.UploadWorkoutVideoCommand;
using FitnessCorrector.Domain.Enums;
using Moq;

namespace FitnessCorrector.Tests;

public class UploadWorkoutVideoCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Mark_As_Completed_When_Output_File_Exists()
    {
        var repository = new Mock<IWorkoutSessionRepository>();
        var aiService = new Mock<IAiAnalyzerService>();

        var outputPath = Path.GetTempFileName();

        aiService
            .Setup(x => x.AnalyzeVideoAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<Stream>(),
                It.IsAny<string>()))
            .ReturnsAsync(("ok", outputPath));

        var handler = new UploadWorkoutVideoCommandHandler(repository.Object, aiService.Object);

        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var command = new UploadWorkoutVideoCommand(
            UserId: Guid.NewGuid(),
            ExerciseId: Guid.NewGuid(),
            Slug: "push-up",
            VideoStream: stream,
            FileName: "video.mp4");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(WorkoutSessionStatus.Completed, result.Status);
        Assert.Equal("ok", result.AiFeedback);
        Assert.Equal(outputPath, result.OutputPath);

        repository.Verify(x => x.UpdateAsync(result), Times.Once);

        File.Delete(outputPath);
    }

    [Fact]
    public async Task Handle_Should_Mark_As_Failed_When_Output_File_Missing()
    {
        var repository = new Mock<IWorkoutSessionRepository>();
        var aiService = new Mock<IAiAnalyzerService>();

        var outputPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mp4");

        aiService
            .Setup(x => x.AnalyzeVideoAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<Stream>(),
                It.IsAny<string>()))
            .ReturnsAsync(("failed", outputPath));

        var handler = new UploadWorkoutVideoCommandHandler(repository.Object, aiService.Object);

        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var command = new UploadWorkoutVideoCommand(
            UserId: Guid.NewGuid(),
            ExerciseId: Guid.NewGuid(),
            Slug: "push-up",
            VideoStream: stream,
            FileName: "video.mp4");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(WorkoutSessionStatus.Failed, result.Status);
        Assert.Equal("failed", result.AiFeedback);

        repository.Verify(x => x.UpdateAsync(result), Times.Once);
    }
}
