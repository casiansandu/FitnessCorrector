using FitnessCorrector.Application.Subscriptions.Common;
using MediatR;

namespace FitnessCorrector.Application.Subscriptions.Queries.GetUserSubscriptionQuery;

public record GetUserSubscriptionQuery(Guid UserId) : IRequest<SubscriptionDto?>;
