namespace Haven.Application.Models.Tenants.QueryParameters;

/// <summary>
/// Dapper parameters for QR join room preview.
/// </summary>
public class TenantJoinRoomQueryParametersModel
{
    /// <summary>
    /// Gets or sets the landlord party public identifier encoded in the token.
    /// </summary>
    public Guid LandlordPartyPublicId { get; set; }

    /// <summary>
    /// Gets or sets allowed property relationship codes.
    /// </summary>
    public IReadOnlyCollection<string> RelationshipCodes { get; set; } = [];

    /// <summary>
    /// Gets or sets the room public identifier encoded in the token.
    /// </summary>
    public Guid RoomPublicId { get; set; }
}
