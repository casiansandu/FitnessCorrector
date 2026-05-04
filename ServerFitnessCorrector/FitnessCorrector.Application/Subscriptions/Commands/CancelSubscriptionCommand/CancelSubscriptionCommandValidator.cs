using FluentValidation;

namespace FitnessCorrector.Application.Subscriptions.Commands.CancelSubscriptionCommand;

public class CancelSubscriptionCommandValidator : AbstractValidator<CancelSubscriptionCommand>
{
    public CancelSubscriptionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.StripeSubscriptionId)
            .NotEmpty().WithMessage("Stripe subscription ID is required");
    }
}
