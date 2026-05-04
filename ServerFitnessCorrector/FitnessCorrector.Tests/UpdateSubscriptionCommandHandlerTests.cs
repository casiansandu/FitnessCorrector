using FitnessCorrector.Application.Abstractions;
using FitnessCorrector.Application.Subscriptions.Commands.UpdateSubscriptionCommand;
using FitnessCorrector.Domain.Entities;
using FitnessCorrector.Domain.Enums;
using Moq;

namespace FitnessCorrector.Tests;

public class UpdateSubscriptionCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Throw_When_Subscription_Not_Found()
    {
        var subscriptionService = new Mock<ISubscriptionService>();
        var subscriptionRepository = new Mock<ISubscriptionRepository>();

        subscriptionRepository
            .Setup(x => x.GetByStripeSubscriptionIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        var handler = new UpdateSubscriptionCommandHandler(
            subscriptionService.Object,
            subscriptionRepository.Object);

        var command = new UpdateSubscriptionCommand(
            UserId: Guid.NewGuid(),
            StripeSubscriptionId: "sub_123",
            NewPlanType: PlanType.Premium);

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal("You don't have permission to update this subscription", ex.Message);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Subscription_Belongs_To_Different_User()
    {
        var subscriptionService = new Mock<ISubscriptionService>();
        var subscriptionRepository = new Mock<ISubscriptionRepository>();

        subscriptionRepository
            .Setup(x => x.GetByStripeSubscriptionIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Subscription { UserId = Guid.NewGuid() });

        var handler = new UpdateSubscriptionCommandHandler(
            subscriptionService.Object,
            subscriptionRepository.Object);

        var command = new UpdateSubscriptionCommand(
            UserId: Guid.NewGuid(),
            StripeSubscriptionId: "sub_123",
            NewPlanType: PlanType.Premium);

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal("You don't have permission to update this subscription", ex.Message);
    }

    [Fact]
    public async Task Handle_Should_Update_Subscription_When_User_Matches()
    {
        var subscriptionService = new Mock<ISubscriptionService>();
        var subscriptionRepository = new Mock<ISubscriptionRepository>();
        var userId = Guid.NewGuid();

        subscriptionRepository
            .Setup(x => x.GetByStripeSubscriptionIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Subscription { UserId = userId });

        subscriptionService
            .Setup(x => x.UpdateSubscriptionAsync(
                It.IsAny<string>(),
                It.IsAny<PlanType>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new UpdateSubscriptionCommandHandler(
            subscriptionService.Object,
            subscriptionRepository.Object);

        var command = new UpdateSubscriptionCommand(
            UserId: userId,
            StripeSubscriptionId: "sub_123",
            NewPlanType: PlanType.Premium);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result);
    }
}
