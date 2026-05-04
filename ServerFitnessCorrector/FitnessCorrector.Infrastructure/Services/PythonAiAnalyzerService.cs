using System.Diagnostics;
using FitnessCorrector.Application.Abstractions;
using Microsoft.EntityFrameworkCore.Query;

namespace ClassLibrary1.Services;

public class PythonAiAnalyzerService : IAiAnalyzerService
{
    public async Task<(string, string)> AnalyzeVideoAsync(
        Guid workoutSessionId,
        Guid exerciseId,
        string exerciseSlug,
        Stream videoStream,
        string fileName)
    {
        var baseFolder = @"C:\Users\casia\OneDrive\Desktop\.net2\Project FitnessCorrector\PoseDetection";
        var tempVideoPath = Path.Combine(baseFolder, "temp", $"{workoutSessionId}{Path.GetExtension(fileName)}");
        var outputPath = Path.Combine(baseFolder, "PoseDetectionResults", $"{workoutSessionId}.json");

        Directory.CreateDirectory(Path.GetDirectoryName(tempVideoPath)!);
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        await using (var fileStream = new FileStream(tempVideoPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await videoStream.CopyToAsync(fileStream);
        }

        var start = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = $"main.py \"{workoutSessionId}\" \"{tempVideoPath}\" \"{exerciseSlug}\" \"{outputPath}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = baseFolder
        };

        using var process = Process.Start(start);
        if (process == null)
            throw new Exception("Could not start Python process.");

        string aiFeedback = await process.StandardOutput.ReadToEndAsync();
        string error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();
    
        if (process.ExitCode != 0)
        {
            throw new Exception($"Python Script Error: {error}");
        }
        
        try
        {
            if (File.Exists(tempVideoPath))
            {
                File.Delete(tempVideoPath);
            }
        }
        catch (Exception ex)
        {
            // Log the error if needed, but don't throw - cleanup failure shouldn't fail the entire operation
            // Example: _logger.LogWarning(ex, "Failed to delete temporary video file at {Path}", tempVideoPath);
        }
        
        return (aiFeedback, outputPath);
    }
}