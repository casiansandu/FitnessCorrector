using FitnessCorrector.Application.Abstractions;
using FitnessCorrector.Domain.Entities;
using MediatR;

namespace FitnessCorrector.Application.Exercises.Commands.AnalyzeWorkoutCommand;

public class AnalyzeWokroutCommandHandler : IRequestHandler<AnalyzeWorkoutCommand, WorkoutSession>
{
    private readonly IWorkoutSessionRepository _repository;
    private readonly IAiAnalyzerService _aiService;
    private readonly IWorkoutSessionMetricsRepository _metricsRepository;

    public AnalyzeWokroutCommandHandler(
        IWorkoutSessionRepository repository,
        IAiAnalyzerService aiService,
        IWorkoutSessionMetricsRepository metricsRepository)
    {
        _repository = repository;
        _aiService = aiService;
        _metricsRepository = metricsRepository;
    }

    public async Task<WorkoutSession> Handle(Exercises.Commands.AnalyzeWorkoutCommand.AnalyzeWorkoutCommand request, CancellationToken cancellationToken)
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
                await SaveMetricsAsync(session, request.Slug, outputPath);
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

    private async Task SaveMetricsAsync(WorkoutSession session, string exerciseSlug, string outputPath)
    {
        if (!File.Exists(outputPath))
        {
            return;
        }
        try
        {
            using var stream = File.OpenRead(outputPath);
            using var doc = System.Text.Json.JsonDocument.Parse(stream);

        if (!doc.RootElement.TryGetProperty("metrics", out var metricsElement))
        {
            return;
        }

        if (!metricsElement.TryGetProperty("session", out var sessionMetricsElement))
        {
            return;
        }

        var avgDepth = GetDouble(sessionMetricsElement, "avg_depth");
        var avgTempo = GetDouble(sessionMetricsElement, "avg_tempo_seconds");
        var avgSymmetry = GetDouble(sessionMetricsElement, "avg_symmetry");
        var repCount = GetInt(sessionMetricsElement, "rep_count");

        var sessionMetrics = WorkoutSessionMetrics.Create(
            session.Id,
            session.UserId,
            session.ExerciseId,
            exerciseSlug,
            avgDepth,
            avgTempo,
            avgSymmetry,
            repCount);

        var repMetrics = new List<WorkoutSessionRepMetric>();
        if (metricsElement.TryGetProperty("reps", out var repsElement) && repsElement.ValueKind == System.Text.Json.JsonValueKind.Array)
        {
            foreach (var rep in repsElement.EnumerateArray())
            {
                var repIndex = GetInt(rep, "rep_index");
                var depth = GetDouble(rep, "depth");
                var total = GetDouble(rep, "tempo_total_seconds");
                var eccentric = GetDouble(rep, "tempo_eccentric_seconds");
                var concentric = GetDouble(rep, "tempo_concentric_seconds");
                var symmetry = GetDouble(rep, "symmetry");

                repMetrics.Add(WorkoutSessionRepMetric.Create(
                    session.Id,
                    repIndex,
                    depth,
                    total,
                    eccentric,
                    concentric,
                    symmetry));
            }
        }

            await _metricsRepository.AddSessionMetricsAsync(sessionMetrics, repMetrics);
        }
        catch (Exception)
        {
            // Swallow parsing issues to avoid failing the session.
        }
    }

    private static double GetDouble(System.Text.Json.JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.TryGetDouble(out var value)
            ? value
            : 0.0;
    }

    private static int GetInt(System.Text.Json.JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.TryGetInt32(out var value)
            ? value
            : 0;
    }
}