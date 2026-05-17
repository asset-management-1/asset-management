namespace Authentication.Application.Dtos.Authentications.ExternalProviders;

/// <summary>
/// Describes one supported external identity provider for authentication use cases.
/// </summary>
public class ExternalProviderDescriptorDto
{
    /// <summary>
    /// Gets or sets the provider name used by API contracts and persistence.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the default party type used when this provider provisions a new account.
    /// </summary>
    public string DefaultPartyType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this provider requires verified email for provisioning.
    /// </summary>
    public bool RequireVerifiedEmail { get; set; }
}
