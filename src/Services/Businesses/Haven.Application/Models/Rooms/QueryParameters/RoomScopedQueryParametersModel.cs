namespace Haven.Application.Models.Rooms.QueryParameters;

/// <summary>
/// Parameters used to load one room under the current party scope.
/// </summary>
public class RoomScopedQueryParametersModel
{
    /// <summary>
    /// Gets or sets the current party internal identifier.
    /// </summary>
    public long CurrentPartyId { get; set; }

    /// <summary>
    /// Gets or sets relationship codes that grant property access.
    /// </summary>
    public IReadOnlyCollection<string> RelationshipCodes { get; set; } = [];

    /// <summary>
    /// Gets or sets the room public identifier.
    /// </summary>
    public Guid RoomPublicId { get; set; }

    /// <summary>
    /// Gets or sets the system package type code that does not constitute a room package override.
    /// </summary>
    public string NoFurniturePackageTypeCode { get; set; }
}
