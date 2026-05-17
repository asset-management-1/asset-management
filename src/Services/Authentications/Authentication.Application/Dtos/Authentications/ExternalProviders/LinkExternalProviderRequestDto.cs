namespace Authentication.Application.Dtos.Authentications.ExternalProviders;

/// <summary>
/// Represents the external-provider link payload used by the authentication service.
/// </summary>
public class LinkExternalProviderRequestDto
{
    public string Provider { get; set; }
    public string ExternalToken { get; set; }
}
