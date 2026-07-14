namespace Haven.Application.Models.Tenants.Detail;

/// <summary>
/// Represents a tenant detail request scoped to the current landlord party.
/// </summary>
public class TenantDetailRequestModel
{
    /// <summary>
    /// Gets or sets the current party context.
    /// </summary>
    public CurrentPartyContextModel CurrentParty { get; set; }

    /// <summary>
    /// Gets or sets the occupancy public identifier.
    /// </summary>
    public Guid OccupancyPublicId { get; set; }
}
