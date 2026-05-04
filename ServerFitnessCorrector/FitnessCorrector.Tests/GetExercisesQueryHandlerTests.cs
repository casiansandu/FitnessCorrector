using FitnessCorrector.Application.Abstractions;
using FitnessCorrector.Application.Exercises.Queries;
using FitnessCorrector.Domain.Entities;
using FitnessCorrector.Domain.Enums;
using Moq;

namespace FitnessCorrector.Tests;

public class GetExercisesQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Map_Exercises()
    {
        var repository = new Mock<IExercisesRepository>();

        var exercise1 = Exercise.Create("push-up", "Push Up", "Classic push up", MuscleGroup.Chest);
        var exercise2 = Exercise.Create("squat", "Squat", "Classic squat", MuscleGroup.Legs);

        repository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Exercise> { exercise1, exercise2 });

        var handler = new GetExercisesQueryHandler(repository.Object);

        var result = await handler.Handle(new GetExercisesQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(exercise1.Id, result[0].Id);
        Assert.Equal(exercise1.TargetMuscleGroup.ToString(), result[0].MuscleGroup);
        Assert.Equal(exercise2.Name, result[1].Name);
    }
}
