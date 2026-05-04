using FluentValidation;

namespace FitnessCorrector.Application.Subscriptions.Commands.HandleStripeWebhookCommand;

public class HandleStripeWebhookCommandValidator : AbstractValidator<HandleStripeWebhookCommand>
{
    public HandleStripeWebhookCommandValidator()
    {
        RuleFor(x => x.Json)
            .NotEmpty().WithMessage("Payload is required");

        RuleFor(x => x.Signature)
            .NotEmpty().WithMessage("Signature is required");
    }
}
