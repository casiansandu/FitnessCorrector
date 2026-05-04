using FitnessCorrector.Application.Abstractions;
using FitnessCorrector.Domain.Entities;
using MediatR;

namespace FitnessCorrector.Application.Exercises.Commands.UploadWorkoutVideoCommand;

public class UploadWorkoutVideoCommandHandler : IRequestHandler<UploadWorkoutVideoCommand, WorkoutSession>
{
    private readonly IWorkoutSessionRepository _repository;
    private readonly IAiAnalyzerService _aiService;

    public UploadWorkoutVideoCommandHandler(
        IWorkoutSessionRepository repository,
        IAiAnalyzerService aiService)
    {
        _repository = repository;
        _aiService = aiService;
    }

    public async Task<WorkoutSession> Handle(UploadWorkoutVideoCommand request, CancellationToken cancellationToken)
    {
        var session = WorkoutSession.Create(request.UserId, request.ExerciseId, request.FileName);
        await _repository.AddAsync(session);

        var (aiFeedback, outputPath) = await _aiService.AnalyzeVideoAsync(
            session.Id,
            request.ExerciseId,
            request.Slug,
            request.VideoStream,
            request.FileName);

        try
        {
            if (File.Exists(outputPath))
            {
                session.MarkAsCompleted(aiFeedback, outputPath);
            }
            else
            {
                session.MarkAsFailed(aiFeedback);
            }
        }
        catch (Exception ex)
        {
            session.MarkAsFailed("AI Analysis Failed: " + ex.Message);
        }

        await _repository.UpdateAsync(session);

        return session;
    }
}