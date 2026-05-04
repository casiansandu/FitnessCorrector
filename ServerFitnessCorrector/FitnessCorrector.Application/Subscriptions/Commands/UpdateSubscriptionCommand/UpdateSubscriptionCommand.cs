using FitnessCorrector.Domain.Enums;
using MediatR;

namespace FitnessCorrector.Application.Subscriptions.Commands.UpdateSubscriptionCommand;

public record UpdateSubscriptionCommand(
    Guid UserId,
    string StripeSubscriptionId,
    PlanType NewPlanType
) : IRequest<bool>;
