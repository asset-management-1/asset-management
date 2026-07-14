namespace Haven.Application.Interfaces.Services;

/// <summary>
/// Provides standalone room package list, detail, create, update, and delete workflows.
/// </summary>
public interface IRoomPackageService
{
    /// <summary>
    /// Loads the effective package list for one landlord-scoped room.
    /// </summary>
    /// <param name="request">The current landlord context and room identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the read.</param>
    /// <returns>The room header, common-package flag, and effective packages.</returns>
    Task<RoomPackageListResponseDto> GetListAsync(
        RoomPackageListRequestModel request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads one package from a room's effective package set.
    /// </summary>
    /// <param name="request">The current landlord context and package identity.</param>
    /// <param name="cancellationToken">The token used to cancel the read.</param>
    /// <returns>The requested package detail.</returns>
    Task<RoomPackageDetailResponseDto> GetDetailAsync(
        RoomPackageDetailRequestModel request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a room-owned package after materializing common packages when required.
    /// </summary>
    /// <param name="request">The package fields and current landlord context.</param>
    /// <param name="cancellationToken">The token used to cancel the mutation.</param>
    /// <returns>The created room package detail.</returns>
    Task<RoomPackageDetailResponseDto> CreateAsync(
        RoomPackageCreateRequestModel request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies submitted partial fields to one effective room package.
    /// </summary>
    /// <param name="request">The package identity, submitted fields, and current landlord context.</param>
    /// <param name="cancellationToken">The token used to cancel the mutation.</param>
    /// <returns>The updated room package detail.</returns>
    Task<RoomPackageDetailResponseDto> UpdateAsync(
        RoomPackageUpdateRequestModel request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes one editable room package while preserving the default package invariant.
    /// </summary>
    /// <param name="request">The package identity and current landlord context.</param>
    /// <param name="cancellationToken">The token used to cancel the mutation.</param>
    /// <returns>The successful operation status.</returns>
    Task<OperationStatusResponseDto> DeleteAsync(
        RoomPackageDeleteRequestModel request,
        CancellationToken cancellationToken = default);
}
