using FitnessCorrector.Application.WorkoutSessions.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCorrector.WebAPI.Controllers;

public class WorkoutUploadRequest
{
    public Guid ExerciseId { get; set; }
    public IFormFile VideoFile { get; set; } = null!;
}

[ApiController]
[Route("api/[controller]")]
public class WorkoutSessionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IWebHostEnvironment _environment;

    public WorkoutSessionsController(IMediator mediator, IWebHostEnvironment environment)
    {
        _mediator = mediator;
        _environment = environment; // Used to get the wwwroot folder path
    }

    [HttpPost("analyze")]
    [Consumes("multipart/form-data")] // This tells Swagger explicitly what to expect
    public async Task<IActionResult> UploadAndAnalyze([FromForm] WorkoutUploadRequest request)
    {
        // Now you access them via 'request'
        if (request.VideoFile == null || request.VideoFile.Length == 0)
            return BadRequest("No file uploaded.");

        // 1. Create the 'uploads' directory if it doesn't exist
        var uploadsFolder = Path.Combine(_environment.ContentRootPath, "uploads");
        Directory.CreateDirectory(uploadsFolder);

        // 2. Generate a unique filename and save the file
        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(request.VideoFile.FileName);
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await request.VideoFile.CopyToAsync(stream);
        }

        // 3. Send the Clean Command to MediatR
        var command = new AnalyzeWorkoutCommand(request.ExerciseId, filePath);
        var sessionId = await _mediator.Send(command);

        return Ok(new { Message = "Video analyzed successfully", SessionId = sessionId });
    }
}