using FitnessCorrector.Application.Abstractions;
using FitnessCorrector.Domain.Entities;
using FitnessCorrector.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FitnessCorrector.Infrastructure.Repositories;

public class WorkoutSessionMetricsRepository : IWorkoutSessionMetricsRepository
{
    private readonly AppDbContext _context;

    public WorkoutSessionMetricsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddSessionMetricsAsync(WorkoutSessionMetrics sessionMetrics, IReadOnlyCollection<WorkoutSessionRepMetric> repMetrics)
    {
        await _context.WorkoutSessionMetrics.AddAsync(sessionMetrics);

        if (repMetrics.Count > 0)
        {
            await _context.WorkoutSessionRepMetrics.AddRangeAsync(repMetrics);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<WorkoutSessionMetrics>> GetMetricsForUserExerciseAsync(Guid userId, Guid exerciseId, DateTime sinceUtc)
    {
        return await _context.WorkoutSessionMetrics
            .Where(m => m.UserId == userId && m.ExerciseId == exerciseId && m.CreatedAt >= sinceUtc)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }
}
