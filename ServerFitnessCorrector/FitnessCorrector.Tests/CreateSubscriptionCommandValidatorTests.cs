using FitnessCorrector.Application.Subscriptions.Commands.CreateSubscriptionCommand;
using FitnessCorrector.Domain.Enums;

namespace FitnessCorrector.Tests;

public class CreateSubscriptionCommandValidatorTests
{
    [Fact]
    public void CreateSubscriptionCommandValidator_Should_Pass_For_Valid_Command()
    {
        var validator = new CreateSubscriptionCommandValidator();
        var command = new CreateSubscriptionCommand(
            UserId: Guid.NewGuid(),
            Email: "user@example.com",
            PlanType: PlanType.Basic
        );

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateSubscriptionCommandValidator_Should_Fail_When_UserId_Is_Empty()
    {
        var validator = new CreateSubscriptionCommandValidator();
        var command = new CreateSubscriptionCommand(
            UserId: Guid.Empty,
            Email: "user@example.com",
            PlanType: PlanType.Basic
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "UserId");
    }

    [Fact]
    public void CreateSubscriptionCommandValidator_Should_Fail_When_Email_Is_Empty()
    {
        var validator = new CreateSubscriptionCommandValidator();
        var command = new CreateSubscriptionCommand(
            UserId: Guid.NewGuid(),
            Email: "",
            PlanType: PlanType.Basic
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public void CreateSubscriptionCommandValidator_Should_Fail_When_Email_Is_Invalid()
    {
        var validator = new CreateSubscriptionCommandValidator();
        var command = new CreateSubscriptionCommand(
            UserId: Guid.NewGuid(),
            Email: "invalid-email",
            PlanType: PlanType.Basic
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public void CreateSubscriptionCommandValidator_Should_Fail_When_Email_Is_Too_Long()
    {
        var validator = new CreateSubscriptionCommandValidator();
        var email = new string('a', 251) + "@a.com";
        var command = new CreateSubscriptionCommand(
            UserId: Guid.NewGuid(),
            Email: email,
            PlanType: PlanType.Basic
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public void CreateSubscriptionCommandValidator_Should_Fail_When_PlanType_Is_Invalid()
    {
        var validator = new CreateSubscriptionCommandValidator();
        var command = new CreateSubscriptionCommand(
            UserId: Guid.NewGuid(),
            Email: "user@example.com",
            PlanType: (PlanType)999
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "PlanType");
    }
}
