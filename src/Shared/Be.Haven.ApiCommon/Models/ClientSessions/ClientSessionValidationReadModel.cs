namespace Be.Haven.ApiCommon.Models.ClientSessions;

/// <summary>
/// Minimal read model returned by client-session validation.
/// </summary>
public sealed class ClientSessionValidationReadModel
{
    /// <summary>
    /// Gets or sets the matched server-issued session public identifier.
    /// </summary>
    public Guid SessionPublicId { get; set; }
}
