namespace Be.Haven.Tests.Shared.Be.Haven.Core.Common.Fakes;

public sealed class FakeDbConnectionFactory : IDbConnectionFactory
{
    private readonly string _fallbackConnectionString;

    public FakeDbConnectionFactory(string fallbackConnectionString)
    {
        _fallbackConnectionString = fallbackConnectionString;
    }

    public List<(SqlProvider Provider, string ConnectionString)> Requests { get; } = [];

    public async Task<DbConnection> GetOpenConnectionAsync(
        SqlProvider provider,
        string connectionString,
        CancellationToken ct = default)
    {
        Requests.Add((provider, connectionString));

        var effectiveConnectionString = string.IsNullOrWhiteSpace(connectionString)
            ? _fallbackConnectionString
            : connectionString;
        var connection = new SqliteConnection(effectiveConnectionString);

        await connection.OpenAsync(ct);

        return connection;
    }
}
