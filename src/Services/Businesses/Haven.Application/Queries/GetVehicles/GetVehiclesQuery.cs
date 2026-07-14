namespace Haven.Application.Queries.GetVehicles;

/// <summary>
/// Represents one request to load every active vehicle inside a room.
/// </summary>
public sealed record GetVehiclesQuery(Guid RoomId)
    : IQuery<ResponseDto<IReadOnlyList<VehicleListItemResponseDto>>>;
