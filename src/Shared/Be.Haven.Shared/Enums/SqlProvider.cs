namespace Be.Haven.Shared.Enums;

/// <summary>
/// Represents the supported SQL database providers that can be used by the application.
/// </summary>
public enum SqlProvider : byte
{
    /// <summary>
    /// Microsoft SQL Server database provider.
    /// </summary>
    SqlServer = 0,

    /// <summary>
    /// MySQL database provider.
    /// </summary>
    MySql = 1,

    /// <summary>
    /// PostgreSQL database provider.
    /// </summary>
    PostgreSql = 2,

    /// <summary>
    /// SQLite database provider.
    /// </summary>
    SqLite = 3
}
