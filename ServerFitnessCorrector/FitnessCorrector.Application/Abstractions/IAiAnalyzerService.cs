namespace FitnessCorrector.Application.Abstractions;

public interface IAiAnalyzerService
{
    Task<string> AnalyzeVideoAsync(string videoFilePath, Guid exerciseId);
}