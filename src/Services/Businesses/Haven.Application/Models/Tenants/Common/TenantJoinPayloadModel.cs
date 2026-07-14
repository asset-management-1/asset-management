namespace Haven.Application.Models.Tenants.Common;

/// <summary>
/// Represents the cache-backed payload stored for a room tenant-join token.
/// </summary>
public class TenantJoinPayloadModel
{
    /// <summary>
    /// Gets or sets the room public identifier.
    /// </summary>
    public Guid RoomPublicId { get; set; }

    /// <summary>
    /// Gets or sets the landlord party public identifier.
    /// </summary>
    public Guid LandlordPartyPublicId { get; set; }

    /// <summary>
    /// Gets or sets the encoded role code.
    /// </summary>
    public string RoleCode { get; set; }

    /// <summary>
    /// Gets or sets the internal absolute expiry used only to preserve the original TTL on restoration.
    /// </summary>
    public DateTimeOffset ExpiresAtUtc { get; set; }
}
