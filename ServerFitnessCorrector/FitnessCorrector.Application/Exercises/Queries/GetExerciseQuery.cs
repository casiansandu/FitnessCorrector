namespace FitnessCorrector.Application.Exercises.Queries;

using MediatR;

public record GetExercisesQuery : IRequest<List<ExerciseDto>>;