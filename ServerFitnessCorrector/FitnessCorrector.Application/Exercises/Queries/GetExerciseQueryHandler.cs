using FitnessCorrector.Application.Abstractions;

namespace FitnessCorrector.Application.Exercises.Queries;

using FitnessCorrector.Application.Abstractions;
using MediatR;

public class GetExercisesQueryHandler : IRequestHandler<GetExercisesQuery, List<ExerciseDto>>
{
    private readonly IExercisesRepository _repository;

    public GetExercisesQueryHandler(IExercisesRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ExerciseDto>> Handle(GetExercisesQuery request, CancellationToken cancellationToken)
    {
        var exercises = await _repository.GetAllAsync();

        return exercises.Select(e => new ExerciseDto(
            e.Id, 
            e.Name, 
            e.TargetMuscleGroup.ToString()
        )).ToList();
    }
}