using FitnessCorrector.Domain.Entities;
using FitnessCorrector.Domain.Enums;

namespace FitnessCorrector.Tests;

public class ExerciseTests
{
    [Fact]
    public void Create_Should_Throw_When_Name_Is_Empty()
    {
        var ex = Assert.Throws<ArgumentNullException>(
            () => Exercise.Create("slug", "", "desc", MuscleGroup.Core));

        Assert.Equal("name", ex.ParamName);
    }

    [Fact]
    public void Create_Should_Throw_When_Description_Is_Empty()
    {
        var ex = Assert.Throws<ArgumentNullException>(
            () => Exercise.Create("slug", "name", " ", MuscleGroup.Core));

        Assert.Equal("description", ex.ParamName);
    }

    [Fact]
    public void Create_Should_Throw_When_Slug_Is_Empty()
    {
        var ex = Assert.Throws<ArgumentNullException>(
            () => Exercise.Create(" ", "name", "desc", MuscleGroup.Core));

        Assert.Equal("slug", ex.ParamName);
    }

    [Fact]
    public void Create_Should_Set_Properties()
    {
        var exercise = Exercise.Create("push-up", "Push Up", "Classic push up", MuscleGroup.Chest);

        Assert.NotEqual(Guid.Empty, exercise.Id);
        Assert.Equal("push-up", exercise.Slug);
        Assert.Equal("Push Up", exercise.Name);
        Assert.Equal("Classic push up", exercise.Description);
        Assert.Equal(MuscleGroup.Chest, exercise.TargetMuscleGroup);
    }
}
