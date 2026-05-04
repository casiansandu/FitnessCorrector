using FitnessCorrector.Application.Abstractions;
using MediatR;

namespace FitnessCorrector.Application.WorkoutSessions.Queries.GetUserWorkoutSessionsQuery;

public class GetUserWorkoutSessionsQueryHandler : IRequestHandler<GetUserWorkoutSessionsQuery, IEnumerable<WorkoutSessionDto>>
{
    private readonly IWorkoutSessionRepository _repository;

    public GetUserWorkoutSessionsQueryHandler(IWorkoutSessionRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<WorkoutSessionDto>> Handle(GetUserWorkoutSessionsQuery request, CancellationToken cancellationToken)
    {
        var sessions = await _repository.GetByUserIdAsync(request.UserId);

        return sessions.Select(s => new WorkoutSessionDto(
            s.Id,
            s.UserId,
            s.ExerciseId,
            s.VideoFilePath,
            s.Status,
            s.AiFeedback,
            s.OutputPath,
            s.CreatedAt
        ));
    }
}
