using Testcontainers.Redis;

namespace Be.Haven.Tests.Shared.Be.Haven.Cache.Services;

public sealed class AtomicCacheServiceContainerTests : IAsyncLifetime
{
    private readonly RedisContainer _redis = new RedisBuilder("redis:7-alpine").Build();
    private IConnectionMultiplexer _connection;

    public async Task InitializeAsync()
    {
        await _redis.StartAsync();
        _connection = await ConnectionMultiplexer.ConnectAsync(_redis.GetConnectionString());
    }

    public async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
        await _redis.DisposeAsync();
    }

    [Theory]
    [InlineData("auth:otp:forgot-password:test:consume")]
    [InlineData("auth:reset-session:test:consume")]
    public async Task TrySetIfAbsentAsync_Should_AllowOnlyOneConcurrentConsumer(string key)
    {
        var sut = new AtomicCacheService(_connection, Mock.Of<ILogger<AtomicCacheService>>());

        var results = await Task.WhenAll(
            sut.TrySetIfAbsentAsync(key, "1", TimeSpan.FromMinutes(5)),
            sut.TrySetIfAbsentAsync(key, "1", TimeSpan.FromMinutes(5)));

        results.Count(result => result).Should().Be(1);
    }
}
