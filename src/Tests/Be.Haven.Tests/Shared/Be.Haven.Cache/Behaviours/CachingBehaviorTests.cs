namespace Be.Haven.Tests.Shared.Be.Haven.Cache.Behaviours;

public sealed class CachingBehaviorTests
{
    [Fact]
    public async Task Handle_Should_InvokeNextWithoutCache_When_RequestBypassesCache()
    {
        // Arrange
        var cache = new FakeDistributedCache();
        var request = new CacheSampleQuery
        {
            BypassCache = true
        };
        var sut = CreateSut(cache);
        var callCount = 0;

        // Act
        var result = await sut.Handle(
            request,
            (_, _) =>
            {
                callCount++;
                return ValueTask.FromResult("fresh");
            },
            CancellationToken.None);

        // Assert
        result.Should().Be("fresh");
        callCount.Should().Be(1);
        cache.Values.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_ReturnCachedResponse_When_CacheContainsPayload()
    {
        // Arrange
        var cache = new FakeDistributedCache();
        var request = new CacheSampleQuery
        {
            CacheKey = "cache:sample",
            Search = "abc",
            Page = 1
        };
        var version = new Mock<ICacheVersionService>();
        version.Setup(x => x.GetEpoch()).Returns("20260101");
        version.Setup(x => x.GetAsync("cache:sample", string.Empty, "20260101")).ReturnsAsync(DEFAULT_VERSION);
        var key = CacheKeyHelper.BuildKey(request, string.Empty, "20260101", DEFAULT_VERSION);
        cache.Values[key] = Encoding.UTF8.GetBytes("\"cached\"");
        var serializer = new Mock<IJsonSerializerService>();
        serializer.Setup(x => x.Deserialize<string>("\"cached\"")).Returns("cached");
        var sut = CreateSut(cache, serializer, version);

        // Act
        var result = await sut.Handle(
            request,
            (_, _) => ValueTask.FromResult("fresh"),
            CancellationToken.None);

        // Assert
        result.Should().Be("cached");
    }

    [Fact]
    public async Task Handle_Should_WriteCacheWithConfiguredTtl_When_CacheMisses()
    {
        // Arrange
        var cache = new FakeDistributedCache();
        var request = new CacheSampleQuery
        {
            CacheKey = "cache:sample",
            Search = "abc",
            Page = 1
        };
        var serializer = new Mock<IJsonSerializerService>();
        serializer.Setup(x => x.Serialize("fresh")).Returns("\"fresh\"");
        var version = new Mock<ICacheVersionService>();
        version.Setup(x => x.GetEpoch()).Returns("20260101");
        version.Setup(x => x.GetAsync("cache:sample", string.Empty, "20260101")).ReturnsAsync(DEFAULT_VERSION);
        var sut = CreateSut(cache, serializer, version);

        // Act
        var result = await sut.Handle(
            request,
            (_, _) => ValueTask.FromResult("fresh"),
            CancellationToken.None);

        // Assert
        result.Should().Be("fresh");
        cache.Values.Should().ContainSingle();
        cache.OptionsByKey.Values.Single().AbsoluteExpirationRelativeToNow.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public async Task Handle_Should_WriteCacheWithRequestTtl_When_RequestProvidesPositiveExpiration()
    {
        // Arrange
        var cache = new FakeDistributedCache();
        var request = new CacheSampleQuery
        {
            CacheKey = "cache:sample",
            AbsoluteExpiration = TimeSpan.FromSeconds(5)
        };
        var serializer = new Mock<IJsonSerializerService>();
        serializer.Setup(x => x.Serialize("fresh")).Returns("\"fresh\"");
        var sut = CreateSut(cache, serializer);

        // Act
        await sut.Handle(
            request,
            (_, _) => ValueTask.FromResult("fresh"),
            CancellationToken.None);

        // Assert
        cache.OptionsByKey.Values.Single().AbsoluteExpirationRelativeToNow.Should().Be(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_Should_InvokeNextWithoutCache_When_VersionReadFails()
    {
        // Arrange
        var cache = new FakeDistributedCache();
        var request = new CacheSampleQuery
        {
            CacheKey = "cache:sample"
        };
        var version = new Mock<ICacheVersionService>();
        version.Setup(x => x.GetEpoch()).Returns("20260101");
        version.Setup(x => x.GetAsync("cache:sample", string.Empty, "20260101"))
            .ThrowsAsync(new InvalidOperationException("version unavailable"));
        var sut = CreateSut(cache, version: version);

        // Act
        var result = await sut.Handle(
            request,
            (_, _) => ValueTask.FromResult("fresh"),
            CancellationToken.None);

        // Assert
        result.Should().Be("fresh");
        cache.Values.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_InvokeNextWithoutCache_When_CacheReadFails()
    {
        // Arrange
        var cache = new ThrowingDistributedCache(throwOnGet: true);
        var request = new CacheSampleQuery
        {
            CacheKey = "cache:sample"
        };
        var sut = CreateSut(cache);

        // Act
        var result = await sut.Handle(
            request,
            (_, _) => ValueTask.FromResult("fresh"),
            CancellationToken.None);

        // Assert
        result.Should().Be("fresh");
        cache.Values.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_ReturnFreshResponse_When_CacheWriteFails()
    {
        // Arrange
        var cache = new ThrowingDistributedCache(throwOnSet: true);
        var request = new CacheSampleQuery
        {
            CacheKey = "cache:sample"
        };
        var serializer = new Mock<IJsonSerializerService>();
        serializer.Setup(x => x.Serialize("fresh")).Returns("\"fresh\"");
        var sut = CreateSut(cache, serializer);

        // Act
        var result = await sut.Handle(
            request,
            (_, _) => ValueTask.FromResult("fresh"),
            CancellationToken.None);

        // Assert
        result.Should().Be("fresh");
        cache.Values.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_InvokeNextWithoutCache_When_BypassMarkerExists()
    {
        // Arrange
        var cache = new FakeDistributedCache();
        var request = new CacheSampleQuery
        {
            CacheKey = "cache:sample"
        };
        var bypass = new Mock<ICacheBypassService>();
        bypass.Setup(x => x.ShouldBypassAsync("cache:sample", string.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var sut = CreateSut(cache, bypassService: bypass);

        // Act
        var result = await sut.Handle(
            request,
            (_, _) => ValueTask.FromResult("fresh"),
            CancellationToken.None);

        // Assert
        result.Should().Be("fresh");
        cache.Values.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_ResolveCurrentUserScope_When_RequestUsesCurrentUserScope()
    {
        // Arrange
        var userPublicId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var cache = new FakeDistributedCache();
        var request = new CacheSampleQuery
        {
            CacheKey = "cache:sample",
            CacheScope = CURRENT_USER_CACHE_SCOPE
        };
        var auth = new Mock<IAuthService>();
        auth.Setup(x => x.UserId()).Returns(userPublicId);
        var version = new Mock<ICacheVersionService>();
        version.Setup(x => x.GetEpoch()).Returns("20260101");
        version.Setup(x => x.GetAsync("cache:sample", userPublicId.ToUserCacheScope(), "20260101"))
            .ReturnsAsync(DEFAULT_VERSION);
        var serializer = new Mock<IJsonSerializerService>();
        serializer.Setup(x => x.Serialize("fresh")).Returns("\"fresh\"");
        var sut = CreateSut(cache, serializer, version, auth: auth);

        // Act
        await sut.Handle(
            request,
            (_, _) => ValueTask.FromResult("fresh"),
            CancellationToken.None);

        // Assert
        cache.Values.Keys.Single().Should().Contain(":user:22222222-2222-2222-2222-222222222222:");
    }

    [Fact]
    public async Task Handle_Should_UseEmptyScope_When_CurrentUserScopeHasNoAuthenticatedUser()
    {
        // Arrange
        var cache = new FakeDistributedCache();
        var request = new CacheSampleQuery
        {
            CacheKey = "cache:sample",
            CacheScope = CURRENT_USER_CACHE_SCOPE
        };
        var auth = new Mock<IAuthService>();
        auth.Setup(x => x.UserId()).Returns((Guid?)null);
        var version = new Mock<ICacheVersionService>();
        version.Setup(x => x.GetEpoch()).Returns("20260101");
        version.Setup(x => x.GetAsync("cache:sample", string.Empty, "20260101"))
            .ReturnsAsync(DEFAULT_VERSION);
        var serializer = new Mock<IJsonSerializerService>();
        serializer.Setup(x => x.Serialize("fresh")).Returns("\"fresh\"");
        var sut = CreateSut(cache, serializer, version, auth: auth);

        // Act
        await sut.Handle(
            request,
            (_, _) => ValueTask.FromResult("fresh"),
            CancellationToken.None);

        // Assert
        cache.Values.Keys.Single().Should().NotContain(":user:");
    }

    private static CachingBehavior<CacheSampleQuery, string> CreateSut(
        IDistributedCache cache,
        Mock<IJsonSerializerService> serializer = null,
        Mock<ICacheVersionService> version = null,
        Mock<IAuthService> auth = null,
        Mock<ICacheBypassService> bypassService = null) =>
        new(
            cache,
            Mock.Of<ILogger<CachingBehavior<CacheSampleQuery, string>>>(),
            (serializer ?? CreateSerializer()).Object,
            Options.Create(new CacheOptions
            {
                AbsoluteExpiration = 30
            }),
            (version ?? CreateVersion()).Object,
            (auth ?? new Mock<IAuthService>()).Object,
            (bypassService ?? CreateBypass()).Object);

    private static Mock<IJsonSerializerService> CreateSerializer()
    {
        var serializer = new Mock<IJsonSerializerService>();
        serializer.Setup(x => x.Serialize(It.IsAny<string>())).Returns<string>(value => JsonConvert.SerializeObject(value));
        serializer.Setup(x => x.Deserialize<string>(It.IsAny<string>())).Returns<string>(JsonConvert.DeserializeObject<string>);

        return serializer;
    }

    private static Mock<ICacheVersionService> CreateVersion()
    {
        var version = new Mock<ICacheVersionService>();
        version.Setup(x => x.GetEpoch()).Returns("20260101");
        version.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(DEFAULT_VERSION);

        return version;
    }

    private static Mock<ICacheBypassService> CreateBypass()
    {
        var bypass = new Mock<ICacheBypassService>();
        bypass.Setup(x => x.ShouldBypassAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        return bypass;
    }

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
