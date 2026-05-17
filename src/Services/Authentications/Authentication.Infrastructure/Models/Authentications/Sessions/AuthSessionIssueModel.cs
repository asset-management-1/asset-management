namespace Authentication.Infrastructure.Models.Authentications.Sessions;

/// <summary>
/// Represents the prepared JWT response and refresh-token entity for one issued session.
/// </summary>
internal class AuthSessionIssueModel
{
    /// <summary>
    /// Gets or sets the refresh-token entity to persist.
    /// </summary>
    public RefreshToken RefreshToken { get; set; }

    /// <summary>
    /// Gets or sets the login response returned to API clients.
    /// </summary>
    public LoginResponseDto LoginResponse { get; set; }
}
