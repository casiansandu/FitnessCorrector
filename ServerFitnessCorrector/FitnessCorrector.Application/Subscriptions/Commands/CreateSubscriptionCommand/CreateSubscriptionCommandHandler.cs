using FitnessCorrector.Application.Abstractions;
using FitnessCorrector.Application.Subscriptions.Common;
using MediatR;

namespace FitnessCorrector.Application.Subscriptions.Commands.CreateSubscriptionCommand;

public class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, CreateSubscriptionCheckoutDto>
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ISubscriptionRepository _subscriptionRepository;

    public CreateSubscriptionCommandHandler(
        ISubscriptionService subscriptionService,
        ISubscriptionRepository subscriptionRepository)
    {
        _subscriptionService = subscriptionService;
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<CreateSubscriptionCheckoutDto> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        // Check if user already has an active subscription
        var existingSubscription = await _subscriptionRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (existingSubscription != null)
        {
            throw new InvalidOperationException("User already has an active subscription");
        }

        // Create checkout session via Stripe
        var checkoutUrl = await _subscriptionService.CreateSubscriptionAsync(
            request.UserId,
            request.Email,
            request.PlanType,
            cancellationToken);

        return new CreateSubscriptionCheckoutDto(checkoutUrl);
    }
}
