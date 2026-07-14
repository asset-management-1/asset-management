namespace Haven.Application.Models.RoomPackages.QueryParameters;

/// <summary>
/// Represents the scoped parameters for reading user-managed packages for one room.
/// </summary>
public sealed class RoomPackageQueryParametersModel
{
    /// <summary>
    /// Gets or sets the current landlord party identifier.
    /// </summary>
    public long CurrentPartyId { get; set; }

    /// <summary>
    /// Gets or sets relationship codes that grant access to the room's property.
    /// </summary>
    public IReadOnlyCollection<string> RelationshipCodes { get; set; } = [];

    /// <summary>
    /// Gets or sets the frontend-safe room identifier.
    /// </summary>
    public Guid RoomPublicId { get; set; }

    /// <summary>
    /// Gets or sets the active package-status code used by the effective package query.
    /// </summary>
    public string ActiveStatusCode { get; set; }

    /// <summary>
    /// Gets or sets the fallback package-type code excluded from package-management responses.
    /// </summary>
    public string NoFurniturePackageTypeCode { get; set; }
}
