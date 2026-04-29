namespace Haven.Core.Interfaces.Services;

public interface IThirdPartyApiService
{
    /// <summary>
    /// Sends a request to a third-party API, handles the response, and (optionally)
    /// validates its status code.
    /// </summary>
    /// <param name="request">
    /// The request object containing the details for communicating with the third-party API,
    /// including method, headers, and query parameters.
    /// </param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response content as a string.</returns>
    Task<string> HandleApiData<TRquest>(TRquest request) where TRquest : BaseThirdPartyApiRequest;

    /// <summary>
    /// Sends a request to a third-party API and handles the response by validating the status and deserializing the content into the specified type.
    /// </summary>
    /// <param name="request">The API request object, containing details such as base URL, endpoint, HTTP method, headers, query parameters, and content for the third-party API call.</param>
    /// <typeparam name="TRquest">The type of the request, which must inherit from <see cref="BaseThirdPartyApiRequest"/>.</typeparam>
    /// <typeparam name="TResponse">The type into which the response content will be deserialized.</typeparam>
    /// <returns>A task representing the asynchronous operation, with the result containing the deserialized response object.</returns>
    /// <exception cref="HttpStatusCodeException">
    /// Thrown when the API response status code is not successful. Logs the status code, reason phrase, and response content for error tracking.
    /// </exception>
    Task<TResponse> HandleApiData<TRquest, TResponse>(TRquest request) where TRquest : BaseThirdPartyApiRequest;

    /// <summary>
    /// Handles an HTTP request dynamically by adapting the provided request information and executing the associated HTTP method.
    /// </summary>
    /// <param name="request">
    /// The request object containing the necessary details for interacting with the API,
    /// such as HTTP method, endpoint, headers, content, and query parameters.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the HTTP response message.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when an unsupported or invalid HTTP method is provided in the request.
    /// </exception>
    Task<HttpResponseMessage> HandleDynamicHttpRequest(BaseThirdPartyApiRequest request);
}