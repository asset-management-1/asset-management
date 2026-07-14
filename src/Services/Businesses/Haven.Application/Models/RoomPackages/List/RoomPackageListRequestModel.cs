namespace Haven.Application.Models.RoomPackages.List;

/// <summary>
/// Represents a landlord-scoped request for effective room packages.
/// </summary>
public sealed class RoomPackageListRequestModel
{
    /// <summary>
    /// Gets or sets the current landlord context.
    /// </summary>
    public CurrentPartyContextModel CurrentParty { get; set; }

    /// <summary>
    /// Gets or sets the frontend-safe room identifier.
    /// </summary>
    public Guid RoomPublicId { get; set; }
}
