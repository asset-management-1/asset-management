namespace Haven.Application.Interfaces.Services;

/// <summary>
/// Provides landlord property list, detail, creation, update, and delete workflows.
/// </summary>
public interface IPropertyService
{
    /// <summary>
    /// Loads paged properties scoped to the current party.
    /// </summary>
    /// <param name="request">The party-scoped list request.</param>
    /// <param name="cancellationToken">The token used to cancel the read.</param>
    /// <returns>The paged property response.</returns>
    Task<PaginationResponse<IReadOnlyList<PropertyListItemResponseDto>>> GetPropertiesAsync(
        PropertyListRequestModel request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads one property detail scoped to the current party.
    /// </summary>
    /// <param name="request">The party-scoped detail request.</param>
    /// <param name="cancellationToken">The token used to cancel the read.</param>
    /// <returns>The property detail response.</returns>
    Task<PropertyDetailResponseDto> GetPropertyDetailAsync(
        PropertyDetailRequestModel request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a property from the final frontend-submitted unit structure and optional charge policies.
    /// </summary>
    /// <param name="request">The party-scoped creation request.</param>
    /// <param name="cancellationToken">The token used to cancel property creation.</param>
    /// <returns>The created property summary.</returns>
    Task<CreatedPropertyResponseDto> CreatePropertyAsync(
        PropertyCreationRequestModel request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a property edit form and returns refreshed property detail.
    /// </summary>
    /// <param name="request">The party-scoped update request.</param>
    /// <param name="cancellationToken">The token used to cancel the property mutation.</param>
    /// <returns>The refreshed property detail response.</returns>
    Task<PropertyDetailResponseDto> UpdatePropertyAsync(
        PropertyUpdateRequestModel request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes a property when it has no protected dependencies.
    /// </summary>
    /// <param name="request">The party-scoped delete request.</param>
    /// <param name="cancellationToken">The token used to cancel the delete scheduling.</param>
    /// <returns>The operation status response.</returns>
    Task<OperationStatusResponseDto> DeletePropertyAsync(
        PropertyDeleteRequestModel request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Restores a pending property delete request.
    /// </summary>
    /// <param name="request">The party-scoped restore-delete request.</param>
    /// <param name="cancellationToken">The token used to cancel the delete restoration.</param>
    /// <returns>The operation status response.</returns>
    Task<OperationStatusResponseDto> RestorePropertyDeleteAsync(
        PropertyDeleteRequestModel request,
        CancellationToken cancellationToken = default);
}
