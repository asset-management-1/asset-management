namespace Authentication.Infrastructure.Options.ExternalProviders;

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
