namespace Be.Haven.Core.Services;

/// <summary>
/// Concrete implementation of authentication helpers that reads identity, roles, scopes,
/// and token metadata from the current <see cref="HttpContext"/>.
/// </summary>
public class AuthService : IAuthService
{
    // Accessor to the ambient HttpContext for the current request.
    private readonly IHttpContextAccessor _http;

    /// <summary>
    /// Creates the service with an <see cref="IHttpContextAccessor"/> to reach the current request context.
    /// </summary>
    /// <param name="http">Accessor that exposes the current <see cref="HttpContext"/>.</param>
    public AuthService(
        IHttpContextAccessor http)
    {
        _http = http;
    }

    /// <summary>
    /// Gets the current <see cref="HttpContext"/> if available; can be <c>null</c> outside HTTP pipelines.
    /// </summary>
    private HttpContext Context => _http.HttpContext;

    /// <summary>
    /// Gets the <see cref="ClaimsPrincipal"/> representing the current user.
    /// </summary>
    public ClaimsPrincipal Principal => Context?.User;

    /// <summary>
    /// Indicates whether the current user is authenticated.
    /// </summary>
    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    /// <summary>
    /// Returns the authentication scheme used in the <c>Authorization</c> header (e.g., "Bearer").
    /// </summary>
    /// <returns>The scheme name, or <c>null</c> if not present or unparsable.</returns>
    public string Scheme()
    {
        // Authorization: "<Scheme> <Parameter>"
        if (Context?.Request.Headers is null) return null;
        return AuthenticationHeaderValue.TryParse(Context.Request.Headers.Authorization, out var a)
            ? a.Scheme
            : null;
    }

    /// <summary>
    /// Returns the raw token string (the <c>Parameter</c> part of the <c>Authorization</c> header).
    /// </summary>
    /// <returns>The raw token, or <c>null</c> if not present or unparsable.</returns>
    public string RawToken()
    {
        if (Context?.Request.Headers is null) return null;
        return AuthenticationHeaderValue.TryParse(Context.Request.Headers.Authorization, out var a)
            ? a.Parameter
            : null;
    }

    /// <summary>
    /// Attempts to retrieve a specific claim value from the current identity as a typed value.
    /// </summary>
    /// <param name="claimType">The type of claim to retrieve.</param>
    /// <param name="value">The output parameter to store the converted claim value, if successful.</param>
    /// <param name="provider">An optional format provider to assist with type conversion.</param>
    /// <typeparam name="T">The target type to which the claim value will be converted.</typeparam>
    /// <returns>True if the claim value could be successfully retrieved and converted; otherwise, false.</returns>
    public bool TryGet<T>(string claimType, out T value, IFormatProvider provider = null)
    {
        var raw = Get(claimType);
        return StringConvertHelper.TryConvert(raw, out value, provider);
    }

    /// <summary>
    /// Retrieves the value of a specified claim from the <see cref="ClaimsPrincipal"/>, converting it to the desired type.
    /// </summary>
    /// <typeparam name="T">The target type to which the claim value should be converted.</typeparam>
    /// <param name="claimType">The type of claim to search for.</param>
    /// <param name="defaultValue">The default value to return if the claim is not found or cannot be converted.</param>
    /// <param name="provider">An optional format provider to use during conversion.</param>
    /// <returns>The value of the claim converted to type <typeparamref name="T"/>, or the <paramref name="defaultValue"/> if conversion fails or the claim does not exist.</returns>
    public T Get<T>(string claimType, T defaultValue = default, IFormatProvider provider = null)
    {
        var raw = Get(claimType);
        if (string.IsNullOrWhiteSpace(raw))
            return defaultValue;

        return StringConvertHelper.TryConvert(raw, out T v, provider) ? v : defaultValue;
    }

    /// <summary>
    /// Gets all claims that match the specified claim type.
    /// </summary>
    /// <param name="claimType">The claim type to search.</param>
    /// <returns> An enumerable of matching claims; empty when none exist.</returns>
    public IEnumerable<Claim> GetAll(string claimType)
    {
        return Principal?.FindAll(claimType) ?? [];
    }

    /// <summary>
    /// Gets the user identifier from claims.
    /// </summary>
    /// <returns>User id string, or <c>null</c> if not present.</returns>
    public string UserId()
    {
        return Get(ClaimTypes.NameIdentifier) ?? Get("sub");
    }

    /// <summary>
    /// Retrieves the account identifier associated with the current user from the authentication context.
    /// The account identifier is obtained by checking for a claim with the key defined in <c>HEADER_ACCOUNT_ID</c>
    /// or, if unavailable, a claim with the key "account_id".
    /// </summary>
    /// <returns>A string representing the account identifier, or <c>null</c> if not found.</returns>
    public string AccountId()
    {
        return Get(HEADER_ACCOUNT_ID) ?? Get("account_id");
    }

    /// <summary>
    /// Gets the display/ the username from claims.
    /// </summary>
    /// <returns> Username, or <c>null</c> if not present.</returns>
    public string UserName()
    {
        return Get(ClaimTypes.Name) ?? Get("name");
    }

    /// <summary>
    /// Gets the email address from claims.
    /// </summary>
    /// <returns>Email address, or <c>null</c> if not present.</returns>
    public string Email()
    {
        return Get(ClaimTypes.Email) ?? Get("email");
    }

    /// <summary>
    /// Returns a de-duplicated list of roles granted to the user.
    /// </summary>
    /// <returns>A read-only list of role names (possibly empty).</returns>
    public IReadOnlyList<string> Roles()
    {
        var roles = (Principal?.Claims ?? [])
                    .Where(c => c.Type is "role" or "roles" or "na_role" or ClaimTypes.Role)
                    .SelectMany(c => c.Value.Split(
                        [',', ';', ' '],
                        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();
        return roles;
    }

    /// <summary>
    /// Checks whether the user has the specified role (case-insensitive).
    /// </summary>
    /// <param name="role">The role name to check.</param>
    /// <returns><c>true</c> if the user has the role; otherwise <c>false</c>.</returns>
    public bool HasRole(string role)
    {
        return Roles().Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns a de-duplicated list of scopes granted to the user/token.
    /// </summary>
    /// <returns>A read-only list of scopes (possibly empty).</returns>
    public IReadOnlyList<string> Scopes()
    {
        var scopes = (Principal?.Claims ?? [])
                     .Where(c => c.Type == "scope")
                     .SelectMany(c => c.Value.Split(' ',
                         StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                     .Distinct(StringComparer.Ordinal)
                     .ToArray();
        return scopes;
    }

    /// <summary>
    /// Checks whether the specified scope exists in the current scope set.
    /// </summary>
    /// <param name="scope">The scope token to check (e.g., <c>read:orders</c>).</param>
    /// <returns><c>true</c> if present; otherwise <c>false</c>.</returns>
    public bool HasScope(string scope)
    {
        return Scopes().Contains(scope);
    }

    /// <summary>
    /// Returns the JWT issuer (<c>iss</c>) by decoding (not validating) the raw access token.
    /// </summary>
    /// <returns>Issuer string, or <c>null</c> if the token is missing or malformed.</returns>
    public string Issuer()
    {
        var token = RawToken();
        if (string.IsNullOrWhiteSpace(token)) return null;
        try
        {
            return new JwtSecurityTokenHandler().ReadJwtToken(token).Issuer;
        }
        catch
        {
            // If the header isn't a valid JWT, treat the issuer as unknown.
            return null;
        }
    }

    /// <summary>
    /// Returns the JWT expiration time (<c>exp</c>) as Unix seconds since epoch.
    /// </summary>
    /// <returns>The expiration in Unix time, or <c>null</c> if missing/invalid/malformed.</returns>
    public long? ExpUnix()
    {
        var token = RawToken();
        if (string.IsNullOrWhiteSpace(token)) return null;
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        return long.TryParse(jwt.Claims.FirstOrDefault(c => c.Type == "exp")?.Value, out var v) ? v : null;
    }

    /// <summary>
    /// Retrieves the "nbf" (Not Before) claim value from the current JWT token as a Unix timestamp.
    /// </summary>
    /// <returns>The "nbf" claim value as a long Unix timestamp, or <c>null</c> if the claim is not present or cannot be parsed.</returns>
    public long? NbfUnix()
    {
        var token = RawToken();
        if (string.IsNullOrWhiteSpace(token)) return null;
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        return long.TryParse(jwt.Claims.FirstOrDefault(c => c.Type == "nbf")?.Value, out var v) ? v : null;
    }

    /// <summary>
    /// Throws <see cref="UnauthorizedAccessException"/> if the current user is not authenticated.
    /// Use this as a guard in handlers/services that require a signed-in user.
    /// </summary>
    public void EnsureAuthenticated()
    {
        if (!IsAuthenticated) throw new UnauthorizedAccessException();
    }

    /// <summary>
    /// Retrieves the value of a specific claim from the current user's claims.
    /// </summary>
    /// <param name="claimType">The type of claim to retrieve.</param>
    /// <returns>The value of the claim if it exists; otherwise, null.</returns>
    private string Get(string claimType)
    {
        return Principal?.FindFirst(claimType)?.Value;
    }
}