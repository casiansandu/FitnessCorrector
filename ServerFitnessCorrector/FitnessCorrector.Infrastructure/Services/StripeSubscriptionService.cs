using FitnessCorrector.Application.Abstractions;
using FitnessCorrector.Domain.Entities;
using FitnessCorrector.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Billing;
using Stripe.Checkout;
using Subscription = FitnessCorrector.Domain.Entities.Subscription;

namespace FitnessCorrector.Infrastructure.Services;

public class StripeSubscriptionService : ISubscriptionService
{
    private readonly IConfiguration _configuration;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ILogger<StripeSubscriptionService> _logger;
    private readonly string _webhookSecret;

    // Plan pricing configuration (in cents, monthly)
    private static readonly Dictionary<PlanType, (int Price, string StripePriceId)> PlanPricing = new()
    {
        { PlanType.Basic, (4000, "price_1TOeWG94bpqxpSXxQ7uBKJxL") } // 40 RON/month
    };

    public StripeSubscriptionService(
        IConfiguration configuration,
        ISubscriptionRepository subscriptionRepository,
        ILogger<StripeSubscriptionService> logger)
    {
        _configuration = configuration;
        _subscriptionRepository = subscriptionRepository;
        _logger = logger;
        _webhookSecret = configuration["Stripe:WebhookSecret"] ?? string.Empty;

        // Set Stripe API key
        StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];
    }

    public async Task<string> CreateSubscriptionAsync(
        Guid userId,
        string email,
        PlanType planType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Create Stripe customer
            var customerOptions = new CustomerCreateOptions
            {
                Email = email,
                Metadata = new Dictionary<string, string>
                {
                    { "UserId", userId.ToString() }
                }
            };

            var customerService = new CustomerService();
            var customer = await customerService.CreateAsync(customerOptions, cancellationToken: cancellationToken);

            _logger.LogInformation($"Created Stripe customer {customer.Id} for user {userId}");

            // Create checkout session
            if (!PlanPricing.ContainsKey(planType))
            {
                throw new InvalidOperationException($"Plan type {planType} not configured");
            }

            var stripePriceId = PlanPricing[planType].StripePriceId;
            var successUrl = _configuration["Stripe:SuccessUrl"] ?? "http://localhost:5173/success";
            var cancelUrl = _configuration["Stripe:CancelUrl"] ?? "http://localhost:5173/subscriptions";

            var sessionOptions = new SessionCreateOptions
            {
                Mode = "subscription",
                Customer = customer.Id,
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                ClientReferenceId = userId.ToString(),
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Price = stripePriceId,
                        Quantity = 1
                    }
                },
                Metadata = new Dictionary<string, string>
                {
                    { "UserId", userId.ToString() },
                    { "PlanType", planType.ToString() }
                }
            };

            var sessionService = new SessionService();
            var session = await sessionService.CreateAsync(sessionOptions, cancellationToken: cancellationToken);

            _logger.LogInformation("Created Stripe checkout session {SessionId} for user {UserId}", session.Id, userId);

            if (string.IsNullOrWhiteSpace(session.Url))
            {
                throw new InvalidOperationException("Stripe checkout URL was not generated");
            }

            return session.Url;
        }
        catch (StripeException ex)
        {
            _logger.LogError($"Stripe error creating subscription: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> CancelSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var subscriptionService = new SubscriptionService();
            var options = new SubscriptionUpdateOptions
            {
                CancelAtPeriodEnd = true
            };

            await subscriptionService.UpdateAsync(subscriptionId, options, cancellationToken: cancellationToken);

            // Update in database
            var subscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(subscriptionId, cancellationToken);
            if (subscription != null)
            {
                subscription.CancelAtPeriodEnd = true;
                subscription.UpdatedAt = DateTime.UtcNow;
                await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
            }

            _logger.LogInformation($"Canceled subscription {subscriptionId}");
            return true;
        }
        catch (StripeException ex)
        {
            _logger.LogError($"Stripe error canceling subscription: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> UpdateSubscriptionAsync(
        string subscriptionId,
        PlanType newPlanType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!PlanPricing.ContainsKey(newPlanType))
            {
                throw new InvalidOperationException($"Plan type {newPlanType} not configured");
            }

            var subscriptionService = new SubscriptionService();
            var subscription = await subscriptionService.GetAsync(subscriptionId, cancellationToken: cancellationToken);

            if (subscription?.Items?.Data?.Count > 0)
            {
                var itemId = subscription.Items.Data[0].Id;
                var newPriceId = PlanPricing[newPlanType].StripePriceId;

                var options = new SubscriptionItemUpdateOptions { Price = newPriceId };
                await subscriptionService.UpdateAsync(
                    subscriptionId,
                    new SubscriptionUpdateOptions
                    {
                        Items = new List<SubscriptionItemOptions>
                        {
                            new SubscriptionItemOptions { Id = itemId, Price = newPriceId }
                        }
                    },
                    cancellationToken: cancellationToken);

                // Update in database
                var dbSubscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(subscriptionId, cancellationToken);
                if (dbSubscription != null)
                {
                    dbSubscription.PlanType = newPlanType;
                    dbSubscription.UpdatedAt = DateTime.UtcNow;
                    await _subscriptionRepository.UpdateAsync(dbSubscription, cancellationToken);
                }

                _logger.LogInformation($"Updated subscription {subscriptionId} to plan {newPlanType}");
                return true;
            }

            return false;
        }
        catch (StripeException ex)
        {
            _logger.LogError($"Stripe error updating subscription: {ex.Message}");
            throw;
        }
    }

    public async Task<Subscription?> GetSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken = default)
    {
        return await _subscriptionRepository.GetByStripeSubscriptionIdAsync(subscriptionId, cancellationToken);
    }

    public async Task<bool> ProcessWebhookAsync(string json, string signature, CancellationToken cancellationToken = default)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(json, signature, _webhookSecret);

            switch (stripeEvent.Type)
            {
                case EventTypes.CheckoutSessionCompleted:
                    await HandleCheckoutSessionCompletedAsync((Session)stripeEvent.Data.Object, cancellationToken);
                    break;

                case EventTypes.CustomerSubscriptionUpdated:
                    await HandleSubscriptionUpdatedAsync((Stripe.Subscription)stripeEvent.Data.Object, cancellationToken);
                    break;

                case EventTypes.CustomerSubscriptionDeleted:
                    await HandleSubscriptionDeletedAsync((Stripe.Subscription)stripeEvent.Data.Object, cancellationToken);
                    break;

                case EventTypes.InvoicePaid:
                    await HandleInvoicePaidAsync((Invoice)stripeEvent.Data.Object, cancellationToken);
                    break;

                case EventTypes.InvoicePaymentFailed:
                    await HandleInvoicePaymentFailedAsync((Invoice)stripeEvent.Data.Object, cancellationToken);
                    break;
            }

            return true;
        }
        catch (StripeException ex)
        {
            _logger.LogError($"Stripe webhook error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SyncUserSubscriptionAsync(Guid userId, string email, CancellationToken cancellationToken = default)
    {
        var existingLocalSubscription = await _subscriptionRepository.GetByUserIdAsync(userId, cancellationToken);
        if (existingLocalSubscription != null)
        {
            return true;
        }

        var candidateCustomers = new Dictionary<string, Customer>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(email))
        {
            var customerService = new CustomerService();
            var customersByEmail = await customerService.ListAsync(
                new CustomerListOptions
                {
                    Email = email,
                    Limit = 100
                },
                cancellationToken: cancellationToken);

            foreach (var customer in customersByEmail.Data)
            {
                candidateCustomers[customer.Id] = customer;
            }
        }

        if (candidateCustomers.Count == 0)
        {
            _logger.LogInformation("No Stripe customer candidates found for user {UserId}", userId);
            return false;
        }

        var stripeSubscriptionService = new SubscriptionService();
        Stripe.Subscription? stripeSubscription = null;
        Customer? matchedCustomer = null;

        foreach (var customer in candidateCustomers.Values.OrderByDescending(c => c.Created))
        {
            var subscriptions = await stripeSubscriptionService.ListAsync(
                new SubscriptionListOptions
                {
                    Customer = customer.Id,
                    Limit = 20,
                    Status = "all"
                },
                cancellationToken: cancellationToken);

            stripeSubscription = subscriptions.Data
                .Where(s =>
                    string.Equals(s.Status, "active", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(s.Status, "trialing", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(s.Status, "past_due", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(s.Status, "incomplete", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(s.Status, "unpaid", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(s => s.Created)
                .FirstOrDefault();

            if (stripeSubscription != null)
            {
                matchedCustomer = customer;
                break;
            }
        }

        if (stripeSubscription == null || matchedCustomer == null)
        {
            _logger.LogInformation("No Stripe subscriptions found for user {UserId} across {CustomerCount} customers", userId, candidateCustomers.Count);
            return false;
        }

        var existingByStripeId = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(stripeSubscription.Id, cancellationToken);
        var firstItem = stripeSubscription.Items?.Data?.FirstOrDefault();
        var planType = GetPlanTypeFromPriceId(firstItem?.Price?.Id ?? string.Empty);

        if (existingByStripeId != null)
        {
            existingByStripeId.UserId = userId;
            existingByStripeId.StripeCustomerId = matchedCustomer.Id;
            existingByStripeId.PlanType = planType;
            existingByStripeId.Status = MapSubscriptionStatus(stripeSubscription.Status);
            existingByStripeId.CurrentPeriodStart = firstItem?.CurrentPeriodStart ?? stripeSubscription.StartDate;
            existingByStripeId.CurrentPeriodEnd = firstItem?.CurrentPeriodEnd ?? stripeSubscription.TrialEnd ?? stripeSubscription.CancelAt ?? DateTime.UtcNow.AddMonths(1);
            existingByStripeId.CancelAtPeriodEnd = stripeSubscription.CancelAtPeriodEnd;
            existingByStripeId.UpdatedAt = DateTime.UtcNow;

            await _subscriptionRepository.UpdateAsync(existingByStripeId, cancellationToken);
            return true;
        }

        var subscriptionEntity = new Subscription
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            StripeCustomerId = matchedCustomer.Id,
            StripeSubscriptionId = stripeSubscription.Id,
            PlanType = planType,
            Status = MapSubscriptionStatus(stripeSubscription.Status),
            CurrentPeriodStart = firstItem?.CurrentPeriodStart ?? stripeSubscription.StartDate,
            CurrentPeriodEnd = firstItem?.CurrentPeriodEnd ?? stripeSubscription.TrialEnd ?? stripeSubscription.CancelAt ?? DateTime.UtcNow.AddMonths(1),
            CancelAtPeriodEnd = stripeSubscription.CancelAtPeriodEnd,
            CreatedAt = DateTime.UtcNow
        };

        await _subscriptionRepository.CreateAsync(subscriptionEntity, cancellationToken);
        return true;
    }

    public List<(PlanType PlanType, int PriceInCents, string Description, List<string> Features)> GetPricingPlans()
    {
        return new List<(PlanType, int, string, List<string>)>
        {
            (
                PlanType.Basic,
                PlanPricing[PlanType.Basic].Price,
                "Abonament lunar complet pentru Fitness Corrector",
                new List<string>
                {
                    "Analiza completa pentru videoclipuri",
                    "Feedback pentru forma exercitiilor",
                    "Istoric sesiuni si progres",
                    "Acces lunar nelimitat"
                }
            )
        };
    }

    // Private helper methods
    private async Task HandleCheckoutSessionCompletedAsync(Session checkoutSession, CancellationToken cancellationToken)
    {
        var stripeSubscriptionId = checkoutSession.SubscriptionId;
        var stripeCustomerId = checkoutSession.CustomerId;

        if (string.IsNullOrWhiteSpace(stripeSubscriptionId) || string.IsNullOrWhiteSpace(stripeCustomerId))
        {
            return;
        }

        var existingSubscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(stripeSubscriptionId, cancellationToken);
        if (existingSubscription != null)
        {
            return;
        }

        var userIdRaw = checkoutSession.Metadata?.GetValueOrDefault("UserId") ?? checkoutSession.ClientReferenceId;
        if (!Guid.TryParse(userIdRaw, out var userId))
        {
            _logger.LogWarning("Cannot map checkout session {SessionId} to user id", checkoutSession.Id);
            return;
        }

        var subscriptionService = new SubscriptionService();
        var stripeSubscription = await subscriptionService.GetAsync(stripeSubscriptionId, cancellationToken: cancellationToken);
        var firstItem = stripeSubscription.Items?.Data?.FirstOrDefault();

        var planType = GetPlanTypeFromPriceId(firstItem?.Price?.Id ?? string.Empty);

        var subscriptionEntity = new Subscription
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            StripeCustomerId = stripeCustomerId,
            StripeSubscriptionId = stripeSubscriptionId,
            PlanType = planType,
            Status = MapSubscriptionStatus(stripeSubscription.Status),
            CurrentPeriodStart = firstItem?.CurrentPeriodStart ?? stripeSubscription.StartDate,
            CurrentPeriodEnd = firstItem?.CurrentPeriodEnd ?? stripeSubscription.TrialEnd ?? stripeSubscription.CancelAt ?? DateTime.UtcNow.AddMonths(1),
            CancelAtPeriodEnd = stripeSubscription.CancelAtPeriodEnd,
            CreatedAt = DateTime.UtcNow
        };

        await _subscriptionRepository.CreateAsync(subscriptionEntity, cancellationToken);
        _logger.LogInformation("Created local subscription {StripeSubscriptionId} after checkout completion", stripeSubscriptionId);
    }

    private async Task HandleSubscriptionUpdatedAsync(Stripe.Subscription stripeSubscription, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(stripeSubscription.Id, cancellationToken);
        var firstItem = stripeSubscription.Items?.Data?.FirstOrDefault();

        if (subscription != null)
        {
            subscription.Status = MapSubscriptionStatus(stripeSubscription.Status);
            subscription.CurrentPeriodStart = firstItem?.CurrentPeriodStart ?? stripeSubscription.StartDate;
            subscription.CurrentPeriodEnd = firstItem?.CurrentPeriodEnd ?? stripeSubscription.TrialEnd ?? stripeSubscription.CancelAt ?? DateTime.UtcNow.AddMonths(1);
            subscription.CancelAtPeriodEnd = stripeSubscription.CancelAtPeriodEnd;
            subscription.UpdatedAt = DateTime.UtcNow;

            await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
            _logger.LogInformation($"Updated subscription {stripeSubscription.Id} from webhook");
        }
    }

    private async Task HandleSubscriptionDeletedAsync(Stripe.Subscription stripeSubscription, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(stripeSubscription.Id, cancellationToken);

        if (subscription != null)
        {
            subscription.Status = SubscriptionStatus.Canceled;
            subscription.UpdatedAt = DateTime.UtcNow;
            await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
            _logger.LogInformation($"Deleted subscription {stripeSubscription.Id} from webhook");
        }
    }

    private async Task HandleInvoicePaidAsync(Invoice invoice, CancellationToken cancellationToken)
    {
        var subscriptionId = invoice.Parent?.SubscriptionDetails?.SubscriptionId;
        _logger.LogInformation($"Invoice {invoice.Id} paid for subscription {subscriptionId}");
        // Here you could trigger additional actions like sending confirmation emails
    }

    private async Task HandleInvoicePaymentFailedAsync(Invoice invoice, CancellationToken cancellationToken)
    {
        var subscriptionId = invoice.Parent?.SubscriptionDetails?.SubscriptionId;
        _logger.LogWarning($"Invoice {invoice.Id} payment failed for subscription {subscriptionId}");
        // Here you could trigger notifications to the user about payment failure
    }

    private static PlanType GetPlanTypeFromPriceId(string priceId)
    {
        foreach (var plan in PlanPricing)
        {
            if (string.Equals(plan.Value.StripePriceId, priceId, StringComparison.OrdinalIgnoreCase))
            {
                return plan.Key;
            }
        }

        return PlanType.Basic;
    }

    private static SubscriptionStatus MapSubscriptionStatus(string? stripeStatus)
    {
        return stripeStatus?.ToLowerInvariant() switch
        {
            "active" => SubscriptionStatus.Active,
            "canceled" => SubscriptionStatus.Canceled,
            "past_due" => SubscriptionStatus.PastDue,
            "incomplete" => SubscriptionStatus.Incomplete,
            "trialing" => SubscriptionStatus.Trialing,
            _ => SubscriptionStatus.Incomplete
        };
    }

    private static DateTime UnixTimeStampToDateTime(long? unixTimeStamp)
    {
        if (unixTimeStamp == null) return DateTime.UtcNow;
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp.Value).ToUniversalTime();
        return dateTime;
    }
}
