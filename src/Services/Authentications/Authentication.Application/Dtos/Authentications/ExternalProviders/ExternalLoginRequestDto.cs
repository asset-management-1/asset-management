namespace Authentication.Application.Dtos.Authentications.ExternalProviders;

/// <summary>
/// Represents the external-login payload used by the authentication service.
/// </summary>
public class ExternalLoginRequestDto
{
    /// <summary>
    /// Gets or sets the external provider name.
    /// </summary>
    public string Provider { get; set; }

    /// <summary>
    /// Gets or sets the external provider token.
    /// </summary>
    public string ExternalToken { get; set; }
}
