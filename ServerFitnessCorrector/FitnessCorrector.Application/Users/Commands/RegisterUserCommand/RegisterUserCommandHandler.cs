using FitnessCorrector.Application.Abstractions;
using FitnessCorrector.Application.Users.Common;
using FitnessCorrector.Domain.Entities;
using FitnessCorrector.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace FitnessCorrector.Application.Users.Commands.RegisterUserCommand;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthenticationResult>
{
    private readonly IUsersRepository _usersRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IConfiguration _configuration;

    public RegisterUserCommandHandler(
        IUsersRepository usersRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IConfiguration configuration)
    {
        _usersRepository = usersRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _configuration = configuration;
    }

    public async Task<AuthenticationResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Check if user already exists
        if (await _usersRepository.EmailExistsAsync(request.Email, cancellationToken))
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        // Block registration with admin email
        var adminEmail = _configuration["AdminEmail"];
        if (!string.IsNullOrEmpty(adminEmail) &&
            request.Email.Equals(adminEmail, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("This email address is reserved");
        }

        // Create user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = request.PasswordHash, // Already hashed from frontend
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        };

        await _usersRepository.CreateAsync(user, cancellationToken);

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
