namespace Haven.Application.Models.Tenants.QueryParameters;

/// <summary>
/// Dapper parameters for one landlord-scoped tenant occupancy.
/// </summary>
public class TenantScopedQueryParametersModel
{
    /// <summary>
    /// Gets or sets the current landlord party identifier.
    /// </summary>
    public long CurrentPartyId { get; set; }

    /// <summary>
    /// Gets or sets allowed property relationship codes.
    /// </summary>
    public IReadOnlyCollection<string> RelationshipCodes { get; set; } = [];

    /// <summary>
    /// Gets or sets the occupancy public identifier.
    /// </summary>
    public Guid OccupancyPublicId { get; set; }
}
