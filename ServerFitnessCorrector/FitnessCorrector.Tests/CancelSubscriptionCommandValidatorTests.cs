using FitnessCorrector.Application.Subscriptions.Commands.CancelSubscriptionCommand;

namespace FitnessCorrector.Tests;

public class CancelSubscriptionCommandValidatorTests
{
    [Fact]
    public void CancelSubscriptionCommandValidator_Should_Pass_For_Valid_Command()
    {
        var validator = new CancelSubscriptionCommandValidator();
        var command = new CancelSubscriptionCommand(
            UserId: Guid.NewGuid(),
            StripeSubscriptionId: "sub_123"
        );

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CancelSubscriptionCommandValidator_Should_Fail_When_UserId_Is_Empty()
    {
        var validator = new CancelSubscriptionCommandValidator();
        var command = new CancelSubscriptionCommand(
            UserId: Guid.Empty,
            StripeSubscriptionId: "sub_123"
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "UserId");
    }

    [Fact]
    public void CancelSubscriptionCommandValidator_Should_Fail_When_StripeSubscriptionId_Is_Empty()
    {
        var validator = new CancelSubscriptionCommandValidator();
        var command = new CancelSubscriptionCommand(
            UserId: Guid.NewGuid(),
            StripeSubscriptionId: ""
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "StripeSubscriptionId");
    }
}
