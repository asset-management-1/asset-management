namespace Authentication.Infrastructure.Options;

/// <summary>
/// Represents external authentication provider settings.
/// </summary>
public class ExternalAuthenticationOptions
{
    /// <summary>
    /// Configured external providers that are allowed for login and linking.
    /// </summary>
    public List<ExternalProviderOptions> Providers { get; set; } = [];
}

/// <summary>
/// Represents the configuration of one external identity provider.
/// </summary>
public class ExternalProviderOptions
{
    /// <summary>
    /// Provider name used by the API contract.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Provider authority base URL.
    /// </summary>
    public string Authority { get; set; }

    /// <summary>
    /// Optional metadata address for OIDC discovery.
    /// </summary>
    public string MetadataAddress { get; set; }

    /// <summary>
    /// Valid audiences expected in external tokens.
    /// </summary>
    public List<string> Audiences { get; set; } = [];

    /// <summary>
    /// Valid issuers expected in external tokens.
    /// </summary>
    public List<string> ValidIssuers { get; set; } = [];

    /// <summary>
    /// Default party type assigned to newly created external users.
    /// </summary>
    public string DefaultPartyType { get; set; } = "Tenant";

    /// <summary>
    /// Indicates whether verified email is required from the provider.
    /// </summary>
    public bool RequireVerifiedEmail { get; set; } = true;
}
