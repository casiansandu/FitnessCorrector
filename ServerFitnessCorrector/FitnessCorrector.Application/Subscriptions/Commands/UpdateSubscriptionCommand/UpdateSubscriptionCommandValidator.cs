using FluentValidation;

namespace FitnessCorrector.Application.Subscriptions.Commands.UpdateSubscriptionCommand;

public class UpdateSubscriptionCommandValidator : AbstractValidator<UpdateSubscriptionCommand>
{
    public UpdateSubscriptionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.StripeSubscriptionId)
            .NotEmpty().WithMessage("Stripe subscription ID is required");

        RuleFor(x => x.NewPlanType)
            .IsInEnum().WithMessage("Plan type is invalid");
    }
}
