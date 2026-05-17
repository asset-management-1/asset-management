namespace Be.Haven.Core.Interfaces.Services;

public interface IRestClientMultipleService
{
    /// <summary>
    /// Sends a GET request using the specified BaseHttpRequest object and returns the HttpResponseMessage.
    /// </summary>
    /// <param name="request">The request object containing the URI, base address, headers, and query parameters.</param>
    /// <param name="cancellationToken">The token used to cancel the outbound request.</param>
    /// <returns>The HTTP response message resulting from the GET request.</returns>
    Task<HttpResponseMessage> GetAsync(BaseHttpRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a POST request to the specified URI with the provided request data, headers, and optional base address.
    /// </summary>
    /// <param name="request">An instance of BaseHttpRequest containing the request URI, request data, headers, content type, and optional base address.</param>
    /// <param name="cancellationToken">The token used to cancel the outbound request.</param>
    /// <returns>The HTTP response message received from the server.</returns>
    Task<HttpResponseMessage> PostAsync(BaseHttpRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a PUT request to the specified URI with the provided request data.
    /// </summary>
    /// <param name="request">Contains the request details including URI, data, headers, base address, and content type.</param>
    /// <param name="cancellationToken">The token used to cancel the outbound request.</param>
    /// <returns>The HTTP response message received from the server.</returns>
    Task<HttpResponseMessage> PutAsync(BaseHttpRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a DELETE request to the specified URI with optional headers and query parameters.
    /// </summary>
    /// <param name="request"> An object containing details of the request, such as the URI, base address, headers, and query parameters. </param>
    /// <param name="cancellationToken">The token used to cancel the outbound request.</param>
    /// <returns> The HTTP response message received from the DELETE request. </returns>
    Task<HttpResponseMessage> DeleteAsync(BaseHttpRequest request, CancellationToken cancellationToken = default);
}
