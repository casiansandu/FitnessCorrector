using FitnessCorrector.Application.Subscriptions.Commands.HandleStripeWebhookCommand;

namespace FitnessCorrector.Tests;

public class HandleStripeWebhookCommandValidatorTests
{
    [Fact]
    public void HandleStripeWebhookCommandValidator_Should_Pass_For_Valid_Command()
    {
        var validator = new HandleStripeWebhookCommandValidator();
        var command = new HandleStripeWebhookCommand(
            Json: "{\"type\":\"test\"}",
            Signature: "sig_123"
        );

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void HandleStripeWebhookCommandValidator_Should_Fail_When_Json_Is_Empty()
    {
        var validator = new HandleStripeWebhookCommandValidator();
        var command = new HandleStripeWebhookCommand(
            Json: "",
            Signature: "sig_123"
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Json");
    }

    [Fact]
    public void HandleStripeWebhookCommandValidator_Should_Fail_When_Signature_Is_Empty()
    {
        var validator = new HandleStripeWebhookCommandValidator();
        var command = new HandleStripeWebhookCommand(
            Json: "{}",
            Signature: ""
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Signature");
    }
}
