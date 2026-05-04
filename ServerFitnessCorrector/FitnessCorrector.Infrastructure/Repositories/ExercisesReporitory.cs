using FitnessCorrector.Application.Abstractions;
using FitnessCorrector.Domain.Entities;
using FitnessCorrector.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FitnessCorrector.Infrastructure.Repositories;

public class ExercisesRepository : IExercisesRepository
{
    private readonly AppDbContext _context;

    public ExercisesRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Exercise exercise)
    {
        await _context.Exercises.AddAsync(exercise);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Exercise>> GetAllAsync()
    {
        return await _context.Exercises.ToListAsync();
    }
}