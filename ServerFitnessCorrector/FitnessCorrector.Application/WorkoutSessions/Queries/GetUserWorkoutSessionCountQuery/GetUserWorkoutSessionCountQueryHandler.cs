using FitnessCorrector.Application.Abstractions;
using MediatR;

namespace FitnessCorrector.Application.WorkoutSessions.Queries.GetUserWorkoutSessionCountQuery;

public class GetUserWorkoutSessionCountQueryHandler : IRequestHandler<GetUserWorkoutSessionCountQuery, GetUserWorkoutSessionCountDto>
{
    private readonly IWorkoutSessionRepository _repository;

    public GetUserWorkoutSessionCountQueryHandler(IWorkoutSessionRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetUserWorkoutSessionCountDto> Handle(GetUserWorkoutSessionCountQuery request, CancellationToken cancellationToken)
    {
        var total = await _repository.CountByUserIdAsync(request.UserId);
        var remaining = Math.Max(request.Limit - total, 0);
        return new GetUserWorkoutSessionCountDto(total, remaining, request.Limit);
    }
}
