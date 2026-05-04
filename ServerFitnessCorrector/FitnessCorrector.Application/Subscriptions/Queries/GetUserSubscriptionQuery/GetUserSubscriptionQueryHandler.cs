using FitnessCorrector.Application.Abstractions;
using FitnessCorrector.Application.Subscriptions.Common;
using MediatR;

namespace FitnessCorrector.Application.Subscriptions.Queries.GetUserSubscriptionQuery;

public class GetUserSubscriptionQueryHandler : IRequestHandler<GetUserSubscriptionQuery, SubscriptionDto?>
{
    private readonly ISubscriptionRepository _subscriptionRepository;

    public GetUserSubscriptionQueryHandler(ISubscriptionRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<SubscriptionDto?> Handle(GetUserSubscriptionQuery request, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        if (subscription == null)
        {
            return null;
        }

        return new SubscriptionDto(
            subscription.Id,
            subscription.UserId,
            subscription.StripeSubscriptionId,
            subscription.PlanType.ToString(),
            subscription.Status.ToString(),
            subscription.CurrentPeriodStart,
            subscription.CurrentPeriodEnd,
            subscription.CancelAtPeriodEnd,
            subscription.CreatedAt,
            subscription.UpdatedAt,
            null
        );
    }
}
