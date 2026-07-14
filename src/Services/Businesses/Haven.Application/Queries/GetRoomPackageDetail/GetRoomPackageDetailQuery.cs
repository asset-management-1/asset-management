namespace Haven.Application.Queries.GetRoomPackageDetail;

/// <summary>
/// Represents a request for one effective package in a room context.
/// </summary>
/// <param name="PackagePublicId">The frontend-safe package identifier.</param>
/// <param name="RoomPublicId">The room context used to resolve effective package ownership.</param>
public sealed record GetRoomPackageDetailQuery(Guid PackagePublicId, Guid RoomPublicId)
    : IQuery<ResponseDto<RoomPackageDetailResponseDto>>;
