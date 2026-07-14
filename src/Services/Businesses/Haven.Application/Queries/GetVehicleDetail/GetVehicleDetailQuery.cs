namespace Haven.Application.Queries.GetVehicleDetail;

/// <summary>
/// Represents a request to load one landlord-scoped vehicle detail.
/// </summary>
public sealed record GetVehicleDetailQuery(Guid RoomId, Guid VehiclePublicId)
    : IQuery<ResponseDto<VehicleDetailResponseDto>>;
