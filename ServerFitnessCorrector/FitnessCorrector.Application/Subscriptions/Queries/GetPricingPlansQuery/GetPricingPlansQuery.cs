using FitnessCorrector.Application.Subscriptions.Common;
using MediatR;

namespace FitnessCorrector.Application.Subscriptions.Queries.GetPricingPlansQuery;

public record GetPricingPlansQuery() : IRequest<List<PlanPricingDto>>;
