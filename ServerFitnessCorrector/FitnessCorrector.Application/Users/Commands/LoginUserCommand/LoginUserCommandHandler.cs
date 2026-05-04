using FitnessCorrector.Application.Abstractions;
using FitnessCorrector.Application.Users.Common;
using MediatR;

namespace FitnessCorrector.Application.Users.Commands.LoginUserCommand;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, AuthenticationResult>
{
    private readonly IUsersRepository _usersRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginUserCommandHandler(
        IUsersRepository usersRepository,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _usersRepository = usersRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthenticationResult> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        // Get user by email
        var user = await _usersRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Compare password hashes (both are already hashed)
        if (user.PasswordHash != request.PasswordHash)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Generate JWT token
        var token = _jwtTokenGenerator.GenerateToken(user);

        return new AuthenticationResult(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            token,
            user.Role.ToString()
        );
    }
}
