namespace Haven.Application.Interfaces.Services;

/// <summary>
/// Provides room list, detail, update, and delete workflows.
/// </summary>
public interface IRoomService
{
    /// <summary>
    /// Loads grouped rooms scoped to the current party.
    /// </summary>
    /// <param name="request">The party-scoped room list request.</param>
    /// <param name="cancellationToken">The token used to cancel the read.</param>
    /// <returns>The paged grouped room response.</returns>
    Task<PaginationResponse<IReadOnlyList<RoomListPropertyResponseDto>>> GetRoomsAsync(
        RoomListRequestModel request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads one room detail scoped to the current party.
    /// </summary>
    /// <param name="request">The party-scoped room detail request.</param>
    /// <param name="cancellationToken">The token used to cancel the read.</param>
    /// <returns>The room detail response.</returns>
    Task<RoomDetailResponseDto> GetRoomDetailAsync(
        RoomDetailRequestModel request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates one room and returns refreshed detail.
    /// </summary>
    /// <param name="request">The party-scoped room update request.</param>
    /// <param name="cancellationToken">The token used to cancel the room mutation.</param>
    /// <returns>The refreshed room detail response.</returns>
    Task<RoomDetailResponseDto> UpdateRoomAsync(
        RoomUpdateRequestModel request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes one room when protected dependencies allow it.
    /// </summary>
    /// <param name="request">The party-scoped room delete request.</param>
    /// <param name="cancellationToken">The token used to cancel the room deletion.</param>
    /// <returns>The operation status response.</returns>
    Task<OperationStatusResponseDto> DeleteRoomAsync(
        RoomDeleteRequestModel request,
        CancellationToken cancellationToken = default);
}
