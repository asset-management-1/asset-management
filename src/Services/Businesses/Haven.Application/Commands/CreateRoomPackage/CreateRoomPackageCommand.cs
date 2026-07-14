namespace Haven.Application.Commands.CreateRoomPackage;

/// <summary>
/// Represents a request to add one package to a room's effective package list.
/// </summary>
public sealed class CreateRoomPackageCommand : ICommand<ResponseDto<RoomPackageDetailResponseDto>>
{
    /// <summary>
    /// Gets or sets the frontend-safe room identifier receiving the package.
    /// </summary>
    public Guid RoomId { get; set; }

    /// <summary>
    /// Gets or sets the landlord-provided package display name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the amount added to the room rent when this package is selected.
    /// </summary>
    public decimal PriceAdjustment { get; set; }

    /// <summary>
    /// Gets or sets the ordered package item labels submitted by the landlord.
    /// </summary>
    public IReadOnlyList<RoomPackageItemRequestDto> Items { get; set; } = [];
}
