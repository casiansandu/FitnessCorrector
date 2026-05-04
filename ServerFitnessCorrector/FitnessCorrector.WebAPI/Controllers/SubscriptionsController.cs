using FitnessCorrector.Application.Subscriptions.Commands.CancelSubscriptionCommand;
using FitnessCorrector.Application.Subscriptions.Commands.CreateSubscriptionCommand;
using FitnessCorrector.Application.Subscriptions.Commands.UpdateSubscriptionCommand;
using FitnessCorrector.Application.Subscriptions.Queries.GetPricingPlansQuery;
using FitnessCorrector.Application.Subscriptions.Queries.GetUserSubscriptionQuery;
using FitnessCorrector.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FitnessCorrector.Application.Abstractions;

namespace FitnessCorrector.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionsController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly ISubscriptionService _subscriptionService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SubscriptionsController> _logger;

    public SubscriptionsController(
        ISender mediator,
        ISubscriptionService subscriptionService,
        IConfiguration configuration,
        ILogger<SubscriptionsController> logger)
    {
        _mediator = mediator;
        _subscriptionService = subscriptionService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet("pricing-plans")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPricingPlans(CancellationToken cancellationToken)
    {
        try
        {
            var pricingPlans = await _mediator.Send(new GetPricingPlansQuery(), cancellationToken);
            return Ok(pricingPlans);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching pricing plans: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred while fetching pricing plans" });
        }
    }

    [HttpPost("create")]
    [Authorize]
    public async Task<IActionResult> CreateSubscription(
        [FromBody] CreateSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserIdFromToken();
            var userEmail = GetEmailFromToken();

            var command = new CreateSubscriptionCommand(userId, userEmail, request.PlanType);
            var result = await _mediator.Send(command, cancellationToken);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating subscription: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred while creating subscription" });
        }
    }

    [HttpGet("my-subscription")]
    [Authorize]
    public async Task<IActionResult> GetMySubscription(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserIdFromToken();
            var subscription = await _mediator.Send(new GetUserSubscriptionQuery(userId), cancellationToken);

            if (subscription == null)
            {
                var email = GetEmailFromToken();
                await _subscriptionService.SyncUserSubscriptionAsync(userId, email, cancellationToken);
                subscription = await _mediator.Send(new GetUserSubscriptionQuery(userId), cancellationToken);
            }

            if (subscription == null)
            {
                return NotFound(new { message = "No subscription found" });
            }

            return Ok(subscription);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching subscription: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred while fetching subscription" });
        }
    }

    [HttpPut("change-plan")]
    [Authorize]
    public async Task<IActionResult> ChangePlan(
        [FromBody] ChangePlanRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserIdFromToken();

            var command = new UpdateSubscriptionCommand(
                userId,
                request.StripeSubscriptionId,
                request.NewPlanType);

            var result = await _mediator.Send(command, cancellationToken);

            return Ok(new { success = result });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating subscription: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred while updating subscription" });
        }
    }

    [HttpDelete("cancel")]
    [Authorize]
    public async Task<IActionResult> CancelSubscription(
        [FromQuery] string? stripeSubscriptionId,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserIdFromToken();
            var effectiveStripeSubscriptionId = stripeSubscriptionId;

            if (string.IsNullOrWhiteSpace(effectiveStripeSubscriptionId))
            {
                var currentSubscription = await _mediator.Send(new GetUserSubscriptionQuery(userId), cancellationToken);
                if (currentSubscription == null)
                {
                    return NotFound(new { message = "No active subscription found" });
                }

                effectiveStripeSubscriptionId = currentSubscription.StripeSubscriptionId;
            }

            var command = new CancelSubscriptionCommand(userId, effectiveStripeSubscriptionId);
            var result = await _mediator.Send(command, cancellationToken);

            return Ok(new { success = result });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error canceling subscription: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred while canceling subscription" });
        }
    }

    [HttpPost("webhook")]
    [HttpPost("/api/stripe/webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> HandleWebhook(CancellationToken cancellationToken)
    {
        try
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var signature = Request.Headers["Stripe-Signature"].ToString();

            if (string.IsNullOrEmpty(signature))
            {
                return BadRequest(new { message = "Invalid signature" });
            }

            var stripeWebhookSecret = _configuration["Stripe:WebhookSecret"];
            if (string.IsNullOrEmpty(stripeWebhookSecret))
            {
                _logger.LogError("Stripe webhook secret not configured");
                return StatusCode(500);
            }

            // Note: The actual webhook processing is done in the service
            // Here we just verify and pass through
            var success = await _mediator.Send(
                new HandleStripeWebhookCommand(json, signature),
                cancellationToken);

            return success ? Ok(new { received = true }) : BadRequest();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Webhook processing error: {ex.Message}");
            return BadRequest();
        }
    }

    private Guid GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        throw new UnauthorizedAccessException("Unable to retrieve user ID from token");
    }

    private string GetEmailFromToken()
    {
        var emailClaim = User.FindFirst(ClaimTypes.Email) ?? User.FindFirst("email");
        if (emailClaim != null)
        {
            return emailClaim.Value;
        }

        throw new UnauthorizedAccessException("Unable to retrieve email from token");
    }
}

// Request DTOs
public record CreateSubscriptionRequest(PlanType PlanType);
public record ChangePlanRequest(string StripeSubscriptionId, PlanType NewPlanType);

// Webhook handling command
public record HandleStripeWebhookCommand(string Json, string Signature) : IRequest<bool>;

public class HandleStripeWebhookCommandHandler : IRequestHandler<HandleStripeWebhookCommand, bool>
{
    private readonly ISubscriptionService _subscriptionService;

    public HandleStripeWebhookCommandHandler(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    public async Task<bool> Handle(HandleStripeWebhookCommand request, CancellationToken cancellationToken)
    {
        return await _subscriptionService.ProcessWebhookAsync(request.Json, request.Signature, cancellationToken);
    }
}
