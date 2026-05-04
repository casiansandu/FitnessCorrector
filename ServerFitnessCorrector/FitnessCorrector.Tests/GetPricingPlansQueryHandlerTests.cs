using FitnessCorrector.Application.Abstractions;
using FitnessCorrector.Application.Subscriptions.Queries.GetPricingPlansQuery;
using FitnessCorrector.Domain.Enums;
using Moq;

namespace FitnessCorrector.Tests;

public class GetPricingPlansQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Map_Pricing_Plans()
    {
        var subscriptionService = new Mock<ISubscriptionService>();

        subscriptionService
            .Setup(x => x.GetPricingPlans())
            .Returns(new List<(PlanType PlanType, int PriceInCents, string Description, List<string> Features)>
            {
                (PlanType.Basic, 1000, "Basic", new List<string> { "A" }),
                (PlanType.Pro, 2500, "Pro", new List<string> { "A", "B" })
            });

        var handler = new GetPricingPlansQueryHandler(subscriptionService.Object);

        var result = await handler.Handle(new GetPricingPlansQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(PlanType.Basic.ToString(), result[0].PlanType);
        Assert.Equal(1000, result[0].PriceInCents);
        Assert.Equal("Pro", result[1].Description);
        Assert.Equal(2, result[1].Features.Count);
    }
}
