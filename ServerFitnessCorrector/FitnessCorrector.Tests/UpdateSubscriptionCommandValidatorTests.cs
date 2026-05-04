using FitnessCorrector.Application.Subscriptions.Commands.UpdateSubscriptionCommand;
using FitnessCorrector.Domain.Enums;

namespace FitnessCorrector.Tests;

public class UpdateSubscriptionCommandValidatorTests
{
    [Fact]
    public void UpdateSubscriptionCommandValidator_Should_Pass_For_Valid_Command()
    {
        var validator = new UpdateSubscriptionCommandValidator();
        var command = new UpdateSubscriptionCommand(
            UserId: Guid.NewGuid(),
            StripeSubscriptionId: "sub_123",
            NewPlanType: PlanType.Premium
        );

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void UpdateSubscriptionCommandValidator_Should_Fail_When_UserId_Is_Empty()
    {
        var validator = new UpdateSubscriptionCommandValidator();
        var command = new UpdateSubscriptionCommand(
            UserId: Guid.Empty,
            StripeSubscriptionId: "sub_123",
            NewPlanType: PlanType.Premium
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "UserId");
    }

    [Fact]
    public void UpdateSubscriptionCommandValidator_Should_Fail_When_StripeSubscriptionId_Is_Empty()
    {
        var validator = new UpdateSubscriptionCommandValidator();
        var command = new UpdateSubscriptionCommand(
            UserId: Guid.NewGuid(),
            StripeSubscriptionId: "",
            NewPlanType: PlanType.Premium
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "StripeSubscriptionId");
    }

    [Fact]
    public void UpdateSubscriptionCommandValidator_Should_Fail_When_PlanType_Is_Invalid()
    {
        var validator = new UpdateSubscriptionCommandValidator();
        var command = new UpdateSubscriptionCommand(
            UserId: Guid.NewGuid(),
            StripeSubscriptionId: "sub_123",
            NewPlanType: (PlanType)999
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "NewPlanType");
    }
}
