namespace Be.Haven.Core.Helpers;

/// <summary>
/// Provides token time, claim, and log-safe token utilities.
/// </summary>
public static class TokenHelper
{
    /// <summary>
    /// Builds the cache key used to store a user's auth reset timestamp.
    /// </summary>
    /// <param name="userPublicId">The public user identifier.</param>
    /// <returns>The auth reset cache key.</returns>
    public static string BuildAuthResetCacheKey(Guid userPublicId)
    {
        // Keep the key format centralized so token issue, reset, and validation paths share one Redis contract.
        return string.Format(AUTH_RESET_AT_KEY_PATTERN, userPublicId);
    }

    /// <summary>
    /// Converts a UTC timestamp to Unix milliseconds for stable token comparison.
    /// </summary>
    /// <param name="utcDateTime">The UTC timestamp to convert.</param>
    /// <returns>The Unix millisecond value.</returns>
    public static long ToUnixTimeMilliseconds(DateTime utcDateTime)
    {
        // Normalize to UTC before conversion so DB values and JWT claims compare consistently.
        var utc = utcDateTime.Kind switch
        {
            DateTimeKind.Utc => utcDateTime,
            DateTimeKind.Local => utcDateTime.ToUniversalTime(),
            _ => DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc)
        };

        return new DateTimeOffset(utc).ToUnixTimeMilliseconds();
    }

    /// <summary>
    /// Converts an optional auth reset timestamp to Unix milliseconds.
    /// </summary>
    /// <param name="authResetAt">The optional UTC auth reset timestamp.</param>
    /// <returns>The Unix millisecond value, or <c>0</c> when no reset exists.</returns>
    public static long ToAuthResetUnixMilliseconds(DateTime? authResetAt)
    {
        // Zero is the cache sentinel for users that have never reset authentication state.
        return authResetAt.HasValue ? ToUnixTimeMilliseconds(authResetAt.Value) : 0;
    }

    /// <summary>
    /// Builds the auth reset cache lifetime from refresh-token configuration.
    /// </summary>
    /// <param name="refreshTokenDays">The configured refresh-token lifetime in days.</param>
    /// <returns>The cache lifetime for auth reset markers.</returns>
    public static TimeSpan GetAuthResetCacheTtl(int refreshTokenDays)
    {
        // Keep reset markers at least as long as refresh-token sessions can exist.
        return TimeSpan.FromDays(refreshTokenDays + AUTH_RESET_CACHE_TTL_PADDING_DAYS);
    }

    /// <summary>
    /// Calculates the remaining time until the token expires, subtracting safety padding to ensure early refresh.
    /// </summary>
    /// <param name="tokenExpiration">The expiration DateTime of the token.</param>
    /// <returns>
    /// A TimeSpan representing the safe duration until expiration.
    /// If the calculated duration is negative or zero, a minimum padding duration is returned instead.
    /// </returns>
    public static TimeSpan GetExpirationDateTimeNowSpan(DateTime tokenExpiration)
    {
        // Subtract padding so callers refresh token-dependent work before the token boundary.
        var expirationTime = tokenExpiration - DateTime.Now;
        var safeExpirationTime = expirationTime - TimeSpan.FromSeconds(PADDING_SECONDS);

        return safeExpirationTime > TimeSpan.Zero ? safeExpirationTime : TimeSpan.FromSeconds(PADDING_SECONDS);
    }

    /// <summary>
    /// Converts a token remaining lifetime (in seconds) to a safe TimeSpan by subtracting padding.
    /// If result is non-positive, returns the padding as a minimum.
    /// </summary>
    /// <param name="remainingSeconds">Remaining lifetime in seconds.</param>
    /// <returns>
    /// A safe TimeSpan for caching/refresh decisions (remainingSeconds - padding, min = padding).
    /// </returns>
    public static TimeSpan GetSafeTtlSpan(long? remainingSeconds)
    {
        // Missing or expired token lifetime falls back to the minimum safety padding.
        if (remainingSeconds is not > 0)
        {
            return TimeSpan.FromSeconds(PADDING_SECONDS);
        }

        // Subtract padding so cache entries expire before the token does.
        var safeSeconds = remainingSeconds.Value - PADDING_SECONDS;

        return safeSeconds > 0
            ? TimeSpan.FromSeconds(safeSeconds)
            : TimeSpan.FromSeconds(PADDING_SECONDS);
    }

    /// <summary>
    /// Returns a DateTime value adjusted by subtracting a predefined safety padding in seconds.
    /// Useful for pre-expiry checks.
    /// </summary>
    /// <param name="dateTime">The original expiration DateTime.</param>
    /// <returns>A new DateTime adjusted with padding subtracted.</returns>
    public static DateTime GetSafeTime(DateTime dateTime)
    {
        // Move the deadline earlier so callers can perform pre-expiry checks safely.
        return dateTime - TimeSpan.FromSeconds(PADDING_SECONDS);
    }

    /// <summary>
    /// Reads a Unix timestamp claim in milliseconds or seconds and returns it as UTC.
    /// </summary>
    /// <param name="jwt">The JWT to read from.</param>
    /// <param name="claimType">Claim type key (e.g., "na_exp").</param>
    /// <param name="utc">Parsed UTC time when successful.</param>
    /// <returns><c>true</c> if parsed; otherwise <c>false</c>.</returns>
    public static bool TryGetUnixTimeUtc(JwtSecurityToken jwt, string claimType, out DateTime utc)
    {
        utc = default;

        // Missing or malformed timestamp claims are treated as parse failures.
        var raw = jwt?.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
        if (string.IsNullOrWhiteSpace(raw) || !long.TryParse(raw, out var v))
        {
            return false;
        }

        // Heuristic: > 10^12 => milliseconds; otherwise seconds.
        var dto = v > 1_000_000_000_000
            ? DateTimeOffset.FromUnixTimeMilliseconds(v)
            : DateTimeOffset.FromUnixTimeSeconds(v);

        utc = dto.UtcDateTime;
        return true;
    }

    /// <summary>
    /// Adds an alias claim from <paramref name="fromType"/> to <paramref name="toType"/> when source data exists.
    /// </summary>
    /// <param name="src">Source identity.</param>
    /// <param name="bag">Target claim collection to append to.</param>
    /// <param name="fromType">Existing claim type to copy.</param>
    /// <param name="toType">Alias claim type to add.</param>
    public static void ReplaceClaim(
        ClaimsIdentity src,
        List<Claim> bag,
        string fromType,
        string toType)
    {
        // Read the source claim once so alias replacement is skipped when the source is absent.
        var val = src.FindFirst(fromType)?.Value;
        if (string.IsNullOrWhiteSpace(val))
        {
            return;
        }

        // Remove the old alias before adding the normalized alias value.
        bag.RemoveAll(c => c.Type == toType);

        // Add the alias claim expected by downstream auth code.
        bag.Add(new Claim(toType, val));
    }

    /// <summary>
    /// Adds a claim only when the target claim type and value pair is not already present on the identity.
    /// </summary>
    /// <param name="identity">The identity that will receive the claim when it is missing.</param>
    /// <param name="claimType">The claim type to ensure on the identity.</param>
    /// <param name="claimValue">The claim value to ensure on the identity.</param>
    public static void EnsureClaim(ClaimsIdentity identity, string claimType, string claimValue)
    {
        // Avoid duplicating claim pairs when auth handlers run alias normalization repeatedly.
        if (identity.HasClaim(claimType, claimValue))
        {
            return;
        }

        identity.AddClaim(new Claim(claimType, claimValue));
    }

    /// <summary>
    /// Returns a safe token prefix for logging.
    /// Never log full token; only prefix is enough for correlation/debugging.
    /// </summary>
    /// <param name="token">The token value to reduce for log correlation.</param>
    /// <returns>An empty string for blank tokens; otherwise a short token prefix.</returns>
    public static string SafeTokenPrefix(string token)
    {
        // Blank tokens should not add noisy placeholders to logs.
        if (string.IsNullOrWhiteSpace(token))
        {
            return string.Empty;
        }

        // Prefix-only logging supports correlation without exposing the full secret.
        return token.Length <= 12 ? token : token[..12];
    }
}
