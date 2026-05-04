using FitnessCorrector.Application.Abstractions;
using FitnessCorrector.Application.Users.Commands.RegisterUserCommand;
using FitnessCorrector.Domain.Entities;
using FitnessCorrector.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Moq;

namespace FitnessCorrector.Tests;

public class RegisterUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Throw_When_Email_Already_Exists()
    {
        var usersRepository = new Mock<IUsersRepository>();
        var jwtGenerator = new Mock<IJwtTokenGenerator>();
        var configuration = new Mock<IConfiguration>();
        configuration.Setup(x => x["AdminEmail"]).Returns((string?)null);

        usersRepository
            .Setup(x => x.EmailExistsAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new RegisterUserCommandHandler(
            usersRepository.Object,
            jwtGenerator.Object,
            configuration.Object);

        var command = new RegisterUserCommand(
            Email: "user@example.com",
            PasswordHash: "hash",
            FirstName: "Jane",
            LastName: "Doe");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal("User with this email already exists", ex.Message);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Email_Is_Admin_Email()
    {
        var usersRepository = new Mock<IUsersRepository>();
        var jwtGenerator = new Mock<IJwtTokenGenerator>();
        var configuration = new Mock<IConfiguration>();
        configuration.Setup(x => x["AdminEmail"]).Returns("admin@example.com");

        usersRepository
            .Setup(x => x.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new RegisterUserCommandHandler(
            usersRepository.Object,
            jwtGenerator.Object,
            configuration.Object);

        var command = new RegisterUserCommand(
            Email: "admin@example.com",
            PasswordHash: "hash",
            FirstName: "Jane",
            LastName: "Doe");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal("This email address is reserved", ex.Message);
    }

    [Fact]
    public async Task Handle_Should_Create_User_And_Return_Auth_Result()
    {
        var usersRepository = new Mock<IUsersRepository>();
        var jwtGenerator = new Mock<IJwtTokenGenerator>();
        var configuration = new Mock<IConfiguration>();
        configuration.Setup(x => x["AdminEmail"]).Returns((string?)null);

        User? createdUser = null;
        usersRepository
            .Setup(x => x.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        usersRepository
            .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User user, CancellationToken _) =>
            {
                createdUser = user;
                return user;
            });

        jwtGenerator
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("token");

        var handler = new RegisterUserCommandHandler(
            usersRepository.Object,
            jwtGenerator.Object,
            configuration.Object);

        var command = new RegisterUserCommand(
            Email: "user@example.com",
            PasswordHash: "hash",
            FirstName: "Jane",
            LastName: "Doe");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.NotNull(createdUser);
        Assert.Equal(command.Email, createdUser!.Email);
        Assert.Equal(command.PasswordHash, createdUser.PasswordHash);
        Assert.Equal(command.FirstName, createdUser.FirstName);
        Assert.Equal(command.LastName, createdUser.LastName);
        Assert.Equal(UserRole.User, createdUser.Role);

        Assert.Equal(createdUser.Id, result.UserId);
        Assert.Equal("token", result.Token);
        Assert.Equal(UserRole.User.ToString(), result.Role);
    }
}
