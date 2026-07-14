namespace Haven.Application.Interfaces.Services;

/// <summary>
/// Provides landlord vehicle registration, detail, update, and delete workflows.
/// </summary>
public interface IVehicleService
{
    /// <summary>
    /// Loads every active vehicle inside a landlord-managed room.
    /// </summary>
    /// <param name="request">The party and room scope.</param>
    /// <param name="cancellationToken">The token used to cancel the read.</param>
    /// <returns>The compact vehicle management collection.</returns>
    Task<IReadOnlyList<VehicleListItemResponseDto>> GetVehiclesAsync(
        VehicleListRequestModel request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates one room vehicle under an eligible primary tenant payer.
    /// </summary>
    /// <param name="request">The party-scoped vehicle create request.</param>
    /// <param name="cancellationToken">The token used to cancel the mutation.</param>
    /// <returns>The created vehicle detail response.</returns>
    Task<VehicleDetailResponseDto> CreateAsync(VehicleCreateRequestModel request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads one vehicle detail inside the current landlord scope.
    /// </summary>
    /// <param name="request">The party-scoped vehicle detail request.</param>
    /// <param name="cancellationToken">The token used to cancel the read.</param>
    /// <returns>The vehicle detail response.</returns>
    Task<VehicleDetailResponseDto> GetDetailAsync(VehicleDetailRequestModel request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates one vehicle without moving it to a different room.
    /// </summary>
    /// <param name="request">The party-scoped vehicle update request.</param>
    /// <param name="cancellationToken">The token used to cancel the mutation.</param>
    /// <returns>The refreshed vehicle detail response.</returns>
    Task<VehicleDetailResponseDto> UpdateAsync(VehicleUpdateRequestModel request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes one vehicle.
    /// </summary>
    /// <param name="request">The party-scoped vehicle delete request.</param>
    /// <param name="cancellationToken">The token used to cancel the mutation.</param>
    /// <returns>The operation status response.</returns>
    Task<OperationStatusResponseDto> DeleteAsync(VehicleDeleteRequestModel request, CancellationToken cancellationToken = default);
}
