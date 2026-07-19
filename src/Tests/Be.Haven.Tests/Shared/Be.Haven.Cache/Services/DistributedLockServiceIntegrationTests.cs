namespace Be.Haven.Tests.Shared.Be.Haven.Cache.Services;

public sealed class DistributedLockServiceIntegrationTests
{
    private const string REDIS_INTEGRATION_CONNECTION_ENVIRONMENT_VARIABLE = "HAVEN_REDIS_INTEGRATION_CONNECTION";

    [Fact(Skip = "Requires a real Redis endpoint; set HAVEN_REDIS_INTEGRATION_CONNECTION and remove this skip locally.")]
    [Trait("Category", "RedisIntegration")]
    public async Task TryAcquireAsync_Should_CoordinateIndependentProviders_AndRenewHeldLease()
    {
        // Arrange
        var connectionString = Environment.GetEnvironmentVariable(
            REDIS_INTEGRATION_CONNECTION_ENVIRONMENT_VARIABLE);

        connectionString.Should().NotBeNullOrWhiteSpace();

        await using var firstConnection = await ConnectionMultiplexer.ConnectAsync(connectionString);
        await using var secondConnection = await ConnectionMultiplexer.ConnectAsync(connectionString);
        var firstProvider = CreateService(firstConnection);
        var secondProvider = CreateService(secondConnection);
        var sharedKey = $"tests:distributed-lock:{Guid.NewGuid():N}";
        var independentKey = $"tests:distributed-lock:{Guid.NewGuid():N}";
        var leaseDuration = TimeSpan.FromSeconds(2);

        // Acquire the shared key from one provider while proving unrelated keys remain independent.
        var firstHandle = await firstProvider.TryAcquireAsync(
            sharedKey,
            leaseDuration,
            CancellationToken.None);
        await using var independentHandle = await secondProvider.TryAcquireAsync(
            independentKey,
            leaseDuration,
            CancellationToken.None);

        firstHandle.Should().NotBeNull();
        independentHandle.Should().NotBeNull();

        await using (firstHandle)
        {
            // Wait beyond the initial lease; automatic renewal must preserve ownership until disposal.
            await Task.Delay(TimeSpan.FromSeconds(4));
            var contendedHandle = await secondProvider.TryAcquireAsync(
                sharedKey,
                leaseDuration,
                CancellationToken.None);

            contendedHandle.Should().BeNull();
        }

        // Disposing the first handle releases the key for another service instance.
        await using var reacquiredHandle = await secondProvider.TryAcquireAsync(
            sharedKey,
            leaseDuration,
            CancellationToken.None);

        reacquiredHandle.Should().NotBeNull();

    }

    private static DistributedLockService CreateService(IConnectionMultiplexer connection)
    {
        return new DistributedLockService(
            connection,
            Mock.Of<ILogger<DistributedLockService>>());
    }
}
