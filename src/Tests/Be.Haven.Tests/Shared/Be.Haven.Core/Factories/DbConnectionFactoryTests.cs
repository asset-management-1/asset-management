using System.Reflection;
using Microsoft.Data.SqlClient;
using Npgsql;

namespace Be.Haven.Tests.Shared.Be.Haven.Core.Factories;

public sealed class DbConnectionFactoryTests
{
    [Fact]
    public async Task GetOpenConnectionAsync_Should_OpenSqliteConnection_When_ConnectionStringIsProvided()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        await using var result = await sut.GetOpenConnectionAsync(
            SqlProvider.SqLite,
            "Data Source=:memory:");

        // Assert
        result.Should().BeOfType<SqliteConnection>();
        result.State.Should().Be(ConnectionState.Open);
    }

    [Fact]
    public async Task GetOpenConnectionAsync_Should_RefreshDefaultConnection_When_ConnectionStringIsBlank()
    {
        // Arrange
        var provider = new Mock<IConnectionStringProvider>();
        provider.Setup(x => x.GetConnectionString(DEFAULT_CONNECTION)).Returns("Data Source=:memory:");
        var sut = CreateSut(provider.Object);

        // Act
        await using var result = await sut.GetOpenConnectionAsync(SqlProvider.SqLite, string.Empty);

        // Assert
        result.State.Should().Be(ConnectionState.Open);
        provider.Verify(x => x.RefreshAsync(DEFAULT_CONNECTION, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetOpenConnectionAsync_Should_ThrowInvalidOperationException_When_ResolvedConnectionStringIsBlank()
    {
        // Arrange
        var provider = new Mock<IConnectionStringProvider>();
        provider.Setup(x => x.GetConnectionString(DEFAULT_CONNECTION)).Returns(string.Empty);
        var sut = CreateSut(provider.Object);

        // Act
        var act = async () => await sut.GetOpenConnectionAsync(SqlProvider.SqLite, string.Empty);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetOpenConnectionAsync_Should_ThrowNotSupportedException_When_ProviderIsUnsupported()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var act = async () => await sut.GetOpenConnectionAsync(
            (SqlProvider)255,
            "Data Source=:memory:");

        // Assert
        await act.Should().ThrowAsync<NotSupportedException>();
    }

    [Fact]
    public void Create_Should_ReturnSqlConnection_When_ProviderIsSqlServer()
    {
        // Arrange
        var method = typeof(DbConnectionFactory).GetMethod(
            "Create",
            BindingFlags.Static | BindingFlags.NonPublic);

        // Act
        using var result = (DbConnection)method.Invoke(
            null,
            [
            SqlProvider.SqlServer,
            "Server=localhost;Database=missing;User Id=user;Password=password;"
            ]);

        // Assert
        result.Should().BeOfType<SqlConnection>();
    }

    [Fact]
    public void Create_Should_ReturnNpgsqlConnection_When_ProviderIsPostgreSql()
    {
        // Arrange
        var method = typeof(DbConnectionFactory).GetMethod(
            "Create",
            BindingFlags.Static | BindingFlags.NonPublic);

        // Act
        using var result = (DbConnection)method.Invoke(
            null,
            [
            SqlProvider.PostgreSql,
            "Host=localhost;Database=missing;Username=user;Password=password;"
            ]);

        // Assert
        result.Should().BeOfType<NpgsqlConnection>();
    }

    private static DbConnectionFactory CreateSut(IConnectionStringProvider provider = null)
    {
        provider ??= Mock.Of<IConnectionStringProvider>();

        return new DbConnectionFactory(
            provider,
            Options.Create(new GcpOptions
            {
                DatabaseSettings = new DatabaseOptions
                {
                    ConnectionName = string.Empty
                }
            }),
            Mock.Of<ILogger<DbConnectionFactory>>());
    }
}
