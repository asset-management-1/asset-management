namespace Haven.Application.Models.Properties.QueryParameters;

/// <summary>
/// Parameters used to load one property within a party scope.
/// </summary>
public class PropertyScopedQueryParametersModel
{
    public long CurrentPartyId { get; set; }

    public IReadOnlyCollection<string> RelationshipCodes { get; set; } = [];

    public Guid PropertyPublicId { get; set; }
}

