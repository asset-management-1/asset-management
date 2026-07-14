namespace Haven.Application.Interfaces.Services;

/// <summary>
/// Provides tenant list, detail, create, move-out, and QR join workflows.
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// Loads tenant occupancies scoped to the current landlord party.
    /// </summary>
    /// <param name="request">The tenant list request.</param>
    /// <param name="cancellationToken">The token used to cancel the read.</param>
    /// <returns>The paged tenant response.</returns>
    Task<PaginationResponse<IReadOnlyList<TenantListItemResponseDto>>> GetTenantsAsync(
        TenantListRequestModel request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads one tenant occupancy detail scoped to the current landlord party.
    /// </summary>
    /// <param name="request">The tenant detail request.</param>
    /// <param name="cancellationToken">The token used to cancel the read.</param>
    /// <returns>The tenant detail response.</returns>
    Task<TenantDetailResponseDto> GetTenantDetailAsync(
        TenantDetailRequestModel request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a tenant or member to a room.
    /// </summary>
    /// <param name="request">The tenant create request.</param>
    /// <param name="cancellationToken">The token used to cancel tenant creation.</param>
    /// <returns>The created tenant occupancy response.</returns>
    Task<TenantDetailResponseDto> CreateTenantAsync(
        TenantCreateRequestModel request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves one tenant occupancy out of a room.
    /// </summary>
    /// <param name="request">The tenant move-out request.</param>
    /// <param name="cancellationToken">The token used to cancel the tenant move-out.</param>
    /// <returns>The operation status response.</returns>
    Task<OperationStatusResponseDto> DeleteTenantAsync(
        TenantDeleteRequestModel request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a room tenant-join QR token.
    /// </summary>
    /// <param name="request">The QR token request.</param>
    /// <param name="cancellationToken">The token used to cancel token creation.</param>
    /// <returns>The QR token response.</returns>
    Task<TenantJoinQrResponseDto> CreateTenantJoinQrAsync(
        TenantJoinQrRequestModel request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a room join preview from a QR token.
    /// </summary>
    /// <param name="request">The preview request.</param>
    /// <param name="cancellationToken">The token used to cancel the read.</param>
    /// <returns>The join preview response.</returns>
    Task<TenantJoinPreviewResponseDto> GetTenantJoinPreviewAsync(
        TenantJoinPreviewRequestModel request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirms a room join token for the current tenant party.
    /// </summary>
    /// <param name="request">The confirm request.</param>
    /// <param name="cancellationToken">The token used to cancel the pending room join.</param>
    /// <returns>The created tenant occupancy response.</returns>
    Task<TenantDetailResponseDto> ConfirmTenantJoinAsync(
        TenantJoinConfirmRequestModel request,
        CancellationToken cancellationToken = default);
}
