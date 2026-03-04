using FitnessCorrector.Application.Abstractions;
using FitnessCorrector.Domain.Entities;
using MediatR;

namespace FitnessCorrector.Application.WorkoutSessions.Commands;

public class AnalyzeWokroutCommandHandler : IRequestHandler<AnalyzeWorkoutCommand, Guid>
{
    private readonly IWorkoutSessionRepository _repository;
    private readonly IAiAnalyzerService _aiService;

    public AnalyzeWokroutCommandHandler(
        IWorkoutSessionRepository repository, 
        IAiAnalyzerService aiService)
    {
        _repository = repository;
        _aiService = aiService;
    }
    
    public async Task<Guid> Handle(AnalyzeWorkoutCommand request, CancellationToken cancellationToken)
    {
        var session = WorkoutSession.Create(request.ExerciseId, request.FilePath);
        await _repository.AddAsync(session);

        try
        {
            var result = await _aiService.AnalyzeVideoAsync(request.FilePath, request.ExerciseId);

            session.MarkAsCompleted(result);
        }
        catch (Exception ex)
        {
            session.MarkAsFailed("AI Analysis Failed: " + ex.Message);
        }

        await _repository.UpdateAsync(session);

        return session.Id;
    }
    
}