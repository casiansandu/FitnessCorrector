using FitnessCorrector.Application.Abstractions;
using FitnessCorrector.Application.WorkoutSessions.Queries.GetUserWorkoutSessionsQuery;
using MediatR;

namespace FitnessCorrector.Application.WorkoutSessions.Queries.GetAllWorkoutSessionsQuery;

public class GetAllWorkoutSessionsQueryHandler : IRequestHandler<GetAllWorkoutSessionsQuery, IEnumerable<WorkoutSessionDto>>
{
    private readonly IWorkoutSessionRepository _repository;

    public GetAllWorkoutSessionsQueryHandler(IWorkoutSessionRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<WorkoutSessionDto>> Handle(GetAllWorkoutSessionsQuery request, CancellationToken cancellationToken)
    {
        var sessions = await _repository.GetLatestAsync(request.Take);

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
