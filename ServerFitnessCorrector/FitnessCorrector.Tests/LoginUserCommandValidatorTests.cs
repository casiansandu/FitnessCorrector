using FitnessCorrector.Application.Users.Commands.LoginUserCommand;

namespace FitnessCorrector.Tests;

public class LoginUserCommandValidatorTests
{
    [Fact]
    public void LoginUserCommandValidator_Should_Pass_For_Valid_Command()
    {
        var validator = new LoginUserCommandValidator();
        var command = new LoginUserCommand(
            Email: "user@example.com",
            PasswordHash: new string('a', 60)
        );

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void LoginUserCommandValidator_Should_Fail_When_Email_Is_Empty()
    {
        var validator = new LoginUserCommandValidator();
        var command = new LoginUserCommand(
            Email: "",
            PasswordHash: new string('a', 60)
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public void LoginUserCommandValidator_Should_Fail_When_Email_Is_Invalid()
    {
        var validator = new LoginUserCommandValidator();
        var command = new LoginUserCommand(
            Email: "invalid-email",
            PasswordHash: new string('a', 60)
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public void LoginUserCommandValidator_Should_Fail_When_Email_Is_Too_Long()
    {
        var validator = new LoginUserCommandValidator();
        var email = new string('a', 251) + "@a.com";
        var command = new LoginUserCommand(
            Email: email,
            PasswordHash: new string('a', 60)
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public void LoginUserCommandValidator_Should_Fail_When_PasswordHash_Is_Empty()
    {
        var validator = new LoginUserCommandValidator();
        var command = new LoginUserCommand(
            Email: "user@example.com",
            PasswordHash: ""
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "PasswordHash");
    }

    [Fact]
    public void LoginUserCommandValidator_Should_Fail_When_PasswordHash_Is_Too_Long()
    {
        var validator = new LoginUserCommandValidator();
        var command = new LoginUserCommand(
            Email: "user@example.com",
            PasswordHash: new string('a', 501)
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "PasswordHash");
    }
}
