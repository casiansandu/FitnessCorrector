using FitnessCorrector.Application.Abstractions;
using FitnessCorrector.Application.Subscriptions.Common;
using FitnessCorrector.Domain.Enums;
using MediatR;

namespace FitnessCorrector.Application.Subscriptions.Queries.GetPricingPlansQuery;

public class GetPricingPlansQueryHandler : IRequestHandler<GetPricingPlansQuery, List<PlanPricingDto>>
{
    private readonly ISubscriptionService _subscriptionService;

    public GetPricingPlansQueryHandler(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    public Task<List<PlanPricingDto>> Handle(GetPricingPlansQuery request, CancellationToken cancellationToken)
    {
        var plans = _subscriptionService.GetPricingPlans();

        var dtos = plans.Select(p => new PlanPricingDto(
            p.PlanType.ToString(),
            p.PriceInCents,
            p.Description,
            p.Features
        )).ToList();

        return Task.FromResult(dtos);
    }
}
