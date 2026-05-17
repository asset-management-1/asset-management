namespace Be.Haven.Tests.Shared.Be.Haven.Cache.Services;

public sealed class DistributedCacheBypassServiceTests
{
    [Fact]
    public async Task MarkBypassAsync_Should_WriteMarkerWithEpochTtl_When_GroupIsValid()
    {
        // Arrange
        var cache = new FakeDistributedCache();
        var sut = CreateSut(cache);

        // Act
        await sut.MarkBypassAsync("group", "user:1", CancellationToken.None);

        // Assert
        cache.Values.Should().ContainKey("bypass:group:user:1");
        cache.OptionsByKey["bypass:group:user:1"].AbsoluteExpirationRelativeToNow.Should().NotBeNull();
    }

    [Fact]
    public async Task MarkBypassAsync_Should_WriteUnscopedMarker_When_ScopeIsBlank()
    {
        // Arrange
        var cache = new FakeDistributedCache();
        var sut = CreateSut(cache);

        // Act
        await sut.MarkBypassAsync("group", string.Empty, CancellationToken.None);

        // Assert
        cache.Values.Should().ContainKey("bypass:group");
    }

    [Fact]
    public async Task MarkBypassAsync_Should_IgnoreBlankGroup_When_Called()
    {
        // Arrange
        var cache = new FakeDistributedCache();
        var sut = CreateSut(cache);

        // Act
        await sut.MarkBypassAsync(" ", "user:1", CancellationToken.None);

        // Assert
        cache.Values.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldBypassAsync_Should_ReturnTrue_When_MarkerExists()
    {
        // Arrange
        var cache = new FakeDistributedCache();
        var sut = CreateSut(cache);
        await sut.MarkBypassAsync("group", "user:1", CancellationToken.None);

        // Act
        var result = await sut.ShouldBypassAsync("group", "user:1", CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldBypassAsync_Should_ReturnFalse_When_MarkerDoesNotExist()
    {
        // Arrange
        var sut = CreateSut(new FakeDistributedCache());

        // Act
        var result = await sut.ShouldBypassAsync("group", "user:1", CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldBypassAsync_Should_ReturnFalse_When_GroupIsBlank()
    {
        // Arrange
        var sut = CreateSut(new FakeDistributedCache());

        // Act
        var result = await sut.ShouldBypassAsync(" ", "user:1", CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task MarkBypassAsync_Should_NotThrow_When_DistributedCacheWriteFails()
    {
        // Arrange
        var cache = new ThrowingDistributedCache(throwOnSet: true);
        var sut = CreateSut(cache);

        // Act
        var action = () => sut.MarkBypassAsync("group", "user:1", CancellationToken.None);

        // Assert
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ShouldBypassAsync_Should_ReturnFalse_When_DistributedCacheReadFails()
    {
        // Arrange
        var cache = new ThrowingDistributedCache(throwOnGet: true);
        var sut = CreateSut(cache);

        // Act
        var result = await sut.ShouldBypassAsync("group", "user:1", CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    private static DistributedCacheBypassService CreateSut(IDistributedCache cache) =>
        new(
            cache,
            Mock.Of<ILogger<DistributedCacheBypassService>>());

    private sealed class ThrowingDistributedCache : FakeDistributedCache
    {
        private readonly bool _throwOnGet;
        private readonly bool _throwOnSet;

        public ThrowingDistributedCache(
            bool throwOnGet = false,
            bool throwOnSet = false)
        {
            _throwOnGet = throwOnGet;
            _throwOnSet = throwOnSet;
        }

        public override Task<byte[]> GetAsync(
            string key,
            CancellationToken token = default)
        {
            if (_throwOnGet)
            {
                throw new InvalidOperationException("cache read failed");
            }

            return base.GetAsync(key, token);
        }

        public override Task SetAsync(
            string key,
            byte[] value,
            DistributedCacheEntryOptions options,
            CancellationToken token = default)
        {
            if (_throwOnSet)
            {
                throw new InvalidOperationException("cache write failed");
            }

            return base.SetAsync(key, value, options, token);
        }
    }
}
