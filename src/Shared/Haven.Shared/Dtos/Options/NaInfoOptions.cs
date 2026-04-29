namespace Haven.Shared.Dtos.Options;

public class NaInfoOptions
{
    /// <summary>
    /// The base URL of the authentication server or API used for generating requests.
    /// </summary>
    public string BaseUrl { get; set; }

    /// <summary>
    /// Specifies the realm or domain within the identity provider's system used for authentication and authorization.
    /// This value may be required to uniquely identify a security domain or tenant.
    /// </summary>
    public string Realm { get; set; }

    /// <summary>
    /// Expected token issuer for validation.
    /// Example: <c>https://idp.example.com/</c>
    /// </summary>
    public string ValidIssuer { get; set; }

    /// <summary>
    /// Expected audience claim for access tokens consumed by this API/service.
    /// Example: <c>api://my-service</c>
    /// </summary>
    public string ValidAudience { get; set; }

    /// <summary>
    /// OIDC authority base URL used by JWT middleware to discover metadata.
    /// Example: <c>https://idp.example.com</c>
    /// </summary>
    public string Authority { get; set; }

    /// <summary>
    /// Explicit OpenID Connect discovery document endpoint (overrides default discovery when set).
    /// Example: <c>https://idp.example.com/.well-known/openid-configuration</c>
    /// </summary>
    public string OpenIdConfigEndpoint { get; set; }
}
