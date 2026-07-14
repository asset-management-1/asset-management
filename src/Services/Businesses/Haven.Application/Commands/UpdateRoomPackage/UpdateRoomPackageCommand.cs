namespace Haven.Application.Commands.UpdateRoomPackage;

/// <summary>
/// Represents a partial update for one effective room package.
/// </summary>
public sealed class UpdateRoomPackageCommand : ICommand<ResponseDto<RoomPackageDetailResponseDto>>
{
    /// <summary>
    /// Gets or sets the frontend-safe package identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the room context used to resolve common or room-owned package data.
    /// </summary>
    public Guid RoomId { get; set; }

    /// <summary>
    /// Gets or sets the optional replacement display name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the optional replacement rent adjustment.
    /// </summary>
    public decimal? PriceAdjustment { get; set; }

    /// <summary>
    /// Gets or sets the optional replacement item list; null preserves and an empty list clears persisted items.
    /// </summary>
    public IReadOnlyList<RoomPackageItemRequestDto> Items { get; set; }
}
