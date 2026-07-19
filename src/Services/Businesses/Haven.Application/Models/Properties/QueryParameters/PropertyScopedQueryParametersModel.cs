namespace Haven.Application.Models.Properties.QueryParameters;

/// <summary>
/// Parameters used to load one property within a party scope.
/// </summary>
public class PropertyScopedQueryParametersModel
{
    /// <summary>
    /// Gets or sets the current party identifier.
    /// </summary>
    public long CurrentPartyId { get; set; }

    /// <summary>
    /// Gets or sets relationship codes that grant property access.
    /// </summary>
    public IReadOnlyCollection<string> RelationshipCodes { get; set; } = [];

    /// <summary>
    /// Gets or sets the public property identifier.
    /// </summary>
    public Guid PropertyPublicId { get; set; }
}

