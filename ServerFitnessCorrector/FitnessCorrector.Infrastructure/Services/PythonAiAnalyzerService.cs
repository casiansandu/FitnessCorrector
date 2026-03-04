using System.Diagnostics;
using FitnessCorrector.Application.Abstractions;

namespace ClassLibrary1.Services;

public class PythonAiAnalyzerService : IAiAnalyzerService
{
    public Task<string> AnalyzeVideoAsync(string videoFilePath, Guid exerciseId)
    {
        return Task.Run(() =>
        {
            // Set up the process to run Python
            var start = new ProcessStartInfo
            {
                // Ensure "python" is in your system PATH, or provide the full path to python.exe
                FileName = "python", 
                Arguments = $"main.py \"{videoFilePath}\" \"{exerciseId}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                // Make sure this points to where your main.py actually lives!
                WorkingDirectory = @"C:\Users\casia\OneDrive\Desktop\.net2\Project FitnessCorrector\PoseDetection" 
            };

            using var process = Process.Start(start);
            if (process == null) throw new Exception("Could not start Python process.");

            // Read what Python prints to the console
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception($"Python Script Error: {error}");
            }

            return output; // e.g., "Score: 85, Mistake: Knee angle too high"
        });
    }
}