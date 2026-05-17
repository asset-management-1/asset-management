namespace Be.Haven.Tests.Shared.Be.Haven.Cache.Services;

public sealed class LoginMemoryAttemptServiceTests
{
    [Fact]
    public async Task CheckAccountLockedAsync_Should_DoNothing_When_UserNameIsBlank()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var action = () => sut.CheckAccountLockedAsync(" ", CancellationToken.None);

        // Assert
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CountFailedAttemptAsync_Should_LockAccount_When_MaxFailedAttemptsReached()
    {
        // Arrange
        var userName = $"user-{Guid.NewGuid():N}";
        var sut = CreateSut(maxFailedAttempts: 2);

        // Act
        await sut.CountFailedAttemptAsync(userName, CancellationToken.None);
        await sut.CountFailedAttemptAsync(userName, CancellationToken.None);
        var action = () => sut.CheckAccountLockedAsync(userName, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<HttpStatusCodeException>()
            .Where(x => x.StatusCode == AppConstants.SystemCode.MANY_REQUESTS);
    }

    [Fact]
    public async Task RemoveAttemptsAsync_Should_ClearLock_When_UserWasLocked()
    {
        // Arrange
        var userName = $"user-{Guid.NewGuid():N}";
        var sut = CreateSut(maxFailedAttempts: 1);
        await sut.CountFailedAttemptAsync(userName, CancellationToken.None);

        // Act
        await sut.RemoveAttemptsAsync(userName, CancellationToken.None);
        var action = () => sut.CheckAccountLockedAsync(userName, CancellationToken.None);

        // Assert
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CountFailedAttemptAsync_Should_ResetWindow_When_FirstAttemptIsOlderThanWindow()
    {
        // Arrange
        var userName = $"user-{Guid.NewGuid():N}";
        var sut = CreateSut(maxFailedAttempts: 2, failedWindow: TimeSpan.FromMilliseconds(-1));

        // Act
        await sut.CountFailedAttemptAsync(userName, CancellationToken.None);
        await sut.CountFailedAttemptAsync(userName, CancellationToken.None);
        var action = () => sut.CheckAccountLockedAsync(userName, CancellationToken.None);

        // Assert
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CheckAccountLockedAsync_Should_ClearExpiredLock_When_LockDurationAlreadyPassed()
    {
        // Arrange
        var userName = $"user-{Guid.NewGuid():N}";
        var sut = CreateSut(maxFailedAttempts: 1, lockDuration: TimeSpan.FromMilliseconds(-1));
        await sut.CountFailedAttemptAsync(userName, CancellationToken.None);

        // Act
        var action = () => sut.CheckAccountLockedAsync(userName, CancellationToken.None);

        // Assert
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CountFailedAttemptAsync_Should_DoNothing_When_UserNameIsBlank()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var action = () => sut.CountFailedAttemptAsync(" ", CancellationToken.None);

        // Assert
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RemoveAttemptsAsync_Should_DoNothing_When_UserNameIsBlank()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var action = () => sut.RemoveAttemptsAsync(" ", CancellationToken.None);

        // Assert
        await action.Should().NotThrowAsync();
    }

    private static LoginMemoryAttemptService CreateSut(
        int maxFailedAttempts = 3,
        TimeSpan? failedWindow = null,
        TimeSpan? lockDuration = null) =>
        new(
            Mock.Of<ILogger<LoginMemoryAttemptService>>(),
            Options.Create(new LoginAttemptOptions
            {
                MaxFailedAttempts = maxFailedAttempts,
                FailedWindow = failedWindow ?? TimeSpan.FromMinutes(5),
                LockDuration = lockDuration ?? TimeSpan.FromMinutes(10)
            }));
}
