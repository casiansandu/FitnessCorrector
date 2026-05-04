namespace FitnessCorrector.WebAPI.Models;

public class UploadWorkoutRequest
{
    public Guid ExerciseId { get; set; }
    public string? Slug { get; set; }
    public IFormFile VideoFile { get; set; } = null!;
}
