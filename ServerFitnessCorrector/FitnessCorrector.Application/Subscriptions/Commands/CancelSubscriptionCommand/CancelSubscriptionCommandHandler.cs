using FitnessCorrector.Application.Abstractions;
using MediatR;

namespace FitnessCorrector.Application.Subscriptions.Commands.CancelSubscriptionCommand;

public class CancelSubscriptionCommandHandler : IRequestHandler<CancelSubscriptionCommand, bool>
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ISubscriptionRepository _subscriptionRepository;

    public CancelSubscriptionCommandHandler(
        ISubscriptionService subscriptionService,
        ISubscriptionRepository subscriptionRepository)
    {
        _subscriptionService = subscriptionService;
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<bool> Handle(CancelSubscriptionCommand request, CancellationToken cancellationToken)
    {
        // Verify the subscription belongs to the user
        var subscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(
            request.StripeSubscriptionId,
            cancellationToken);

        if (subscription == null || subscription.UserId != request.UserId)
        {
            throw new UnauthorizedAccessException("You don't have permission to cancel this subscription");
        }

        return await _subscriptionService.CancelSubscriptionAsync(request.StripeSubscriptionId, cancellationToken);
    }
}
