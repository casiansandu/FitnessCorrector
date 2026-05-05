using FitnessCorrector.Application.Abstractions;
using FitnessCorrector.Application.Exercises.Commands.UploadWorkoutVideoCommand;
using FitnessCorrector.Application.Exercises.Queries.GetSessionLandmarksQuery;
using FitnessCorrector.Application.WorkoutSessions.Queries.GetUserWorkoutSessionsQuery;
using FitnessCorrector.Application.WorkoutSessions.Queries.GetAllWorkoutSessionsQuery;
using FitnessCorrector.Application.WorkoutSessions.Queries.GetUserWorkoutSessionCountQuery;
using FitnessCorrector.Application.WorkoutSessions.Queries.GetWorkoutProgressQuery;
using FitnessCorrector.Application.Subscriptions.Queries.GetUserSubscriptionQuery;
using FitnessCorrector.WebAPI.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitnessCorrector.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkoutSessionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ISubscriptionService _subscriptionService;
    private const int TrialLimit = 10;

    public WorkoutSessionsController(IMediator mediator, ISubscriptionService subscriptionService)
    {
        _mediator = mediator;
        _subscriptionService = subscriptionService;
    }

    [HttpPost("analyze")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadAndAnalyze([FromForm] UploadWorkoutRequest request)
    {
        if (request.VideoFile == null || request.VideoFile.Length == 0)
            return BadRequest("No file uploaded.");

        // Get authenticated user's ID from JWT token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid or missing user authentication" });
        }

        var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst("email")?.Value;
        var subscription = await _mediator.Send(new GetUserSubscriptionQuery(userId));

        if (subscription == null && !string.IsNullOrWhiteSpace(emailClaim))
        {
            await _subscriptionService.SyncUserSubscriptionAsync(userId, emailClaim, HttpContext.RequestAborted);
            subscription = await _mediator.Send(new GetUserSubscriptionQuery(userId));
        }

        if (subscription == null)
        {
            var usage = await _mediator.Send(new GetUserWorkoutSessionCountQuery(userId, TrialLimit));
            if (usage.TotalCount >= TrialLimit)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "Trial limit reached. Please subscribe to continue analyzing videos."
                });
            }
        }
        else if (!IsAnalysisAllowed(subscription.Status))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = "You need an active subscription before sending videos for analysis."
            });
        }

        await using var videoStream = request.VideoFile.OpenReadStream();

        var command = new UploadWorkoutVideoCommand(
            userId,
            request.ExerciseId,
            request.Slug ?? "unknown",
            videoStream,
            request.VideoFile.FileName);

        var session = await _mediator.Send(command);

        return Ok(new
        {
            Message = "Analysis initiated",
            SessionId = session.Id,
            Status = session.Status
        });
    }

    [HttpGet("trial-usage")]
    public async Task<IActionResult> GetTrialUsage()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid or missing user authentication" });
        }

        var subscription = await _mediator.Send(new GetUserSubscriptionQuery(userId));
        if (subscription != null)
        {
            return Ok(new
            {
                isSubscriber = true,
                totalCount = 0,
                remainingCount = TrialLimit,
                limit = TrialLimit
            });
        }

        var usage = await _mediator.Send(new GetUserWorkoutSessionCountQuery(userId, TrialLimit));
        return Ok(new
        {
            isSubscriber = false,
            totalCount = usage.TotalCount,
            remainingCount = usage.RemainingCount,
            limit = usage.Limit
        });
    }

    private static bool IsAnalysisAllowed(string subscriptionStatus)
    {
        return string.Equals(subscriptionStatus, "Active", StringComparison.OrdinalIgnoreCase)
               || string.Equals(subscriptionStatus, "Trialing", StringComparison.OrdinalIgnoreCase);
    }

    [HttpGet("{id}/landmarks")]
    public async Task<IActionResult> GetLandmarks(Guid id)
    {
        try
        {
            // Get authenticated user's ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid or missing user authentication" });
            }

            var query = new GetSessionLandmarksQuery(id, userId);
            var jsonContent = await _mediator.Send(query);

            if (jsonContent == null)
            {
                return NotFound(new { Message = "Session or landmark data not found." });
            }

            return Content(jsonContent, "application/json");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Message = "An error occurred while retrieving landmarks.",
                Details = ex.Message
            });
        }
    }

    [HttpGet("my-sessions")]
    public async Task<IActionResult> GetMyWorkoutSessions()
    {
        try
        {
            // Get authenticated user's ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid or missing user authentication" });
            }

            var query = new GetUserWorkoutSessionsQuery(userId);
            var sessions = await _mediator.Send(query);

            return Ok(sessions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Message = "An error occurred while retrieving workout sessions.",
                Details = ex.Message
            });
        }
    }

    [HttpGet("admin-sessions")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAdminWorkoutSessions([FromQuery] int take = 25)
    {
        var query = new GetAllWorkoutSessionsQuery(take);
        var sessions = await _mediator.Send(query);
        return Ok(sessions);
    }

    [HttpGet("progress")]
    public async Task<IActionResult> GetProgress([FromQuery] Guid exerciseId, [FromQuery] int rangeDays = 30)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid or missing user authentication" });
        }

        if (exerciseId == Guid.Empty)
        {
            return BadRequest(new { message = "ExerciseId is required." });
        }

        var query = new GetWorkoutProgressQuery(userId, exerciseId, rangeDays);
        var progress = await _mediator.Send(query);

        return Ok(progress);
    }

    [HttpGet("highlights")]
    public async Task<IActionResult> GetHighlights([FromQuery] Guid exerciseId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid or missing user authentication" });
        }

        if (exerciseId == Guid.Empty)
        {
            return BadRequest(new { message = "ExerciseId is required." });
        }

        var progress = await _mediator.Send(new GetWorkoutProgressQuery(userId, exerciseId, 60));
        if (progress.Count < 2)
        {
            return Ok(new
            {
                message = "Complete at least two sessions to unlock improvements.",
                metric = "depth",
                delta = 0.0
            });
        }

        var latest = progress[^1];
        var previous = progress[^2];
        var depthDelta = latest.AverageDepth - previous.AverageDepth;
        var tempoDelta = latest.AverageTempoSeconds - previous.AverageTempoSeconds;
        var symmetryDelta = latest.AverageSymmetry - previous.AverageSymmetry;

        var bestMetric = new[]
        {
            (Key: "depth", Delta: depthDelta, Latest: latest.AverageDepth),
            (Key: "tempo", Delta: -tempoDelta, Latest: latest.AverageTempoSeconds),
            (Key: "symmetry", Delta: symmetryDelta, Latest: latest.AverageSymmetry)
        }.OrderByDescending(entry => entry.Delta).First();

        var message = bestMetric.Key switch
        {
            "tempo" => $"Tempo improved by {Math.Abs(tempoDelta):0.00}s since last session.",
            "symmetry" => $"Symmetry improved by {Math.Abs(symmetryDelta):0.00} since last session.",
            _ => $"Depth improved by {Math.Abs(depthDelta):0.00} since last session."
        };

        return Ok(new
        {
            message,
            metric = bestMetric.Key,
            delta = bestMetric.Delta
        });
    }
}