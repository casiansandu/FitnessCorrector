using MediatR;

namespace FitnessCorrector.Application.Subscriptions.Commands.CancelSubscriptionCommand;

public record CancelSubscriptionCommand(
    Guid UserId,
    string StripeSubscriptionId
) : IRequest<bool>;
