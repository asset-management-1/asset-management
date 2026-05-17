namespace Be.Haven.Tests.Shared.Be.Haven.Core.Interceptors;

public sealed class DbConnectionRefreshInterceptorTests
{
    [Fact]
    public async Task ConnectionFailedAsync_Should_RefreshAndApplyConnectionString_When_FirstFailureForConnection()
    {
        // Arrange
        var provider = new Mock<IConnectionStringProvider>();
        provider.Setup(x => x.GetConnectionString("Main")).Returns("Data Source=:memory:");
        await using var connection = new SqliteConnection();
        var sut = CreateSut(provider.Object);
        var eventData = CreateErrorEventData(connection, Guid.NewGuid());

        // Act
        await sut.ConnectionFailedAsync(connection, eventData);

        // Assert
        provider.Verify(x => x.RefreshAsync("Main", It.IsAny<CancellationToken>()), Times.Once);
        connection.ConnectionString.Should().Be("Data Source=:memory:");
    }

    [Fact]
    public async Task ConnectionFailedAsync_Should_RefreshOnlyOnce_When_SameConnectionFailsAgain()
    {
        // Arrange
        var provider = new Mock<IConnectionStringProvider>();
        provider.Setup(x => x.GetConnectionString("Main")).Returns("Data Source=:memory:");
        await using var connection = new SqliteConnection();
        var sut = CreateSut(provider.Object);
        var eventData = CreateErrorEventData(connection, Guid.NewGuid());

        // Act
        await sut.ConnectionFailedAsync(connection, eventData);
        await sut.ConnectionFailedAsync(connection, eventData);

        // Assert
        provider.Verify(x => x.RefreshAsync("Main", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ConnectionFailedAsync_Should_NotThrow_When_RefreshFails()
    {
        // Arrange
        var provider = new Mock<IConnectionStringProvider>();
        provider
            .Setup(x => x.RefreshAsync("Main", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("refresh failed"));
        await using var connection = new SqliteConnection();
        var sut = CreateSut(provider.Object);
        var eventData = CreateErrorEventData(connection, Guid.NewGuid());

        // Act
        var act = async () => await sut.ConnectionFailedAsync(connection, eventData);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void ConnectionOpenedAndClosed_Should_RemoveTrackedRefreshState_When_ConnectionLifecycleChanges()
    {
        // Arrange
        var provider = new Mock<IConnectionStringProvider>();
        using var connection = new SqliteConnection();
        var sut = CreateSut(provider.Object);
        var openedData = CreateEndEventData(connection, Guid.NewGuid());
        var closedData = CreateEndEventData(connection, Guid.NewGuid());

        // Act
        sut.ConnectionOpened(connection, openedData);
        sut.ConnectionClosed(connection, closedData);

        // Assert
        provider.VerifyNoOtherCalls();
    }

    private static DbConnectionRefreshInterceptor CreateSut(IConnectionStringProvider provider)
    {
        return new DbConnectionRefreshInterceptor(
            provider,
            Mock.Of<ILogger<DbConnectionRefreshInterceptor>>(),
            "Main");
    }

    private static ConnectionErrorEventData CreateErrorEventData(
        DbConnection connection,
        Guid connectionId)
    {
        return new ConnectionErrorEventData(
            null,
            (_, _) => string.Empty,
            connection,
            null,
            connectionId,
            new InvalidOperationException("open failed"),
            true,
            DateTimeOffset.UtcNow,
            TimeSpan.FromMilliseconds(1));
    }

    private static ConnectionEndEventData CreateEndEventData(
        DbConnection connection,
        Guid connectionId)
    {
        return new ConnectionEndEventData(
            null,
            (_, _) => string.Empty,
            connection,
            null,
            connectionId,
            true,
            DateTimeOffset.UtcNow,
            TimeSpan.FromMilliseconds(1));
    }
}
