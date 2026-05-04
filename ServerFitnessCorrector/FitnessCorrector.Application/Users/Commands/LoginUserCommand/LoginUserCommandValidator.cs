using FluentValidation;

namespace FitnessCorrector.Application.Users.Commands.LoginUserCommand;

public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.PasswordHash)
            .NotEmpty().WithMessage("Password hash is required")
            .MaximumLength(500).WithMessage("Password hash must not exceed 500 characters");
    }
}
