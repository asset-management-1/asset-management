namespace Be.Haven.Tests.Shared.Be.Haven.Cache.Services;

public sealed class DistributedLockServiceTests
{
    [Fact]
    public async Task TryAcquireAsync_Should_ThrowCancellation_When_RequestIsAlreadyCancelled()
    {
        // Arrange
        var sut = CreateService(Mock.Of<IConnectionMultiplexer>());
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        // Act
        var action = () => sut.TryAcquireAsync(
            "auth:test-lock",
            TimeSpan.FromSeconds(30),
            cancellationTokenSource.Token);

        // Assert
        await action.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task TryAcquireAsync_Should_RejectNonPositiveLeaseDuration()
    {
        // Arrange
        var sut = CreateService(Mock.Of<IConnectionMultiplexer>());

        // Act
        var action = () => sut.TryAcquireAsync(
            "auth:test-lock",
            TimeSpan.Zero,
            CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task TryAcquireAsync_Should_WrapRedisInfrastructureFailure()
    {
        // Arrange
        var providerFailure = new InvalidOperationException("redis unavailable");
        var connection = new Mock<IConnectionMultiplexer>();
        connection.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                  .Throws(providerFailure);
        var sut = CreateService(connection.Object);

        // Act
        var action = () => sut.TryAcquireAsync(
            "haven:test-lock",
            TimeSpan.FromSeconds(30),
            CancellationToken.None);

        // Assert
        var exception = await action.Should().ThrowAsync<DistributedLockUnavailableException>();
        exception.Which.InnerException.Should().BeSameAs(providerFailure);
    }

    private static DistributedLockService CreateService(IConnectionMultiplexer connection)
    {
        return new DistributedLockService(
            connection,
            Mock.Of<ILogger<DistributedLockService>>());
    }
}
