using FitnessCorrector.Domain.Enums;
using MediatR;

namespace FitnessCorrector.Application.Exercises.Commands;

public record CreateExerciseCommand(
    string Name, 
    string Description, 
    MuscleGroup MuscleGroup
) : IRequest<Guid>;