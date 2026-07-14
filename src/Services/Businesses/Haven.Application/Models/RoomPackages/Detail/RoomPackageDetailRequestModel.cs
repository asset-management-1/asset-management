namespace Haven.Application.Models.RoomPackages.Detail;

/// <summary>
/// Represents a landlord-scoped request for one effective room package.
/// </summary>
public sealed class RoomPackageDetailRequestModel
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
