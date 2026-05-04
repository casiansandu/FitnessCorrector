using FitnessCorrector.Domain.Enums;

namespace FitnessCorrector.Domain.Entities;

public class Subscription
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    
    // Stripe IDs
    public string StripeCustomerId { get; set; } = string.Empty;
    public string StripeSubscriptionId { get; set; } = string.Empty;
    
    // Plan and Status
    public PlanType PlanType { get; set; }
    public SubscriptionStatus Status { get; set; }
    
    // Billing dates
    public DateTime CurrentPeriodStart { get; set; }
    public DateTime CurrentPeriodEnd { get; set; }
    public bool CancelAtPeriodEnd { get; set; }
    
    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation property
    public User? User { get; set; }
}
