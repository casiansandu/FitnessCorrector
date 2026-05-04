using FitnessCorrector.Application.Abstractions;
using FitnessCorrector.Application.Exercises.Queries.GetSessionLandmarksQuery;
using FitnessCorrector.Domain.Entities;
using Moq;

namespace FitnessCorrector.Tests;

public class GetSessionLandmarksQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Null_When_Session_Not_Found()
    {
        var repository = new Mock<IWorkoutSessionRepository>();

        repository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((WorkoutSession?)null);

        var handler = new GetSessionLandmarksHandler(repository.Object);

        var result = await handler.Handle(new GetSessionLandmarksQuery(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_OutputPath_Is_Empty()
    {
        var repository = new Mock<IWorkoutSessionRepository>();
        var session = WorkoutSession.Create(Guid.NewGuid(), Guid.NewGuid(), "video.mp4");

        repository
            .Setup(x => x.GetByIdAsync(session.Id))
            .ReturnsAsync(session);

        var handler = new GetSessionLandmarksHandler(repository.Object);

        var result = await handler.Handle(new GetSessionLandmarksQuery(session.Id, session.UserId), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_User_Not_Authorized()
    {
        var repository = new Mock<IWorkoutSessionRepository>();
        var session = WorkoutSession.Create(Guid.NewGuid(), Guid.NewGuid(), "video.mp4");
        session.MarkAsCompleted("ok", "output.json");

        repository
            .Setup(x => x.GetByIdAsync(session.Id))
            .ReturnsAsync(session);

        var handler = new GetSessionLandmarksHandler(repository.Object);

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(new GetSessionLandmarksQuery(session.Id, Guid.NewGuid()), CancellationToken.None));

        Assert.Equal("You do not have permission to access this workout session.", ex.Message);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_File_Missing()
    {
        var repository = new Mock<IWorkoutSessionRepository>();
        var session = WorkoutSession.Create(Guid.NewGuid(), Guid.NewGuid(), "video.mp4");
        var missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        session.MarkAsCompleted("ok", missingPath);

        repository
            .Setup(x => x.GetByIdAsync(session.Id))
            .ReturnsAsync(session);

        var handler = new GetSessionLandmarksHandler(repository.Object);

        await Assert.ThrowsAsync<FileNotFoundException>(
            () => handler.Handle(new GetSessionLandmarksQuery(session.Id, session.UserId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Should_Return_File_Contents_When_File_Exists()
    {
        var repository = new Mock<IWorkoutSessionRepository>();
        var session = WorkoutSession.Create(Guid.NewGuid(), Guid.NewGuid(), "video.mp4");

        var path = Path.GetTempFileName();
        await File.WriteAllTextAsync(path, "content");
        session.MarkAsCompleted("ok", path);

        repository
            .Setup(x => x.GetByIdAsync(session.Id))
            .ReturnsAsync(session);

        var handler = new GetSessionLandmarksHandler(repository.Object);

        try
        {
            var result = await handler.Handle(new GetSessionLandmarksQuery(session.Id, session.UserId), CancellationToken.None);

            Assert.Equal("content", result);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
