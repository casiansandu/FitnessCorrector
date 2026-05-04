namespace FitnessCorrector.Application.Users.Common;

public record AuthenticationResult(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string Token,
    string Role
);
