using FitnessCorrector.Domain.Enums;

namespace FitnessCorrector.Domain.Entities;

public class Exercise
{
    private Exercise()
    {
        Slug = string.Empty;
    }

    public static Exercise Create(string slug, string name, string description, MuscleGroup targetMuscleGroup)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name), "Exercise name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentNullException(nameof(description), "Description cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new ArgumentNullException(nameof(slug), "Slug cannot be empty.");
        }

        return new Exercise
        {
            Id = Guid.NewGuid(),
            Slug = slug,
            Name = name,
            Description = description,
            TargetMuscleGroup = targetMuscleGroup
        };
    }

    public Guid Id { get; private set; }
    
    public string Slug { get; private set; }
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public MuscleGroup TargetMuscleGroup { get; private set; }
}