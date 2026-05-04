namespace FitnessCorrector.Application.Subscriptions.Common;

public record SubscriptionDto(
    Guid Id,
    Guid UserId,
    string StripeSubscriptionId,
    string PlanType,
    string Status,
    DateTime CurrentPeriodStart,
    DateTime CurrentPeriodEnd,
    bool CancelAtPeriodEnd,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string? CheckoutUrl
);

public record PlanPricingDto(
    string PlanType,
    int PriceInCents,
    string Description,
    List<string> Features
);

public record CreateSubscriptionCheckoutDto(string CheckoutUrl);
