namespace Be.Haven.Tests.Shared.Be.Haven.Cache.Services;

public sealed class DistributedCacheVersionServiceTests
{
    [Fact]
    public void GetEpoch_Should_ReturnUtcDateEpoch_When_Called()
    {
        // Arrange
        var sut = CreateSut(new Mock<IDatabase>());

        // Act
        var result = sut.GetEpoch();

        // Assert
        result.Should().HaveLength(8);
        result.Should().MatchRegex("^\\d{8}$");
    }

    [Fact]
    public async Task GetAsync_Should_ReturnDefaultVersion_When_KeyIsMissing()
    {
        // Arrange
        var db = new Mock<IDatabase>();
        db.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);
        var sut = CreateSut(db);

        // Act
        var result = await sut.GetAsync("auth:user-info", "user:1", "20260517");

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnParsedVersion_When_KeyHasPositiveLongValue()
    {
        // Arrange
        var db = new Mock<IDatabase>();
        db.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue("7"));
        var sut = CreateSut(db);

        // Act
        var result = await sut.GetAsync("auth:user-info", "user:1", "20260517");

        // Assert
        result.Should().Be(7);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("-1")]
    public async Task GetAsync_Should_ThrowInvalidOperationException_When_VersionStateIsUnsafe(string redisValue)
    {
        // Arrange
        var db = new Mock<IDatabase>();
        db.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue(redisValue));
        var sut = CreateSut(db);

        // Act
        var action = () => sut.GetAsync("auth:user-info", "user:1", "20260517");

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    [Theory]
    [InlineData("", "user:1", "20260517")]
    [InlineData("auth:user-info", "user:1", "")]
    public async Task GetAsync_Should_ThrowArgumentException_When_RequiredArgumentsAreBlank(
        string cacheGroup,
        string cacheScope,
        string epoch)
    {
        // Arrange
        var sut = CreateSut(new Mock<IDatabase>());

        // Act
        var action = () => sut.GetAsync(cacheGroup, cacheScope, epoch);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task InvalidateAsync_Should_SeedAndIncrementVersion_When_KeyIsValid()
    {
        // Arrange
        var db = new Mock<IDatabase>();
        db.Setup(x => x.StringSetAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);
        db.Setup(x => x.StringIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<long>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(2);
        db.Setup(x => x.KeyTimeToLiveAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(TimeSpan.FromMinutes(5));
        var sut = CreateSut(db);

        // Act
        var result = await sut.InvalidateAsync("auth:user-info", "user:1", "20260517");

        // Assert
        result.Should().Be(2);
        db.Verify(
            x => x.StringIncrementAsync(
                It.Is<RedisKey>(key => key.ToString() == "ver:auth:user-info:user:1:20260517"),
                1,
                It.IsAny<CommandFlags>()),
            Times.Once);
    }

    [Fact]
    public async Task InvalidateAsync_Should_ApplyTtl_When_KeyHasNoTtl()
    {
        // Arrange
        var db = new Mock<IDatabase>();
        db.Setup(x => x.StringSetAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);
        db.Setup(x => x.StringIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<long>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(3);
        db.Setup(x => x.KeyTimeToLiveAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .Returns(Task.FromResult<TimeSpan?>(null));
        db.Setup(x => x.KeyExpireAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<ExpireWhen>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);
        var sut = CreateSut(db);

        // Act
        var result = await sut.InvalidateAsync("auth:user-info", string.Empty, "20260517");

        // Assert
        result.Should().Be(3);
        db.Verify(
            x => x.KeyExpireAsync(
                It.IsAny<RedisKey>(),
                It.Is<TimeSpan?>(ttl => ttl.HasValue && ttl.Value > TimeSpan.Zero),
                It.IsAny<ExpireWhen>(),
                It.IsAny<CommandFlags>()),
            Times.Once);
    }

    [Fact]
    public async Task InvalidateAsync_Should_ReturnNewVersion_When_TtlCleanupFails()
    {
        // Arrange
        var db = new Mock<IDatabase>();
        db.Setup(x => x.StringSetAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);
        db.Setup(x => x.StringIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<long>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(4);
        db.Setup(x => x.KeyTimeToLiveAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ThrowsAsync(new RedisException("ttl failed"));
        var sut = CreateSut(db);

        // Act
        var result = await sut.InvalidateAsync("auth:user-info", string.Empty, "20260517");

        // Assert
        result.Should().Be(4);
    }

    [Fact]
    public async Task InvalidateAsync_Should_ThrowInvalidOperationException_When_IncrementDoesNotAdvance()
    {
        // Arrange
        var db = new Mock<IDatabase>();
        db.Setup(x => x.StringSetAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);
        db.Setup(x => x.StringIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<long>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(0);
        var sut = CreateSut(db);

        // Act
        var action = () => sut.InvalidateAsync("auth:user-info", string.Empty, "20260517");

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    [Theory]
    [InlineData("", "scope", "20260517")]
    [InlineData("auth:user-info", "scope", "")]
    public async Task InvalidateAsync_Should_ThrowArgumentException_When_RequiredArgumentsAreBlank(
        string cacheGroup,
        string cacheScope,
        string epoch)
    {
        // Arrange
        var sut = CreateSut(new Mock<IDatabase>());

        // Act
        var action = () => sut.InvalidateAsync(cacheGroup, cacheScope, epoch);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }

    private static DistributedCacheVersionService CreateSut(Mock<IDatabase> db)
    {
        var redis = new Mock<IConnectionMultiplexer>();
        redis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(db.Object);

        return new DistributedCacheVersionService(
            redis.Object,
            Mock.Of<ILogger<DistributedCacheVersionService>>());
    }
}
