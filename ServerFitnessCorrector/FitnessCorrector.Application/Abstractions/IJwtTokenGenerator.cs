using FitnessCorrector.Domain.Entities;

namespace FitnessCorrector.Application.Abstractions;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
