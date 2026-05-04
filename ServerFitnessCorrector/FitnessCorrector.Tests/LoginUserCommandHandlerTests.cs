using FitnessCorrector.Application.Abstractions;
using FitnessCorrector.Application.Users.Commands.LoginUserCommand;
using FitnessCorrector.Domain.Entities;
using FitnessCorrector.Domain.Enums;
using Moq;

namespace FitnessCorrector.Tests;

public class LoginUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Throw_When_User_Not_Found()
    {
        var usersRepository = new Mock<IUsersRepository>();
        var jwtGenerator = new Mock<IJwtTokenGenerator>();

        usersRepository
            .Setup(x => x.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var handler = new LoginUserCommandHandler(usersRepository.Object, jwtGenerator.Object);

        var command = new LoginUserCommand(
            Email: "user@example.com",
            PasswordHash: "hash");

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal("Invalid email or password", ex.Message);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Password_Does_Not_Match()
    {
        var usersRepository = new Mock<IUsersRepository>();
        var jwtGenerator = new Mock<IJwtTokenGenerator>();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            PasswordHash = "stored-hash",
            FirstName = "Jane",
            LastName = "Doe",
            Role = UserRole.User
        };

        usersRepository
            .Setup(x => x.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new LoginUserCommandHandler(usersRepository.Object, jwtGenerator.Object);

        var command = new LoginUserCommand(
            Email: "user@example.com",
            PasswordHash: "other-hash");

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal("Invalid email or password", ex.Message);
    }

    [Fact]
    public async Task Handle_Should_Return_Auth_Result_When_Credentials_Are_Valid()
    {
        var usersRepository = new Mock<IUsersRepository>();
        var jwtGenerator = new Mock<IJwtTokenGenerator>();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            PasswordHash = "hash",
            FirstName = "Jane",
            LastName = "Doe",
            Role = UserRole.User
        };

        usersRepository
            .Setup(x => x.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        jwtGenerator
            .Setup(x => x.GenerateToken(user))
            .Returns("token");

        var handler = new LoginUserCommandHandler(usersRepository.Object, jwtGenerator.Object);

        var command = new LoginUserCommand(
            Email: "user@example.com",
            PasswordHash: "hash");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(user.FirstName, result.FirstName);
        Assert.Equal(user.LastName, result.LastName);
        Assert.Equal("token", result.Token);
        Assert.Equal(UserRole.User.ToString(), result.Role);
    }
}
