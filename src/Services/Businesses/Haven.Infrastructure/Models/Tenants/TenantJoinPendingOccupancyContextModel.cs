namespace Haven.Infrastructure.Models.Tenants;

/// <summary>
/// Carries the scoped room, tenant, lookup, and one-time token state for one pending room join.
/// </summary>
public class TenantJoinPendingOccupancyContextModel
{
    /// <summary>
    /// Gets or sets the scoped room projection selected by the QR token.
    /// </summary>
    public TenantJoinRoomRowModel Room { get; set; }

    /// <summary>
    /// Gets or sets the scanning tenant party.
    /// </summary>
    public Party TenantParty { get; set; }

    /// <summary>
    /// Gets or sets the normalized role encoded in the QR token.
    /// </summary>
    public string RoleCode { get; set; }

    /// <summary>
    /// Gets or sets the exact master-data values required by pending occupancy creation.
    /// </summary>
    public TenantMasterDataContextModel Lookups { get; set; }

    /// <summary>
    /// Gets or sets the cache key consumed by this join mutation.
    /// </summary>
    public string CacheKey { get; set; }

    /// <summary>
    /// Gets or sets the cached payload retained for bounded pre-commit restoration.
    /// </summary>
    public TenantJoinPayloadModel Payload { get; set; }
}
