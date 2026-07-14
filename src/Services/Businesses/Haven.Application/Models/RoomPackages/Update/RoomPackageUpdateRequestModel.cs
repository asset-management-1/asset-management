namespace Haven.Application.Models.RoomPackages.Update;

/// <summary>
/// Represents a landlord-scoped partial room package update.
/// </summary>
public sealed class RoomPackageUpdateRequestModel
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

    /// <summary>
    /// Gets or sets the optional replacement package name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the optional replacement price adjustment.
    /// </summary>
    public decimal? PriceAdjustment { get; set; }

    /// <summary>
    /// Gets or sets optional replacement items; <c>null</c> preserves current rows.
    /// </summary>
    public IReadOnlyList<RoomPackageItemRequestDto> Items { get; set; }
}
