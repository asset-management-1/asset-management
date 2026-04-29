namespace Haven.Core.Helpers;

public static class TokenHelper
{
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
        // normalize
        if (remainingSeconds is not > 0)
            return TimeSpan.FromSeconds(PADDING_SECONDS);

        // subtract padding (avoid cutting too close to expiry)
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
        return dateTime - TimeSpan.FromSeconds(PADDING_SECONDS);
    }

    /// <summary>Reads a Unix timestamp claim (ms or s) and returns it as UTC.</summary>
    /// <param name="jwt">The JWT to read from.</param>
    /// <param name="claimType">Claim type key (e.g., "na_exp").</param>
    /// <param name="utc">Parsed UTC time when successful.</param>
    /// <returns><c>true</c> if parsed; otherwise <c>false</c>.</returns>
    public static bool TryGetUnixTimeUtc(JwtSecurityToken jwt, string claimType, out DateTime utc)
    {
        utc = default;
        var raw = jwt?.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
        if (string.IsNullOrWhiteSpace(raw) || !long.TryParse(raw, out var v)) return false;

        // Heuristic: > 10^12 => milliseconds; otherwise seconds.
        var dto = v > 1_000_000_000_000
            ? DateTimeOffset.FromUnixTimeMilliseconds(v)
            : DateTimeOffset.FromUnixTimeSeconds(v);

        utc = dto.UtcDateTime;
        return true;
    }

    /// <summary>Adds an alias claim from <paramref name="fromType"/> to <paramref name="toType"/> if missing.</summary>
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
        var val = src.FindFirst(fromType)?.Value;
        if (string.IsNullOrWhiteSpace(val)) return;

        // remove existing toType from bag (if any)
        bag.RemoveAll(c => c.Type == toType);

        // add new toType
        bag.Add(new Claim(toType, val));
    }
    
    /// <summary>
    /// Returns a safe token prefix for logging.
    /// Never log full token; only prefix is enough for correlation/debugging.
    /// </summary>
    public static string SafeTokenPrefix(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return string.Empty;

        return token.Length <= 12 ? token : token[..12];
    }
}
