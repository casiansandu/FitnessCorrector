using FitnessCorrector.Domain.Entities;

namespace FitnessCorrector.Application.Abstractions;

public interface IWorkoutSessionMetricsRepository
{
    Task AddSessionMetricsAsync(WorkoutSessionMetrics sessionMetrics, IReadOnlyCollection<WorkoutSessionRepMetric> repMetrics);
    Task<List<WorkoutSessionMetrics>> GetMetricsForUserExerciseAsync(Guid userId, Guid exerciseId, DateTime sinceUtc);
}
