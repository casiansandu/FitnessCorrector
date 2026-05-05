using FitnessCorrector.Application.Abstractions;
using MediatR;

namespace FitnessCorrector.Application.WorkoutSessions.Queries.GetWorkoutProgressQuery;

public class GetWorkoutProgressQueryHandler : IRequestHandler<GetWorkoutProgressQuery, List<WorkoutProgressPointDto>>
{
    private readonly IWorkoutSessionMetricsRepository _repository;

    public GetWorkoutProgressQueryHandler(IWorkoutSessionMetricsRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<WorkoutProgressPointDto>> Handle(GetWorkoutProgressQuery request, CancellationToken cancellationToken)
    {
        var rangeDays = request.RangeDays <= 0 ? 30 : request.RangeDays;
        var since = DateTime.UtcNow.AddDays(-rangeDays);
        var metrics = await _repository.GetMetricsForUserExerciseAsync(request.UserId, request.ExerciseId, since);

        return metrics.Select(m => new WorkoutProgressPointDto(
            m.CreatedAt,
            m.AverageDepth,
            m.AverageTempoSeconds,
            m.AverageSymmetry,
            m.RepCount
        )).ToList();
    }
}
