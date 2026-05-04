using FitnessCorrector.Application.Users.Common;
using MediatR;

namespace FitnessCorrector.Application.Users.Queries.GetUserByIdQuery;

public record GetUserByIdQuery(Guid UserId) : IRequest<UserDto>;
