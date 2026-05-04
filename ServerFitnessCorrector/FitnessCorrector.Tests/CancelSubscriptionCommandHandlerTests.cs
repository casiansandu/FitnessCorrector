using FitnessCorrector.Application.Abstractions;
using FitnessCorrector.Application.Subscriptions.Commands.CancelSubscriptionCommand;
using FitnessCorrector.Domain.Entities;
using Moq;

namespace FitnessCorrector.Tests;

public class CancelSubscriptionCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Throw_When_Subscription_Not_Found()
    {
        var subscriptionService = new Mock<ISubscriptionService>();
        var subscriptionRepository = new Mock<ISubscriptionRepository>();

        subscriptionRepository
            .Setup(x => x.GetByStripeSubscriptionIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        var handler = new CancelSubscriptionCommandHandler(
            subscriptionService.Object,
            subscriptionRepository.Object);

        var command = new CancelSubscriptionCommand(
            UserId: Guid.NewGuid(),
            StripeSubscriptionId: "sub_123");

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal("You don't have permission to cancel this subscription", ex.Message);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Subscription_Belongs_To_Different_User()
    {
        var subscriptionService = new Mock<ISubscriptionService>();
        var subscriptionRepository = new Mock<ISubscriptionRepository>();

        subscriptionRepository
            .Setup(x => x.GetByStripeSubscriptionIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Subscription { UserId = Guid.NewGuid() });

        var handler = new CancelSubscriptionCommandHandler(
            subscriptionService.Object,
            subscriptionRepository.Object);

        var command = new CancelSubscriptionCommand(
            UserId: Guid.NewGuid(),
            StripeSubscriptionId: "sub_123");

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal("You don't have permission to cancel this subscription", ex.Message);
    }

    [Fact]
    public async Task Handle_Should_Cancel_Subscription_When_User_Matches()
    {
        var subscriptionService = new Mock<ISubscriptionService>();
        var subscriptionRepository = new Mock<ISubscriptionRepository>();
        var userId = Guid.NewGuid();

        subscriptionRepository
            .Setup(x => x.GetByStripeSubscriptionIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Subscription { UserId = userId });

        subscriptionService
            .Setup(x => x.CancelSubscriptionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new CancelSubscriptionCommandHandler(
            subscriptionService.Object,
            subscriptionRepository.Object);

        var command = new CancelSubscriptionCommand(
            UserId: userId,
            StripeSubscriptionId: "sub_123");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result);
    }
}
