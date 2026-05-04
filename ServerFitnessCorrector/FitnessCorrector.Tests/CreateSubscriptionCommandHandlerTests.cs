using FitnessCorrector.Application.Abstractions;
using FitnessCorrector.Application.Subscriptions.Commands.CreateSubscriptionCommand;
using FitnessCorrector.Domain.Entities;
using FitnessCorrector.Domain.Enums;
using Moq;

namespace FitnessCorrector.Tests;

public class CreateSubscriptionCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Throw_When_Subscription_Already_Exists()
    {
        var subscriptionService = new Mock<ISubscriptionService>();
        var subscriptionRepository = new Mock<ISubscriptionRepository>();

        subscriptionRepository
            .Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Subscription());

        var handler = new CreateSubscriptionCommandHandler(
            subscriptionService.Object,
            subscriptionRepository.Object);

        var command = new CreateSubscriptionCommand(
            UserId: Guid.NewGuid(),
            Email: "user@example.com",
            PlanType: PlanType.Basic);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal("User already has an active subscription", ex.Message);
    }

    [Fact]
    public async Task Handle_Should_Return_Checkout_Url_When_Subscription_Is_Created()
    {
        var subscriptionService = new Mock<ISubscriptionService>();
        var subscriptionRepository = new Mock<ISubscriptionRepository>();

        subscriptionRepository
            .Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        subscriptionService
            .Setup(x => x.CreateSubscriptionAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<PlanType>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("checkout-url");

        var handler = new CreateSubscriptionCommandHandler(
            subscriptionService.Object,
            subscriptionRepository.Object);

        var command = new CreateSubscriptionCommand(
            UserId: Guid.NewGuid(),
            Email: "user@example.com",
            PlanType: PlanType.Basic);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal("checkout-url", result.CheckoutUrl);
    }
}
