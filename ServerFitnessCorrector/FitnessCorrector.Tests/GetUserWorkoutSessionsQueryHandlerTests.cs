using FitnessCorrector.Application.Abstractions;
using FitnessCorrector.Application.WorkoutSessions.Queries.GetUserWorkoutSessionsQuery;
using FitnessCorrector.Domain.Entities;
using FitnessCorrector.Domain.Enums;
using Moq;

namespace FitnessCorrector.Tests;

public class GetUserWorkoutSessionsQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Mapped_Sessions()
    {
        var repository = new Mock<IWorkoutSessionRepository>();
        var userId = Guid.NewGuid();

        var session1 = WorkoutSession.Create(userId, Guid.NewGuid(), "video1.mp4");
        var session2 = WorkoutSession.Create(userId, Guid.NewGuid(), "video2.mp4");
        session2.MarkAsCompleted("ok", "output.json");

        repository
            .Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(new List<WorkoutSession> { session1, session2 });

        var handler = new GetUserWorkoutSessionsQueryHandler(repository.Object);

        var result = (await handler.Handle(new GetUserWorkoutSessionsQuery(userId), CancellationToken.None)).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(session1.Id, result[0].Id);
        Assert.Equal(session2.Id, result[1].Id);
        Assert.Equal(session2.OutputPath, result[1].OutputPath);
        Assert.Equal(WorkoutSessionStatus.Completed, result[1].Status);
    }
}
