using FitnessCorrector.Domain.Entities;
using FitnessCorrector.Domain.Enums;

namespace FitnessCorrector.Application.Abstractions;

public interface ISubscriptionService
{
    /// <summary>
    /// Creates a Stripe Checkout Session URL for subscription payment
    /// </summary>
    Task<string> CreateSubscriptionAsync(
        Guid userId,
        string email,
        PlanType planType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a subscription at the end of the current billing period
    /// </summary>
    Task<bool> CancelSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a subscription to a new plan type
    /// </summary>
    Task<bool> UpdateSubscriptionAsync(
        string subscriptionId,
        PlanType newPlanType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves subscription details from Stripe
    /// </summary>
    Task<Subscription?> GetSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes Stripe webhook events
    /// </summary>
    Task<bool> ProcessWebhookAsync(string json, string signature, CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts to sync a user's subscription from Stripe into local storage.
    /// Useful when webhook delivery is delayed.
    /// </summary>
    Task<bool> SyncUserSubscriptionAsync(Guid userId, string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pricing information for all plans
    /// </summary>
    List<(PlanType PlanType, int PriceInCents, string Description, List<string> Features)> GetPricingPlans();
}
