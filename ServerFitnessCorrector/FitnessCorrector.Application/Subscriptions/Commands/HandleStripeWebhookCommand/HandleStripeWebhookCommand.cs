using FitnessCorrector.Application.Abstractions;
using MediatR;

namespace FitnessCorrector.Application.Subscriptions.Commands.HandleStripeWebhookCommand;

public record HandleStripeWebhookCommand(string Json, string Signature) : IRequest<bool>;

public class HandleStripeWebhookCommandHandler : IRequestHandler<HandleStripeWebhookCommand, bool>
{
    private readonly ISubscriptionService _subscriptionService;

    public HandleStripeWebhookCommandHandler(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    public async Task<bool> Handle(HandleStripeWebhookCommand request, CancellationToken cancellationToken)
    {
        return await _subscriptionService.ProcessWebhookAsync(request.Json, request.Signature, cancellationToken);
    }
}
