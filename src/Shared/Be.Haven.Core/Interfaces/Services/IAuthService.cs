namespace Be.Haven.Core.Interfaces.Services;

/// <summary>
/// Abstraction for reading authentication/authorization details of the current request/user.
/// Designed to wrap HttpContext/identity providers behind a stable interface.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Indicates whether the current user context is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Gets the underlying <see cref="ClaimsPrincipal"/> for the current user.
    /// </summary>
    ClaimsPrincipal Principal { get; }

    /// <summary>
    /// Returns the authentication scheme name used for the current request (e.g., "Bearer").
    /// </summary>
    string Scheme();

    /// <summary>
    /// Returns the raw access token string from the current request, if available.
    /// </summary>
    string RawToken();

    /// <summary>
    /// Attempts to retrieve a specific claim value from the current identity as a typed value.
    /// </summary>
    /// <param name="claimType">The type of claim to retrieve.</param>
    /// <param name="value">The output parameter to store the converted claim value, if successful.</param>
    /// <param name="provider">An optional format provider to assist with type conversion.</param>
    /// <typeparam name="T">The target type to which the claim value will be converted.</typeparam>
    /// <returns>True if the claim value could be successfully retrieved and converted; otherwise, false.</returns>
    bool TryGet<T>(string claimType, out T value, IFormatProvider provider = null);

    /// <summary>
    /// Retrieves the value of a specified claim from the <see cref="ClaimsPrincipal"/>, converting it to the desired type.
    /// </summary>
    /// <typeparam name="T">The target type to which the claim value should be converted.</typeparam>
    /// <param name="claimType">The type of claim to search for.</param>
    /// <param name="defaultValue">The default value to return if the claim is not found or cannot be converted.</param>
    /// <param name="provider">An optional format provider to use during conversion.</param>
    /// <returns>The value of the claim converted to type <typeparamref name="T"/>, or the <paramref name="defaultValue"/> if conversion fails or the claim does not exist.</returns>
    T Get<T>(string claimType, T defaultValue = default, IFormatProvider provider = null);

    /// <summary>
    /// Gets all claims for a given claim type.
    /// </summary>
    /// <param name="claimType">The claim type to search for.</param>
    /// <returns> An enumerable of matching claims; empty if none.</returns>
    IEnumerable<Claim> GetAll(string claimType);

    /// <summary>
    /// Gets the authenticated user's public identifier from the normalized user-id claim.
    /// </summary>
    /// <returns>The current user's public identifier, or <c>null</c> when the request is anonymous or invalid.</returns>
    Guid? UserId();

    /// <summary>
    /// Gets the authenticated session public identifier from the normalized session claim.
    /// </summary>
    /// <returns>The current session public identifier, or <c>null</c> when the request is anonymous or invalid.</returns>
    Guid? SessionId();

    /// <summary>
    /// Retrieves the account identifier associated with the current user or context.
    /// </summary>
    /// <returns>The account identifier as a string.</returns>
    string AccountId();

    /// <summary>
    /// Gets the username from claims.
    /// </summary>
    /// <returns> Username or <c>null</c> if not present.</returns>
    string UserName();

    /// <summary>
    /// Gets the email address from claims.
    /// </summary>
    /// <returns>Email address or <c>null</c> if not present.</returns>
    string Email();

    /// <summary>
    /// Gets the list of role names assigned to the user.
    /// </summary>
    /// <returns>Read-only list of roles (possibly empty).</returns>
    IReadOnlyList<string> Roles();

    /// <summary>
    /// Checks whether the user has the specified role.
    /// </summary>
    /// <param name="role">Role name to check.</param>
    /// <returns><c>true</c> if the user has the role; otherwise <c>false</c>.</returns>
    bool HasRole(string role);

    /// <summary>
    /// Gets the list of OAuth/OIDC scopes granted to the access token.
    /// </summary>
    /// <returns>Read-only list of scopes (possibly empty).</returns>
    IReadOnlyList<string> Scopes();

    /// <summary>
    /// Checks whether the user has the specified scope.
    /// </summary>
    /// <param name="scope">Scope to check (e.g., "read:orders").</param>
    /// <returns><c>true</c> if the scope is present; otherwise <c>false</c>.</returns>
    bool HasScope(string scope);

    /// <summary>
    /// Gets the token issuer (iss) value.
    /// </summary>
    /// <returns>Issuer string or <c>null</c> if not present.</returns>
    string Issuer();

    /// <summary>
    /// Gets the token expiration as Unix time (seconds since epoch).
    /// </summary>
    /// <returns>Expiration in Unix seconds; <c>null</c> if not present.</returns>
    long? ExpUnix();

    /// <summary>
    /// Retrieves the "nbf" (Not Before) claim value from the current JWT token as a Unix timestamp.
    /// </summary>
    /// <returns>The "nbf" claim value as a long Unix timestamp, or <c>null</c> if the claim is not present or cannot be parsed.</returns>
    long? NbfUnix();

    /// <summary>
    /// Ensures the current user is authenticated; intended to guard handlers/services.
    /// </summary>
    /// <exception cref="System.UnauthorizedAccessException">
    /// Thrown when the user is not authenticated.
    /// </exception>
    void EnsureAuthenticated();
}
