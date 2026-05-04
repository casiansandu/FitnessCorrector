using FitnessCorrector.Application.Abstractions;
using FitnessCorrector.Application.Users.Queries.GetUserByIdQuery;
using FitnessCorrector.Domain.Entities;
using Moq;

namespace FitnessCorrector.Tests;

public class GetUserByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Throw_When_User_Not_Found()
    {
        var repository = new Mock<IUsersRepository>();
        var userId = Guid.NewGuid();

        repository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var handler = new GetUserByIdQueryHandler(repository.Object);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.Handle(new GetUserByIdQuery(userId), CancellationToken.None));

        Assert.Equal($"User with ID {userId} not found", ex.Message);
    }

    [Fact]
    public async Task Handle_Should_Return_UserDto_When_User_Found()
    {
        var repository = new Mock<IUsersRepository>();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            FirstName = "Jane",
            LastName = "Doe",
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        repository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new GetUserByIdQueryHandler(repository.Object);

        var result = await handler.Handle(new GetUserByIdQuery(user.Id), CancellationToken.None);

        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(user.FirstName, result.FirstName);
        Assert.Equal(user.LastName, result.LastName);
        Assert.Equal(user.CreatedAt, result.CreatedAt);
    }
}
