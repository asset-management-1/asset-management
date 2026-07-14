namespace Haven.Application.Interfaces.Services;

/// <summary>
/// Resolves province, district, and ward location values for Haven business workflows.
/// </summary>
public interface ILocationService
{
    /// <summary>
    /// Gets the hierarchical location context for supplied location codes.
    /// </summary>
    /// <param name="key">The requested location codes.</param>
    /// <param name="cancellationToken">The token used to cancel the lookup.</param>
    /// <returns>The resolved location context.</returns>
    Task<PropertyLocationContextModel> GetLocationsAsync(LocationKeyModel key, CancellationToken cancellationToken = default);
}
