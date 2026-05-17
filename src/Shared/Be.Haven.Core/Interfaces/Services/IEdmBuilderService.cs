namespace Be.Haven.Core.Interfaces.Services;

public interface IEdmBuilderService
{
    /// <summary>
    /// Parses and validates OData query parameters from the incoming HTTP request.
    /// It uses the provided entity set name to locate the appropriate EDM entity,
    /// and builds a strongly-typed <see cref="ODataQueryOptions"/> object.
    /// </summary>
    /// <typeparam name="T">The CLR type corresponding to the OData entity.</typeparam>
    /// <param name="req">The incoming HTTP request containing OData query parameters.</param>
    /// <param name="entitySetName">The name of the entity set defined in the EDM model.</param>
    /// <returns>Validated and parsed OData query options for the specified type.</returns>
    ODataQueryOptions<T> ConvertToQueryOptions<T>(HttpRequest req, string entitySetName);

    /// <summary>
    /// Parses and validates the OData query from an HTTP request and returns the raw query string.
    /// Useful for rebuilding the full OData-compliant URI.
    /// </summary>
    /// <param name="req">The incoming HTTP request containing OData parameters.</param>
    /// <param name="entitySetName">The EDM entity set name to validate against.</param>
    /// <returns>The raw OData query string extracted from the request.</returns>
    string ConvertToQueryString(HttpRequest req, string entitySetName);

    /// <summary>
    /// Creates an <see cref="HttpRequest"/> object from a full OData query URL string.
    /// This can be used to simulate an incoming request when the OData query is stored externally.
    /// </summary>
    /// <param name="odataUrl">The full OData query URL.</param>
    /// <returns>An <see cref="HttpRequest"/> object representing the URL.</returns>
    HttpRequest CreateHttpRequestFromUri(string odataUrl);
}
