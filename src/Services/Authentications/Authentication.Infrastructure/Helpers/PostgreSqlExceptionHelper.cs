namespace Authentication.Infrastructure.Helpers;

/// <summary>
/// Extracts PostgreSQL constraint metadata from EF Core persistence exceptions.
/// </summary>
internal static class PostgreSqlExceptionHelper
{
    /// <summary>
    /// Gets the violated unique constraint without exposing provider exceptions to business flows.
    /// </summary>
    /// <param name="exception">The EF Core persistence exception.</param>
    /// <returns>The PostgreSQL constraint name, or <c>null</c> for a different database failure.</returns>
    public static string GetUniqueConstraintName(DbUpdateException exception)
    {
        if (exception.InnerException is not PostgresException postgresException
            || postgresException.SqlState != PostgresErrorCodes.UniqueViolation)
        {
            return null;
        }

        return postgresException.ConstraintName;
    }
}
