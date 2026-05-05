using FitnessCorrector.Application.Abstractions;
using FitnessCorrector.Domain.Entities;
using FitnessCorrector.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitnessCorrector.Infrastructure.Repositories;

public class WorkoutSessionsRepository : IWorkoutSessionRepository
{
    private readonly AppDbContext _context;

    public WorkoutSessionsRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task AddAsync(WorkoutSession workoutSession)
    {
        // 1. Tell EF Core to track this new entity
        await _context.WorkoutSessions.AddAsync(workoutSession);
        
        // 2. Actually save it to the database
        await _context.SaveChangesAsync(); 
    }

    public async Task UpdateAsync(WorkoutSession workoutSession)
    {
        // 1. Tell EF Core this entity has been modified
        _context.WorkoutSessions.Update(workoutSession);
        
        // 2. Commit the changes to the database
        await _context.SaveChangesAsync();
    }

    public async Task<WorkoutSession?> GetByIdAsync(Guid id)
    {
        // FindAsync returns the entity or null if not found
        return await _context.WorkoutSessions.FindAsync(id);
    }

    public async Task<IEnumerable<WorkoutSession>> GetByUserIdAsync(Guid userId)
    {
        return await _context.WorkoutSessions
            .Where(ws => ws.UserId == userId)
            .OrderByDescending(ws => ws.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkoutSession>> GetLatestAsync(int take)
    {
        var safeTake = take <= 0 ? 20 : take;
        return await _context.WorkoutSessions
            .OrderByDescending(ws => ws.CreatedAt)
            .Take(safeTake)
            .ToListAsync();
    }

    public async Task<int> CountByUserIdAsync(Guid userId)
    {
        return await _context.WorkoutSessions
            .CountAsync(ws => ws.UserId == userId);
    }
}