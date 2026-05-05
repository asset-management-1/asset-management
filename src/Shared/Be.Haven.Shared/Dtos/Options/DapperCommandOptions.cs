using Be.Haven.Shared.Enums;

namespace Be.Haven.Shared.Dtos.Options;

/// <summary>
/// Represents configuration options for executing Dapper commands. This class encapsulates
/// the settings required to execute database commands using Dapper, supporting different
/// database providers, command types, and execution flags.
/// </summary>
public class DapperCommandOptions
{
    /// <summary>
    /// Gets or initializes the SQL database provider to be used for executing commands.
    /// Specifies the type of database provider (e.g., SQL Server, MySQL, PostgreSQL)
    /// to ensure compatibility with the configured database connection.
    /// </summary>
    public SqlProvider? Provider { get; init; }

    /// <summary>
    /// Gets or initializes the database connection string to be used for establishing a connection.
    /// Connection string used to open the database connection.
    /// Keep secrets out of source control and configuration defaults.
    /// </summary>
    public string ConnectionString { get; init; }

    /// <summary>
    /// Gets or initializes the type of the database command to execute.
    /// This property determines how the command text is interpreted, such as Text, StoredProcedure, or TableDirect.
    /// The default value is CommandType.StoredProcedure.
    /// Example: CommandType.Text for "SELECT ...", CommandType.StoredProcedure for "sp_GetData".
    /// </summary>
    public CommandType CommandType { get; init; } = CommandType.StoredProcedure;

    /// <summary>
    /// Gets or initializes the timeout (in seconds) to wait for the command to execute.
    /// This property determines how long the database operation will attempt to execute before timing out.
    /// The default value is 180 seconds.
    /// </summary>
    public int CommandTimeout { get; init; } = 180;

    /// <summary>
    /// Gets or initializes the cancellation token used to propagate the notification
    /// that the operation associated with this command should be canceled.
    /// Token used to cancel the operation cooperatively (e.g., when a request is aborted).
    /// Pass CancellationToken.None if you don't need cancellation.
    /// </summary>
    public CancellationToken CancellationToken { get; init; } = CancellationToken.None;

    /// <summary>
    /// Gets or initializes the command behavior flags for Dapper execution.
    /// Additional execution flags that tweak behavior:
    /// - CommandFlags.Buffered: buffer results before returning.
    /// - CommandFlags.None: stream results (unbuffered).
    /// - CommandFlags.NoCache: bypass plan cache for this command.
    /// - CommandFlags.Pipelined: allow async pipelining (advanced).
    /// </summary>
    public CommandFlags Flags { get; init; } = CommandFlags.Buffered;

    /// <summary>
    /// Gets or initializes the database transaction to be used during the execution of a Dapper command.
    /// Ambient transaction to execute within (optional).
    /// Ensure the transaction belongs to the same open connection used for the command.
    /// </summary>
    public IDbTransaction Transaction { get; init; } = null;

}
