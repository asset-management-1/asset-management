namespace Be.Haven.Cache.Models;

/// <summary>
/// Represents one in-memory cache-version counter and its cleanup window.
/// </summary>
/// <param name="Version">The current cache version value.</param>
/// <param name="ExpiresAtUtc">The UTC timestamp when this version state expires.</param>
internal sealed record CacheVersionStateModel(long Version, DateTimeOffset ExpiresAtUtc);
