namespace FitnessCorrector.Application.Abstractions;

public interface IAiAnalyzerService
{
    Task<(string, string)> AnalyzeVideoAsync(
        Guid workoutSessionId,
        Guid exerciseId,
        string exerciseSlug,
        Stream videoStream,
        string fileName);
}