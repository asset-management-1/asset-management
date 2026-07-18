using static Be.Haven.Shared.Constants.Cache.RedisConstants.EnvironmentVariables;

namespace Be.Haven.Tests.Shared.Be.Haven.Cache.Services;

public sealed class LoginAttemptServiceTests
{
    [Fact]
    public async Task CheckAccountLockedAsync_Should_ReturnWithoutRedisCall_When_UserNameIsBlank()
    {
        // Arrange
        var db = new Mock<IDatabase>();
        var sut = CreateSut(db);

        // Act
        await sut.CheckAccountLockedAsync(" ", CancellationToken.None);

        // Assert
        db.Verify(x => x.KeyExistsAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()), Times.Never);
    }

    [Fact]
    public async Task CheckAccountLockedAsync_Should_Return_When_LockKeyDoesNotExist()
    {
        // Arrange
        var db = new Mock<IDatabase>();
        db.Setup(x => x.KeyExistsAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(false);
        var sut = CreateSut(db);

        // Act
        await sut.CheckAccountLockedAsync("tenant.test", CancellationToken.None);

        // Assert
        db.Verify(
            x => x.KeyExistsAsync(
                It.Is<RedisKey>(key => key.ToString() == string.Format(LOCK_KEY_PREFIX, "tenant.test")),
                It.IsAny<CommandFlags>()),
            Times.Once);
    }

    [Fact]
    public async Task CheckAccountLockedAsync_Should_ThrowTooManyRequests_When_LockKeyExists()
    {
        // Arrange
        var db = new Mock<IDatabase>();
        db.Setup(x => x.KeyExistsAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);
        db.Setup(x => x.KeyTimeToLiveAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(TimeSpan.FromMinutes(2));
        var sut = CreateSut(db);

        // Act
        var action = () => sut.CheckAccountLockedAsync("tenant.test", CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<HttpStatusCodeException>()
            .Where(x => x.StatusCode == AppConstants.SystemCode.MANY_REQUESTS);
    }

    [Fact]
    public async Task CheckAccountLockedAsync_Should_ThrowTooManyRequests_When_LockKeyExistsWithoutTtl()
    {
        // Arrange
        var db = new Mock<IDatabase>();
        db.Setup(x => x.KeyExistsAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);
        db.Setup(x => x.KeyTimeToLiveAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((TimeSpan?)null);
        var sut = CreateSut(db);

        // Act
        var action = () => sut.CheckAccountLockedAsync("tenant.test", CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<HttpStatusCodeException>()
            .Where(x => x.StatusCode == AppConstants.SystemCode.MANY_REQUESTS);
    }

    [Fact]
    public async Task CountFailedAttemptAsync_Should_SetWindowTtl_When_FirstFailureIsRecorded()
    {
        // Arrange
        var options = CreateOptions(maxFailedAttempts: 3);
        var db = new Mock<IDatabase>();
        db.Setup(x => x.StringIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<long>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(1);
        db.Setup(x => x.KeyExpireAsync(It.IsAny<RedisKey>(), It.IsAny<TimeSpan?>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);
        db.Setup(x => x.KeyExpireAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<ExpireWhen>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);
        var sut = CreateSut(db, options);

        // Act
        await sut.CountFailedAttemptAsync("tenant.test", CancellationToken.None);

        // Assert
        db.Verify(
            x => x.KeyExpireAsync(
                It.Is<RedisKey>(key => key.ToString() == string.Format(FAIL_KEY_PREFIX, "tenant.test")),
                options.FailedWindow,
                It.IsAny<ExpireWhen>(),
                It.IsAny<CommandFlags>()),
            Times.Once);
        db.Verify(
            x => x.StringSetAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()),
            Times.Never);
    }

    [Fact]
    public async Task CountFailedAttemptAsync_Should_ReturnWithoutRedisCall_When_UserNameIsBlank()
    {
        // Arrange
        var db = new Mock<IDatabase>();
        var sut = CreateSut(db);

        // Act
        await sut.CountFailedAttemptAsync(" ", CancellationToken.None);

        // Assert
        db.Verify(x => x.StringIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<long>(), It.IsAny<CommandFlags>()), Times.Never);
    }

    [Fact]
    public async Task CountFailedAttemptAsync_Should_CreateLockKey_When_ThresholdIsReached()
    {
        // Arrange
        var options = CreateOptions(maxFailedAttempts: 3);
        var db = new Mock<IDatabase>();
        db.Setup(x => x.StringIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<long>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(3);
        db.Setup(x => x.StringSetAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);
        var sut = CreateSut(db, options);

        // Act
        await sut.CountFailedAttemptAsync("tenant.test", CancellationToken.None);

        // Assert
        var matchingLockInvocations = db.Invocations
            .Where(invocation =>
                invocation.Method.Name == nameof(IDatabase.StringSetAsync)
                && invocation.Arguments[0] is RedisKey key
                && key.ToString() == string.Format(LOCK_KEY_PREFIX, "tenant.test")
                && invocation.Arguments[1] is RedisValue value
                && value.ToString() == LOCKED)
            .ToList();
        var lockInvocation = matchingLockInvocations.Should().ContainSingle().Subject;

        lockInvocation.Arguments[2].ToString().Should().Be($"EX {(long)options.LockDuration.TotalSeconds}");
    }

    [Fact]
    public async Task RemoveAttemptsAsync_Should_DeleteFailAndLockKeys_When_UserNameIsProvided()
    {
        // Arrange
        var db = new Mock<IDatabase>();
        db.Setup(x => x.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);
        var sut = CreateSut(db);

        // Act
        await sut.RemoveAttemptsAsync("tenant.test", CancellationToken.None);

        // Assert
        db.Verify(
            x => x.KeyDeleteAsync(
                It.Is<RedisKey>(key => key.ToString() == string.Format(FAIL_KEY_PREFIX, "tenant.test")),
                It.IsAny<CommandFlags>()),
            Times.Once);
        db.Verify(
            x => x.KeyDeleteAsync(
                It.Is<RedisKey>(key => key.ToString() == string.Format(LOCK_KEY_PREFIX, "tenant.test")),
                It.IsAny<CommandFlags>()),
            Times.Once);
    }

    [Fact]
    public async Task RemoveAttemptsAsync_Should_ReturnWithoutRedisCall_When_UserNameIsBlank()
    {
        // Arrange
        var db = new Mock<IDatabase>();
        var sut = CreateSut(db);

        // Act
        await sut.RemoveAttemptsAsync(" ", CancellationToken.None);

        // Assert
        db.Verify(x => x.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()), Times.Never);
    }

    private static LoginAttemptService CreateSut(
        Mock<IDatabase> db,
        LoginAttemptOptions options = null)
    {
        var redis = new Mock<IConnectionMultiplexer>();
        redis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(db.Object);

        return new LoginAttemptService(
            redis.Object,
            Mock.Of<ILogger<LoginAttemptService>>(),
            Options.Create(options ?? CreateOptions()));
    }

    private static LoginAttemptOptions CreateOptions(int maxFailedAttempts = 5) =>
        new()
        {
            MaxFailedAttempts = maxFailedAttempts,
            FailedWindow = TimeSpan.FromMinutes(10),
            LockDuration = TimeSpan.FromMinutes(15)
        };
}
