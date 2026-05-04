using FitnessCorrector.Application.Users.Common;
using MediatR;

namespace FitnessCorrector.Application.Users.Commands.RegisterUserCommand;

public record RegisterUserCommand(
    string Email,
    string PasswordHash,
    string FirstName,
    string LastName
) : IRequest<AuthenticationResult>;
