namespace Haven.Application.Models.RoomPackages.Create;

/// <summary>
/// Represents a landlord-scoped room package creation request.
/// </summary>
public sealed class RoomPackageCreateRequestModel
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
    /// Gets or sets the package display name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the amount added to room rent.
    /// </summary>
    public decimal PriceAdjustment { get; set; }

    /// <summary>
    /// Gets or sets the ordered submitted package items.
    /// </summary>
    public IReadOnlyList<RoomPackageItemRequestDto> Items { get; set; } = [];
}
