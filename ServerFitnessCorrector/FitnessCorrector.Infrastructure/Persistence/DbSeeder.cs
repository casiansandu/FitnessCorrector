using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using FitnessCorrector.Domain.Entities;
using FitnessCorrector.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FitnessCorrector.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAdminUserAsync(AppDbContext context, IConfiguration configuration)
    {
        var adminEmail = configuration["AdminEmail"];
        var adminPassword = configuration["AdminPassword"];

        if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
        {
            return; // No admin credentials configured
        }

        // Check if admin already exists
        var adminExists = await context.Users.AnyAsync(u => u.Email == adminEmail);

        if (adminExists)
        {
            return; // Admin already exists
        }

        // Hash password with SHA256 (same as frontend)
        var passwordHash = HashPasswordSha256(adminPassword);

        // Create admin user
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Email = adminEmail,
            PasswordHash = passwordHash,
            FirstName = "Admin",
            LastName = "User",
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(adminUser);
        await context.SaveChangesAsync();
    }

    public static async Task SeedExercisesAsync(AppDbContext context)
    {
        if (await context.Exercises.AnyAsync())
        {
            return;
        }

        var exercises = new List<Exercise>
        {
            Exercise.Create("squat", "Back Squat", "Barbell back squat", MuscleGroup.Legs),
            Exercise.Create("deadlift", "Deadlift", "Conventional deadlift", MuscleGroup.Back),
            Exercise.Create("bench-press", "Bench Press", "Barbell bench press", MuscleGroup.Chest)
        };

        context.Exercises.AddRange(exercises);
        await context.SaveChangesAsync();
    }

    private static string HashPasswordSha256(string password)
    {
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLower();
    }
}
