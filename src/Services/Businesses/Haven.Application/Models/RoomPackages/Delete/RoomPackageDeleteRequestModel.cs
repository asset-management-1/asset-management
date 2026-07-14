namespace Haven.Application.Models.RoomPackages.Delete;

/// <summary>
/// Represents a landlord-scoped room package delete request.
/// </summary>
public sealed class RoomPackageDeleteRequestModel
{
    /// <summary>
    /// Gets or sets the current landlord context.
    /// </summary>
    public CurrentPartyContextModel CurrentParty { get; set; }

    /// <summary>
    /// Gets or sets the frontend-safe room identifier.
    /// </summary>
    public Guid RoomPublicId { get; set; }

    /// <summary>
    /// Gets or sets the frontend-safe package identifier.
    /// </summary>
    public Guid PackagePublicId { get; set; }
}
