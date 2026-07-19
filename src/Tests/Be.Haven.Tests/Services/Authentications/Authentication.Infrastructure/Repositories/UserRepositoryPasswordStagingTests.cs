using Authentication.Domain.Entities;
using Authentication.Infrastructure.Context;
using Authentication.Infrastructure.Repositories;

namespace Be.Haven.Tests.Services.Authentications.Authentication.Infrastructure.Repositories;

public sealed class UserRepositoryPasswordStagingTests
{
    [Fact]
    public async Task StagePasswordHashChangeAsync_Should_ReuseAlreadyTrackedLockedUser()
    {
        var options = new DbContextOptionsBuilder<AuthenticationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var context = new AuthenticationDbContext(options);
        var trackedUser = new User
        {
            Id = 42,
            PublicId = Guid.NewGuid(),
            UserName = "tracked-user",
            Email = "tracked@example.com",
            FullName = "Tracked User"
        };
        context.Users.Add(trackedUser);
        await context.SaveChangesAsync();
        var sut = new UserRepository(
            context,
            Mock.Of<IDapperService>(),
            Mock.Of<IJsonSerializerService>());
        var authResetAt = DateTime.UtcNow;

        Func<Task> act = () => sut.StagePasswordHashChangeAsync(
            trackedUser.Id,
            "new-hash",
            authResetAt,
            authResetAt);

        await act.Should().NotThrowAsync();
        context.ChangeTracker.Entries<User>().Should().ContainSingle();
        trackedUser.PasswordHash.Should().Be("new-hash");
        trackedUser.AuthResetAt.Should().Be(authResetAt);
        trackedUser.UpdatedAt.Should().Be(authResetAt);
    }
}
