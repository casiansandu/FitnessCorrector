using FitnessCorrector.Application.Abstractions;
using FitnessCorrector.Domain.Entities;
using MediatR;

namespace FitnessCorrector.Application.Exercises.Commands;

public class CreateExerciseHandler : IRequestHandler<CreateExerciseCommand, Guid>
{
    private readonly IExercisesRepository _repository;

    public CreateExerciseHandler(IExercisesRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateExerciseCommand request, CancellationToken cancellationToken)
    {
        var exercise = Exercise.Create(
            request.Name, 
            request.Description, 
            request.MuscleGroup
        );

        await _repository.AddAsync(exercise);

        return exercise.Id;
    }
}