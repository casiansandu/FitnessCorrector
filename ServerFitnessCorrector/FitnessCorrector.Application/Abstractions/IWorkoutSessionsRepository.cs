using FitnessCorrector.Domain.Entities;

namespace FitnessCorrector.Application.Abstractions;

public interface IWorkoutSessionRepository
{
    Task AddAsync(WorkoutSession session);
    Task UpdateAsync(WorkoutSession session);
}