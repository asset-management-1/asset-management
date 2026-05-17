namespace Be.Haven.Tests.Shared.Be.Haven.Core.Services;

public sealed class DistributedLockServiceTests
{
    [Fact]
    public void Constructor_Should_CreateService_When_ConnectionMultiplexerIsProvided()
    {
        // Arrange
        var multiplexer = new Mock<IConnectionMultiplexer>();

        // Act
        var sut = new DistributedLockService(multiplexer.Object);

        // Assert
        sut.Should().NotBeNull();
    }

    [Fact]
    public async Task TryAcquireAsync_Should_ThrowOperationCanceledException_When_TokenIsAlreadyCanceled()
    {
        // Arrange
        var sut = new DistributedLockService(Mock.Of<RedLockNet.IDistributedLockFactory>());
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act
        var action = () => sut.TryAcquireAsync(
            "lock-key",
            TimeSpan.FromSeconds(10),
            cancellationTokenSource.Token);

        // Assert
        await action.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task TryAcquireAsync_Should_ReturnNullAndDisposeLock_When_LockIsNotAcquired()
    {
        // Arrange
        var redLock = new Mock<RedLockNet.IRedLock>();
        redLock.SetupGet(x => x.IsAcquired).Returns(false);

        var factory = new Mock<RedLockNet.IDistributedLockFactory>();
        factory.Setup(x => x.CreateLockAsync("lock-key", TimeSpan.FromSeconds(10)))
               .ReturnsAsync(redLock.Object);

        var sut = new DistributedLockService(factory.Object);

        // Act
        var result = await sut.TryAcquireAsync("lock-key", TimeSpan.FromSeconds(10), CancellationToken.None);

        // Assert
        result.Should().BeNull();
        redLock.Verify(x => x.Dispose(), Times.Once);
    }

    [Fact]
    public async Task TryAcquireAsync_Should_ReturnAsyncDisposableWrapper_When_LockIsAcquired()
    {
        // Arrange
        var redLock = new Mock<RedLockNet.IRedLock>();
        redLock.SetupGet(x => x.IsAcquired).Returns(true);

        var factory = new Mock<RedLockNet.IDistributedLockFactory>();
        factory.Setup(x => x.CreateLockAsync("lock-key", TimeSpan.FromSeconds(10)))
               .ReturnsAsync(redLock.Object);

        var sut = new DistributedLockService(factory.Object);

        // Act
        var result = await sut.TryAcquireAsync("lock-key", TimeSpan.FromSeconds(10), CancellationToken.None);
        await result.DisposeAsync();

        // Assert
        result.Should().NotBeNull();
        redLock.Verify(x => x.Dispose(), Times.Once);
    }
}
