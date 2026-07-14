namespace Haven.Application.Models.Tenants.Delete;

/// <summary>
/// Represents a tenant move-out request scoped to the current landlord party.
/// </summary>
public class TenantDeleteRequestModel
{
    /// <summary>
    /// Gets or sets the current landlord party context.
    /// </summary>
    public CurrentPartyContextModel CurrentParty { get; set; }

    /// <summary>
    /// Gets or sets the occupancy public identifier.
    /// </summary>
    public Guid OccupancyPublicId { get; set; }
}
