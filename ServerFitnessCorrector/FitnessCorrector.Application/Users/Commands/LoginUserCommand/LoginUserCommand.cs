using FitnessCorrector.Application.Users.Common;
using MediatR;

namespace FitnessCorrector.Application.Users.Commands.LoginUserCommand;

public record LoginUserCommand(
    string Email,
    string PasswordHash
) : IRequest<AuthenticationResult>;
