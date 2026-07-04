namespace Haven.Application.Interfaces.Services;

/// <summary>
/// Provides landlord property list, detail, and creation workflows.
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
    /// Creates a property with generated unit structure and optional charge policies.
    /// </summary>
    /// <param name="request">The party-scoped creation request.</param>
    /// <param name="cancellationToken">The token used to cancel the write.</param>
    /// <returns>The created property summary.</returns>
    Task<CreatedPropertyResponseDto> CreatePropertyAsync(
        PropertyCreationRequestModel request,
        CancellationToken cancellationToken = default);
}
