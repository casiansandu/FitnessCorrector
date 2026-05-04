using FitnessCorrector.Application.Abstractions;
using FitnessCorrector.Application.Subscriptions.Queries.GetUserSubscriptionQuery;
using FitnessCorrector.Domain.Entities;
using FitnessCorrector.Domain.Enums;
using Moq;

namespace FitnessCorrector.Tests;

public class GetUserSubscriptionQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Null_When_Subscription_Missing()
    {
        var repository = new Mock<ISubscriptionRepository>();

        repository
            .Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        var handler = new GetUserSubscriptionQueryHandler(repository.Object);

        var result = await handler.Handle(new GetUserSubscriptionQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_Should_Map_Subscription_To_Dto()
    {
        var repository = new Mock<ISubscriptionRepository>();
        var userId = Guid.NewGuid();
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            StripeSubscriptionId = "sub_123",
            PlanType = PlanType.Premium,
            Status = SubscriptionStatus.Active,
            CurrentPeriodStart = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            CurrentPeriodEnd = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            CancelAtPeriodEnd = false,
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = null
        };

        repository
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        var handler = new GetUserSubscriptionQueryHandler(repository.Object);

        var result = await handler.Handle(new GetUserSubscriptionQuery(userId), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(subscription.Id, result!.Id);
        Assert.Equal(subscription.UserId, result.UserId);
        Assert.Equal(subscription.StripeSubscriptionId, result.StripeSubscriptionId);
        Assert.Equal(subscription.PlanType.ToString(), result.PlanType);
        Assert.Equal(subscription.Status.ToString(), result.Status);
    }
}
