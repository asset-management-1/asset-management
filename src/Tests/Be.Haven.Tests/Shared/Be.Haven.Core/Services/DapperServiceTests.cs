namespace Be.Haven.Tests.Shared.Be.Haven.Core.Services;

public sealed class DapperServiceTests
{
    [Fact]
    public async Task QueryAsync_Should_ReturnMappedRows_When_TextQueryMatches()
    {
        // Arrange
        await using var database = await CreateDatabaseAsync();
        var factory = new FakeDbConnectionFactory(database.ConnectionString);
        var sut = CreateSut(factory);

        // Act
        var result = await sut.QueryAsync<CoreDapperSampleModel>(
            "SELECT Name, Quantity FROM Samples WHERE Quantity >= @Minimum ORDER BY Quantity",
            new
            {
                Minimum = 2
            },
            TextOptions(database.ConnectionString));

        // Assert
        result.Select(x => x.Name).Should().Equal("Beta", "Gamma");
        factory.Requests.Should().ContainSingle().Which.Provider.Should().Be(SqlProvider.SqLite);
    }

    [Fact]
    public async Task QueryFirstOrDefaultAsync_Should_ReturnDefault_When_NoRowMatches()
    {
        // Arrange
        await using var database = await CreateDatabaseAsync();
        var sut = CreateSut(new FakeDbConnectionFactory(database.ConnectionString));

        // Act
        var result = await sut.QueryFirstOrDefaultAsync<CoreDapperSampleModel>(
            "SELECT Name, Quantity FROM Samples WHERE Name = @Name",
            new
            {
                Name = "Missing"
            },
            TextOptions(database.ConnectionString));

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task QueryFirstAsync_Should_ReturnFirstMappedRow_When_RowsMatch()
    {
        // Arrange
        await using var database = await CreateDatabaseAsync();
        var sut = CreateSut(new FakeDbConnectionFactory(database.ConnectionString));

        // Act
        var result = await sut.QueryFirstAsync<CoreDapperSampleModel>(
            "SELECT Name, Quantity FROM Samples ORDER BY Quantity",
            options: TextOptions(database.ConnectionString));

        // Assert
        result.Name.Should().Be("Alpha");
        result.Quantity.Should().Be(1);
    }

    [Fact]
    public async Task QuerySingleOrDefaultAsync_Should_ReturnSingleMappedRow_When_OneRowMatches()
    {
        // Arrange
        await using var database = await CreateDatabaseAsync();
        var sut = CreateSut(new FakeDbConnectionFactory(database.ConnectionString));

        // Act
        var result = await sut.QuerySingleOrDefaultAsync<CoreDapperSampleModel>(
            "SELECT Name, Quantity FROM Samples WHERE Name = @Name",
            new
            {
                Name = "Beta"
            },
            TextOptions(database.ConnectionString));

        // Assert
        result.Name.Should().Be("Beta");
        result.Quantity.Should().Be(2);
    }

    [Fact]
    public async Task QuerySingleAsync_Should_ReturnSingleScalarMappedRow_When_OneRowMatches()
    {
        // Arrange
        await using var database = await CreateDatabaseAsync();
        var sut = CreateSut(new FakeDbConnectionFactory(database.ConnectionString));

        // Act
        var result = await sut.QuerySingleAsync<CoreDapperSampleModel>(
            "SELECT Name, Quantity FROM Samples WHERE Name = @Name",
            new
            {
                Name = "Alpha"
            },
            TextOptions(database.ConnectionString));

        // Assert
        result.Name.Should().Be("Alpha");
        result.Quantity.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAndScalarAsync_Should_ModifyRowsAndReturnScalar_When_CommandIsText()
    {
        // Arrange
        await using var database = await CreateDatabaseAsync();
        var sut = CreateSut(new FakeDbConnectionFactory(database.ConnectionString));
        var options = TextOptions(database.ConnectionString);

        // Act
        var affected = await sut.ExecuteAsync(
            "UPDATE Samples SET Quantity = Quantity + 10 WHERE Name = @Name",
            new
            {
                Name = "Alpha"
            },
            options);
        var quantity = await sut.ExecuteScalarAsync<int>(
            "SELECT Quantity FROM Samples WHERE Name = @Name",
            new
            {
                Name = "Alpha"
            },
            options);

        // Assert
        affected.Should().Be(1);
        quantity.Should().Be(11);
    }

    [Fact]
    public async Task QueryMultipleAsync_Should_MapMultipleResultSets_When_QueryReturnsSeveralSets()
    {
        // Arrange
        await using var database = await CreateDatabaseAsync();
        var sut = CreateSut(new FakeDbConnectionFactory(database.ConnectionString));

        // Act
        var result = await sut.QueryMultipleAsync(
            "SELECT Name FROM Samples ORDER BY Quantity; SELECT COUNT(1) FROM Samples;",
            async grid =>
            {
                var names = (await grid.ReadAsync<string>()).ToList();
                var total = await grid.ReadSingleAsync<int>();

                return (Names: names, Total: total);
            },
            options: TextOptions(database.ConnectionString));

        // Assert
        result.Names.Should().Equal("Alpha", "Beta", "Gamma");
        result.Total.Should().Be(3);
    }

    [Fact]
    public async Task QueryAsync_Should_UseConfiguredProvider_When_CommandOptionsProviderIsMissing()
    {
        // Arrange
        await using var database = await CreateDatabaseAsync();
        var factory = new FakeDbConnectionFactory(database.ConnectionString);
        var sut = CreateSut(factory, SqlProvider.SqLite);

        // Act
        var result = await sut.QueryAsync<CoreDapperSampleModel>(
            "SELECT Name, Quantity FROM Samples ORDER BY Quantity",
            options: new DapperCommandOptions
            {
                ConnectionString = database.ConnectionString,
                CommandType = CommandType.Text
            });

        // Assert
        result.Should().HaveCount(3);
        factory.Requests.Should().ContainSingle().Which.Provider.Should().Be(SqlProvider.SqLite);
    }

    private static DapperService CreateSut(
        IDbConnectionFactory factory,
        SqlProvider provider = SqlProvider.PostgreSql)
    {
        return new DapperService(
            factory,
            Options.Create(new GcpOptions
            {
                DatabaseSettings = new DatabaseOptions
                {
                    Provider = provider
                }
            }));
    }

    private static DapperCommandOptions TextOptions(string connectionString) =>
        new()
        {
            Provider = SqlProvider.SqLite,
            ConnectionString = connectionString,
            CommandType = CommandType.Text
        };

    private static async Task<SqliteConnection> CreateDatabaseAsync()
    {
        var connectionString = $"Data Source=CoreDapper{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        var connection = new SqliteConnection(connectionString);

        await connection.OpenAsync();
        await ExecuteAsync(
            connection,
            """
            CREATE TABLE Samples (Name TEXT NOT NULL, Quantity INTEGER NOT NULL);
            INSERT INTO Samples (Name, Quantity) VALUES ('Alpha', 1), ('Beta', 2), ('Gamma', 3);
            """);

        return connection;
    }

    private static async Task ExecuteAsync(SqliteConnection connection, string sql)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        await command.ExecuteNonQueryAsync();
    }
}
