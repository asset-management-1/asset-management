namespace Haven.Application.Queries.GetRoomTenantJoinQr;

/// <summary>
/// Represents a request to generate a room tenant-join QR token.
/// </summary>
/// <param name="RoomPublicId">The room public identifier.</param>
/// <param name="RoleCode">The role code encoded into the token.</param>
public sealed record GetRoomTenantJoinQrQuery(
    Guid RoomPublicId,
    string RoleCode) : IQuery<ResponseDto<TenantJoinQrResponseDto>>;
