using FitnessCorrector.Application.Abstractions;
using FitnessCorrector.Application.Exercises.Commands;
using FitnessCorrector.Domain.Entities;
using FitnessCorrector.Domain.Enums;
using Moq;

namespace FitnessCorrector.Tests;

public class CreateExerciseCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Create_Exercise_And_Return_Id()
    {
        var repository = new Mock<IExercisesRepository>();
        Exercise? createdExercise = null;

        repository
            .Setup(x => x.AddAsync(It.IsAny<Exercise>()))
            .Callback<Exercise>(exercise => createdExercise = exercise)
            .Returns(Task.CompletedTask);

        var handler = new CreateExerciseHandler(repository.Object);

        var command = new CreateExerciseCommand(
            Slug: "push-up",
            Name: "Push Up",
            Description: "Classic push up",
            MuscleGroup: MuscleGroup.Chest);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.NotNull(createdExercise);
        Assert.Equal(command.Slug, createdExercise!.Slug);
        Assert.Equal(command.Name, createdExercise.Name);
        Assert.Equal(command.Description, createdExercise.Description);
        Assert.Equal(command.MuscleGroup, createdExercise.TargetMuscleGroup);
        Assert.Equal(createdExercise.Id, result);
    }
}
