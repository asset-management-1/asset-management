namespace Authentication.Application.Dtos.Authentications.ExternalProviders;

/// <summary>
/// Represents the normalised profile extracted from an external identity provider token.
/// </summary>
public class ExternalIdentityProfileResponseDto
{
    /// <summary>
    /// Gets or sets the provider-specific user identifier.
    /// </summary>
    public string ProviderUserId { get; set; }

    /// <summary>
    /// Gets or sets the email address returned by the provider.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Gets or sets the full name returned by the provider.
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the provider confirmed the email address.
    /// </summary>
    public bool EmailVerified { get; set; }

    /// <summary>
    /// Indicates whether this provider is authoritative enough to auto-link an existing local email.
    /// </summary>
    public bool CanAutoLinkByEmail { get; set; }
}
