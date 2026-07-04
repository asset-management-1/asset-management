namespace Be.Haven.Tests.Shared.Be.Haven.Cache.Behaviours;

public sealed class InvalidationBehaviorTests
{
    [Fact]
    public async Task Handle_Should_ReturnResponseWithoutInvalidation_When_NoPolicyTargetsExist()
    {
        // Arrange
        var version = new Mock<ICacheVersionService>();
        var sut = CreateSut(version: version);

        // Act
        var result = await sut.Handle(
            new CacheSampleCommand(),
            (_, _) => ValueTask.FromResult("ok"),
            CancellationToken.None);

        // Assert
        result.Should().Be("ok");
        version.Verify(x => x.InvalidateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_InvalidateDistinctTargets_When_RequestSucceeds()
    {
        // Arrange
        var target = new CacheInvalidationTargetModel("cache:sample", "user:1");
        var version = new Mock<ICacheVersionService>();
        version.Setup(x => x.GetEpoch()).Returns("20260101");
        version.Setup(x => x.InvalidateAsync(target.CacheGroup, target.CacheScope, "20260101"))
            .ReturnsAsync(DEFAULT_VERSION + 1);
        var sut = CreateSut(
            version,
            new CacheSampleInvalidationPolicy(target, target));

        // Act
        var result = await sut.Handle(
            new CacheSampleCommand(),
            (_, _) => ValueTask.FromResult("ok"),
            CancellationToken.None);

        // Assert
        result.Should().Be("ok");
        version.Verify(x => x.InvalidateAsync(target.CacheGroup, target.CacheScope, "20260101"), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_MarkBypass_When_FailOnErrorTargetInvalidationFails()
    {
        // Arrange
        var target = new CacheInvalidationTargetModel("cache:sample", "user:1", failOnError: true);
        var version = new Mock<ICacheVersionService>();
        version.Setup(x => x.GetEpoch()).Returns("20260101");
        version.Setup(x => x.InvalidateAsync(target.CacheGroup, target.CacheScope, "20260101"))
            .ThrowsAsync(new InvalidOperationException("redis down"));
        var bypass = new Mock<ICacheBypassService>();
        var sut = CreateSut(
            version,
            new CacheSampleInvalidationPolicy(target),
            bypass);

        // Act
        var result = await sut.Handle(
            new CacheSampleCommand(),
            (_, _) => ValueTask.FromResult("ok"),
            CancellationToken.None);

        // Assert
        result.Should().Be("ok");
        bypass.Verify(x => x.MarkBypassAsync(target.CacheGroup, target.CacheScope, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_NotMarkBypass_When_NonCriticalInvalidationFails()
    {
        // Arrange
        var target = new CacheInvalidationTargetModel("cache:sample", "user:1");
        var version = new Mock<ICacheVersionService>();
        version.Setup(x => x.GetEpoch()).Returns("20260101");
        version.Setup(x => x.InvalidateAsync(target.CacheGroup, target.CacheScope, "20260101"))
            .ReturnsAsync(DEFAULT_VERSION);
        var bypass = new Mock<ICacheBypassService>();
        var sut = CreateSut(
            version,
            new CacheSampleInvalidationPolicy(target),
            bypass);

        // Act
        await sut.Handle(
            new CacheSampleCommand(),
            (_, _) => ValueTask.FromResult("ok"),
            CancellationToken.None);

        // Assert
        bypass.Verify(x => x.MarkBypassAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static InvalidationBehavior<CacheSampleCommand, string> CreateSut(
        Mock<ICacheVersionService> version = null,
        CacheSampleInvalidationPolicy policy = null,
        Mock<ICacheBypassService> bypass = null) =>
        new(
            (version ?? CreateVersion()).Object,
            Mock.Of<ILogger<InvalidationBehavior<CacheSampleCommand, string>>>(),
            policy is null
                ? []
                : [policy],
            (bypass ?? new Mock<ICacheBypassService>()).Object);

    private static Mock<ICacheVersionService> CreateVersion()
    {
        var version = new Mock<ICacheVersionService>();
        version.Setup(x => x.GetEpoch()).Returns("20260101");

        return version;
    }

}
