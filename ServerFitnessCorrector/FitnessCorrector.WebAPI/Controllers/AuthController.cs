using FitnessCorrector.Application.Users.Commands.LoginUserCommand;
using FitnessCorrector.Application.Users.Commands.RegisterUserCommand;
using FitnessCorrector.Application.Users.Queries.GetUserByIdQuery;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCorrector.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ISender _mediator;

    public AuthController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(command, cancellationToken);

            // Set HttpOnly cookie with JWT token
            Response.Cookies.Append("fitnessCorrectorToken", result.Token, new CookieOptions
            {
                HttpOnly = true,              // JavaScript can't access it (XSS protection)
                Secure = false,               // Set to true in production (requires HTTPS)
                SameSite = SameSiteMode.Lax,  // Lax works for localhost development
                MaxAge = TimeSpan.FromMinutes(60)
            });

            return Ok(result);
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors
                .Select(e => new
                {
                    property = e.PropertyName,
                    message = e.ErrorMessage
                })
                .ToList();
            return BadRequest(new { errors });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while processing your request" });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(command, cancellationToken);

            // Set HttpOnly cookie with JWT token
            Response.Cookies.Append("fitnessCorrectorToken", result.Token, new CookieOptions
            {
                HttpOnly = true,              // JavaScript can't access it (XSS protection)
                Secure = false,               // Set to true in production (requires HTTPS)
                SameSite = SameSiteMode.Lax,  // Lax works for localhost development
                MaxAge = TimeSpan.FromMinutes(60)
            });

            return Ok(result);
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors
                .Select(e => new
                {
                    property = e.PropertyName,
                    message = e.ErrorMessage
                })
                .ToList();
            return BadRequest(new { errors });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while processing your request" });
        }
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // Clear the HttpOnly cookie
        Response.Cookies.Delete("fitnessCorrectorToken");
        return Ok(new { message = "Logged out successfully" });
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetUserById(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetUserByIdQuery(userId);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while processing your request" });
        }
    }
}
