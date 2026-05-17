namespace Be.Haven.Tests.Shared.Be.Haven.Core.Providers;

public sealed class ConnectionStringProviderTests
{
    [Fact]
    public void GetConnectionString_Should_ReturnConfiguredValueAndCacheAsLastKnownGood_When_ConfigExists()
    {
        // Arrange
        var configuration = BuildConfiguration(new Dictionary<string, string>
        {
            ["ConnectionStrings:Main"] = "local-cs"
        });
        var sut = CreateSut(configuration);

        // Act
        var result = sut.GetConnectionString("Main");

        // Assert
        result.Should().Be("local-cs");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetConnectionString_Should_ThrowArgumentException_When_ConnectionNameIsBlank(string connectionName)
    {
        // Arrange
        var sut = CreateSut(BuildConfiguration());

        // Act
        var act = () => sut.GetConnectionString(connectionName);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetConnectionString_Should_ThrowInvalidOperationException_When_ValueIsMissing()
    {
        // Arrange
        var sut = CreateSut(BuildConfiguration());

        // Act
        var act = () => sut.GetConnectionString("Missing");

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task RefreshAsync_Should_LoadFromConfiguration_When_GcpIsDisabled()
    {
        // Arrange
        var configuration = BuildConfiguration(new Dictionary<string, string>
        {
            ["ConnectionStrings:Main"] = "local-cs",
            [$"{GCP_SETTINGS}:DatabaseSettings:IsUseGcp"] = "false"
        });
        var sut = CreateSut(configuration);

        // Act
        await sut.RefreshAsync("Main");
        var result = sut.GetConnectionString("Main");

        // Assert
        result.Should().Be("local-cs");
    }

    [Fact]
    public async Task RefreshAsync_Should_LoadFromGcpSecret_When_GcpIsEnabledForConfiguredConnection()
    {
        // Arrange
        var configuration = BuildConfiguration(new Dictionary<string, string>
        {
            ["ConnectionStrings:Main"] = "local-cs",
            [$"{GCP_SETTINGS}:DatabaseSettings:IsUseGcp"] = "true",
            [$"{GCP_SETTINGS}:DatabaseSettings:ConnectionName"] = "Main",
            [$"{GCP_SETTINGS}:DatabaseSettings:SecretId"] = "db-secret"
        });
        var secretService = new Mock<IGcpSecretService>();
        secretService
            .Setup(x => x.GetByIdAsync("db-secret", null, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync("secret-cs");
        var sut = CreateSut(configuration, secretService.Object);

        // Act
        await sut.RefreshAsync("Main");
        var result = sut.GetConnectionString("Main");

        // Assert
        result.Should().Be("secret-cs");
        secretService.Verify(
            x => x.GetByIdAsync("db-secret", null, false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RefreshAsync_Should_ThrowArgumentException_When_ConnectionNameIsBlank(string connectionName)
    {
        // Arrange
        var sut = CreateSut(BuildConfiguration());

        // Act
        var action = async () => await sut.RefreshAsync(connectionName);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task RefreshAsync_Should_ThrowInvalidOperationException_When_GcpSecretValueIsBlank()
    {
        // Arrange
        var configuration = BuildConfiguration(new Dictionary<string, string>
        {
            [$"{GCP_SETTINGS}:DatabaseSettings:IsUseGcp"] = "true",
            [$"{GCP_SETTINGS}:DatabaseSettings:ConnectionName"] = "Main",
            [$"{GCP_SETTINGS}:DatabaseSettings:SecretId"] = "db-secret"
        });
        var secretService = new Mock<IGcpSecretService>();
        secretService
            .Setup(x => x.GetByIdAsync("db-secret", null, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(" ");
        var sut = CreateSut(configuration, secretService.Object);

        // Act
        var action = async () => await sut.RefreshAsync("Main");

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task RefreshAsync_Should_ThrowInvalidOperationException_When_ConfigValueIsMissing()
    {
        // Arrange
        var sut = CreateSut(BuildConfiguration());

        // Act
        var act = async () => await sut.RefreshAsync("Missing");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private static ConnectionStringProvider CreateSut(
        IConfiguration configuration,
        IGcpSecretService secretService = null)
    {
        return new ConnectionStringProvider(
            configuration,
            secretService ?? Mock.Of<IGcpSecretService>(),
            Mock.Of<ILogger<ConnectionStringProvider>>());
    }

    private static IConfiguration BuildConfiguration(Dictionary<string, string> values = null)
    {
        values ??= [];

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
