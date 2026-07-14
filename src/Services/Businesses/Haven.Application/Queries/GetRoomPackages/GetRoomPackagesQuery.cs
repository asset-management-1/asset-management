namespace Haven.Application.Queries.GetRoomPackages;

/// <summary>
/// Represents a request for one room's effective package list.
/// </summary>
public sealed class GetRoomPackagesQuery : IQuery<ResponseDto<RoomPackageListResponseDto>>
{
    /// <summary>
    /// Gets or sets the frontend-safe room identifier.
    /// </summary>
    public Guid RoomId { get; set; }
}
