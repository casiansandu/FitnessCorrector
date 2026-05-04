namespace FitnessCorrector.Application.Exercises.Queries.GetSessionLandmarksQuery;

using MediatR;

public record GetSessionLandmarksQuery(Guid Id, Guid UserId) : IRequest<string?>;