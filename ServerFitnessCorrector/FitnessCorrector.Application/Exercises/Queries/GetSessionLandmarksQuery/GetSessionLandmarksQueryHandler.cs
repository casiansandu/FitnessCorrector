using FitnessCorrector.Application.Abstractions;

namespace FitnessCorrector.Application.Exercises.Queries.GetSessionLandmarksQuery;

using MediatR;

public class GetSessionLandmarksHandler : IRequestHandler<GetSessionLandmarksQuery, string?>
{
    private readonly IWorkoutSessionRepository _repository;

    public GetSessionLandmarksHandler(IWorkoutSessionRepository repository)
    {
        _repository = repository;
    }

    public async Task<string?> Handle(GetSessionLandmarksQuery request, CancellationToken cancellationToken)
    {
        var session = await _repository.GetByIdAsync(request.Id);

        if (session == null || string.IsNullOrEmpty(session.OutputPath))
        {
            return null;
        }

        // Authorization check: Ensure the session belongs to the requesting user
        if (session.UserId != request.UserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to access this workout session.");
        }

        if (!File.Exists(session.OutputPath))
        {
            throw new FileNotFoundException("The analysis file is missing on the server.");
        }

        // Read the file content to return it to the controller

        return await File.ReadAllTextAsync(session.OutputPath, cancellationToken);
    }
}