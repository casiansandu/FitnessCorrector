using FitnessCorrector.Application.Exercises.Commands;
using FitnessCorrector.Domain.Enums;

namespace FitnessCorrector.Tests;

public class CreateExerciseCommandValidatorTests
{
    [Fact]
    public void CreateExerciseCommandValidator_Should_Pass_For_Valid_Command()
    {
        var validator = new CreateExerciseCommandValidator();
        var command = new CreateExerciseCommand(
            Slug: "push-up",
            Name: "Push Up",
            Description: "Classic push up",
            MuscleGroup: MuscleGroup.Chest
        );

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateExerciseCommandValidator_Should_Fail_When_Slug_Is_Empty()
    {
        var validator = new CreateExerciseCommandValidator();
        var command = new CreateExerciseCommand(
            Slug: "",
            Name: "Push Up",
            Description: "Classic push up",
            MuscleGroup: MuscleGroup.Chest
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Slug");
    }

    [Fact]
    public void CreateExerciseCommandValidator_Should_Fail_When_Name_Is_Empty()
    {
        var validator = new CreateExerciseCommandValidator();
        var command = new CreateExerciseCommand(
            Slug: "push-up",
            Name: "",
            Description: "Classic push up",
            MuscleGroup: MuscleGroup.Chest
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void CreateExerciseCommandValidator_Should_Fail_When_Name_Is_Too_Long()
    {
        var validator = new CreateExerciseCommandValidator();
        var command = new CreateExerciseCommand(
            Slug: "push-up",
            Name: new string('a', 101),
            Description: "Classic push up",
            MuscleGroup: MuscleGroup.Chest
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void CreateExerciseCommandValidator_Should_Fail_When_Description_Is_Empty()
    {
        var validator = new CreateExerciseCommandValidator();
        var command = new CreateExerciseCommand(
            Slug: "push-up",
            Name: "Push Up",
            Description: "",
            MuscleGroup: MuscleGroup.Chest
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description");
    }

    [Fact]
    public void CreateExerciseCommandValidator_Should_Fail_When_Description_Is_Too_Long()
    {
        var validator = new CreateExerciseCommandValidator();
        var command = new CreateExerciseCommand(
            Slug: "push-up",
            Name: "Push Up",
            Description: new string('a', 501),
            MuscleGroup: MuscleGroup.Chest
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description");
    }

    [Fact]
    public void CreateExerciseCommandValidator_Should_Fail_When_MuscleGroup_Is_Invalid()
    {
        var validator = new CreateExerciseCommandValidator();
        var command = new CreateExerciseCommand(
            Slug: "push-up",
            Name: "Push Up",
            Description: "Classic push up",
            MuscleGroup: (MuscleGroup)999
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "MuscleGroup");
    }
}
