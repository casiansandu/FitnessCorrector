using FitnessCorrector.Application.Abstractions;
using FitnessCorrector.Domain.Entities;
using FitnessCorrector.Infrastructure.Persistence;

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
}