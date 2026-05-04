using FitnessCorrector.Application.Abstractions;
using MediatR;

namespace FitnessCorrector.Application.Subscriptions.Commands.UpdateSubscriptionCommand;

public class UpdateSubscriptionCommandHandler : IRequestHandler<UpdateSubscriptionCommand, bool>
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ISubscriptionRepository _subscriptionRepository;

    public UpdateSubscriptionCommandHandler(
        ISubscriptionService subscriptionService,
        ISubscriptionRepository subscriptionRepository)
    {
        _subscriptionService = subscriptionService;
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<bool> Handle(UpdateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        // Verify subscription belongs to user
        var subscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(
            request.StripeSubscriptionId,
            cancellationToken);

        if (subscription == null || subscription.UserId != request.UserId)
        {
            throw new UnauthorizedAccessException("You don't have permission to update this subscription");
        }

        return await _subscriptionService.UpdateSubscriptionAsync(
            request.StripeSubscriptionId,
            request.NewPlanType,
            cancellationToken);
    }
}
