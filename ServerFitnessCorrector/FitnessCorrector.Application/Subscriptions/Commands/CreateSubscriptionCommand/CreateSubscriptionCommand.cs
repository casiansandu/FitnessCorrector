using FitnessCorrector.Application.Subscriptions.Common;
using FitnessCorrector.Domain.Enums;
using MediatR;

namespace FitnessCorrector.Application.Subscriptions.Commands.CreateSubscriptionCommand;

public record CreateSubscriptionCommand(
    Guid UserId,
    string Email,
    PlanType PlanType
) : IRequest<CreateSubscriptionCheckoutDto>;
