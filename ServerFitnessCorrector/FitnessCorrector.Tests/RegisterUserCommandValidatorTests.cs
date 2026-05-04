using FitnessCorrector.Application.Users.Commands.RegisterUserCommand;

namespace FitnessCorrector.Tests;

public class RegisterUserCommandValidatorTests
{
    [Fact]
    public void RegisterUserCommandValidator_Should_Pass_For_Valid_Command()
    {
        var validator = new RegisterUserCommandValidator();
        var command = new RegisterUserCommand(
            Email: "user@example.com",
            PasswordHash: new string('a', 60),
            FirstName: "Jane",
            LastName: "Doe"
        );

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void RegisterUserCommandValidator_Should_Fail_When_Email_Is_Empty()
    {
        var validator = new RegisterUserCommandValidator();
        var command = new RegisterUserCommand(
            Email: "",
            PasswordHash: new string('a', 60),
            FirstName: "Jane",
            LastName: "Doe"
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public void RegisterUserCommandValidator_Should_Fail_When_Email_Is_Invalid()
    {
        var validator = new RegisterUserCommandValidator();
        var command = new RegisterUserCommand(
            Email: "invalid-email",
            PasswordHash: new string('a', 60),
            FirstName: "Jane",
            LastName: "Doe"
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public void RegisterUserCommandValidator_Should_Fail_When_Email_Is_Too_Long()
    {
        var validator = new RegisterUserCommandValidator();
        var email = new string('a', 251) + "@a.com";
        var command = new RegisterUserCommand(
            Email: email,
            PasswordHash: new string('a', 60),
            FirstName: "Jane",
            LastName: "Doe"
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public void RegisterUserCommandValidator_Should_Fail_When_PasswordHash_Is_Empty()
    {
        var validator = new RegisterUserCommandValidator();
        var command = new RegisterUserCommand(
            Email: "user@example.com",
            PasswordHash: "",
            FirstName: "Jane",
            LastName: "Doe"
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "PasswordHash");
    }

    [Fact]
    public void RegisterUserCommandValidator_Should_Fail_When_PasswordHash_Is_Too_Long()
    {
        var validator = new RegisterUserCommandValidator();
        var command = new RegisterUserCommand(
            Email: "user@example.com",
            PasswordHash: new string('a', 501),
            FirstName: "Jane",
            LastName: "Doe"
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "PasswordHash");
    }

    [Fact]
    public void RegisterUserCommandValidator_Should_Fail_When_FirstName_Is_Empty()
    {
        var validator = new RegisterUserCommandValidator();
        var command = new RegisterUserCommand(
            Email: "user@example.com",
            PasswordHash: new string('a', 60),
            FirstName: "",
            LastName: "Doe"
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "FirstName");
    }

    [Fact]
    public void RegisterUserCommandValidator_Should_Fail_When_FirstName_Is_Too_Long()
    {
        var validator = new RegisterUserCommandValidator();
        var command = new RegisterUserCommand(
            Email: "user@example.com",
            PasswordHash: new string('a', 60),
            FirstName: new string('a', 101),
            LastName: "Doe"
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "FirstName");
    }

    [Fact]
    public void RegisterUserCommandValidator_Should_Fail_When_LastName_Is_Empty()
    {
        var validator = new RegisterUserCommandValidator();
        var command = new RegisterUserCommand(
            Email: "user@example.com",
            PasswordHash: new string('a', 60),
            FirstName: "Jane",
            LastName: ""
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "LastName");
    }

    [Fact]
    public void RegisterUserCommandValidator_Should_Fail_When_LastName_Is_Too_Long()
    {
        var validator = new RegisterUserCommandValidator();
        var command = new RegisterUserCommand(
            Email: "user@example.com",
            PasswordHash: new string('a', 60),
            FirstName: "Jane",
            LastName: new string('a', 101)
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "LastName");
    }
}
