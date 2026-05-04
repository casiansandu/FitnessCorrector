using FluentValidation;
using FitnessCorrector.Application.Common.Behaviors;
using MediatR;

namespace FitnessCorrector.Tests;

public class ValidationBehaviorTests
{
    private record TestRequest(string Value) : IRequest<string>;

    private class TestRequestValidator : AbstractValidator<TestRequest>
    {
        public TestRequestValidator()
        {
            RuleFor(x => x.Value).NotEmpty().WithMessage("Value is required");
        }
    }

    [Fact]
    public async Task Handle_Should_Call_Next_When_No_Validators()
    {
        var behavior = new ValidationBehavior<TestRequest, string>(Array.Empty<IValidator<TestRequest>>());
        var request = new TestRequest("ok");
        var called = false;

        var result = await behavior.Handle(
            request,
            _ =>
            {
                called = true;
                return Task.FromResult("done");
            },
            CancellationToken.None);

        Assert.True(called);
        Assert.Equal("done", result);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Validation_Fails()
    {
        var validators = new IValidator<TestRequest>[] { new TestRequestValidator() };
        var behavior = new ValidationBehavior<TestRequest, string>(validators);
        var request = new TestRequest("");

        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(request, _ => Task.FromResult("done"), CancellationToken.None));

        Assert.Contains(ex.Errors, e => e.PropertyName == "Value");
    }

    [Fact]
    public async Task Handle_Should_Call_Next_When_Validation_Passes()
    {
        var validators = new IValidator<TestRequest>[] { new TestRequestValidator() };
        var behavior = new ValidationBehavior<TestRequest, string>(validators);
        var request = new TestRequest("ok");
        var called = false;

        var result = await behavior.Handle(
            request,
            _ =>
            {
                called = true;
                return Task.FromResult("done");
            },
            CancellationToken.None);

        Assert.True(called);
        Assert.Equal("done", result);
    }
}
