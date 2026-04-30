namespace Be.Haven.Shared.Enums;

/// <summary>
/// Represents the supported SQL database providers that can be used by the application.
/// </summary>
public enum SqlProvider : byte
{
    /// <summary>
    /// Microsoft SQL Server database provider.
    /// </summary>
    SQL_Server = 0,

    /// <summary>
    /// MySQL database provider.
    /// </summary>
    MySQL = 1,

    /// <summary>
    /// PostgreSQL database provider.
    /// </summary>
    PostgreSQL = 2,

    /// <summary>
    /// SQLite database provider.
    /// </summary>
    SQLite = 3
}
