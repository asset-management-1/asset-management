namespace Haven.Application.Models.Tenants.Join;

/// <summary>
/// Represents a landlord request to generate a room tenant-join QR token.
/// </summary>
public class TenantJoinQrRequestModel
{
    /// <summary>
    /// Gets or sets the current landlord party context.
    /// </summary>
    public CurrentPartyContextModel CurrentParty { get; set; }

    /// <summary>
    /// Gets or sets the room public identifier.
    /// </summary>
    public Guid RoomPublicId { get; set; }

    /// <summary>
    /// Gets or sets the role code encoded into the token.
    /// </summary>
    public string RoleCode { get; set; }
}
