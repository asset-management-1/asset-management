namespace Be.Haven.Tests.Shared.Be.Haven.Core.Services;

public sealed class DbConnectionHostServiceTests
{
    [Fact]
    public async Task StartAsync_Should_RefreshAndValidateConnectionString_When_ConnectionNameIsConfigured()
    {
        // Arrange
        var provider = new Mock<IConnectionStringProvider>();
        provider.Setup(x => x.GetConnectionString("Main")).Returns("connection-string");
        var sut = CreateSut(provider.Object, "Main");

        // Act
        await sut.StartAsync(CancellationToken.None);

        // Assert
        provider.Verify(x => x.RefreshAsync("Main", It.IsAny<CancellationToken>()), Times.Once);
        provider.Verify(x => x.GetConnectionString("Main"), Times.Once);
    }

    [Fact]
    public async Task StartAsync_Should_UseDefaultConnectionName_When_ConfiguredNameIsBlank()
    {
        // Arrange
        var provider = new Mock<IConnectionStringProvider>();
        provider.Setup(x => x.GetConnectionString(DEFAULT_CONNECTION)).Returns("connection-string");
        var sut = CreateSut(provider.Object, string.Empty);

        // Act
        await sut.StartAsync(CancellationToken.None);

        // Assert
        provider.Verify(x => x.RefreshAsync(DEFAULT_CONNECTION, It.IsAny<CancellationToken>()), Times.Once);
        provider.Verify(x => x.GetConnectionString(DEFAULT_CONNECTION), Times.Once);
    }

    [Fact]
    public async Task StartAsync_Should_WrapRefreshFailure_When_ProviderThrows()
    {
        // Arrange
        var provider = new Mock<IConnectionStringProvider>();
        provider
            .Setup(x => x.RefreshAsync("Main", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApplicationException("refresh failed"));
        var sut = CreateSut(provider.Object, "Main");

        // Act
        var act = async () => await sut.StartAsync(CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.Which.InnerException.Should().BeOfType<ApplicationException>();
    }

    [Fact]
    public async Task StopAsync_Should_Complete_When_Called()
    {
        // Arrange
        var sut = CreateSut(Mock.Of<IConnectionStringProvider>(), "Main");

        // Act
        var act = async () => await sut.StopAsync(CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    private static DbConnectionHostService CreateSut(
        IConnectionStringProvider provider,
        string connectionName)
    {
        return new DbConnectionHostService(
            provider,
            Options.Create(new GcpOptions
            {
                DatabaseSettings = new DatabaseOptions
                {
                    ConnectionName = connectionName
                }
            }),
            Mock.Of<ILogger<DbConnectionHostService>>());
    }
}
