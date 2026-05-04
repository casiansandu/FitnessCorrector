using FitnessCorrector.Domain.Enums;

namespace FitnessCorrector.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Subscription relationship
    public Guid? SubscriptionId { get; set; }
    public Subscription? Subscription { get; set; }
}
