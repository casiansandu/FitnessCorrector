using FitnessCorrector.Domain.Entities;

namespace FitnessCorrector.Application.Abstractions;

public interface IExercisesRepository
{
    Task AddAsync(Exercise exercise);
    Task<List<Exercise>> GetAllAsync();
}