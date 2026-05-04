using FitnessCorrector.Application.Abstractions;
using FitnessCorrector.Application.Subscriptions.Commands.HandleStripeWebhookCommand;
using Moq;

namespace FitnessCorrector.Tests;

public class HandleStripeWebhookCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Service_Result()
    {
        var subscriptionService = new Mock<ISubscriptionService>();
        subscriptionService
            .Setup(x => x.ProcessWebhookAsync("{}", "sig", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new HandleStripeWebhookCommandHandler(subscriptionService.Object);

        var command = new HandleStripeWebhookCommand("{}", "sig");
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result);
    }
}
