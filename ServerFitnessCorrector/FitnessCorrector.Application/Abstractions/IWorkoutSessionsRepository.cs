using FitnessCorrector.Domain.Entities;

namespace FitnessCorrector.Application.Abstractions;

public interface IWorkoutSessionRepository
{
    Task AddAsync(WorkoutSession session);
    Task UpdateAsync(WorkoutSession session);

    Task<WorkoutSession?> GetByIdAsync(Guid id);
    Task<IEnumerable<WorkoutSession>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<WorkoutSession>> GetLatestAsync(int take);
    Task<int> CountByUserIdAsync(Guid userId);
}