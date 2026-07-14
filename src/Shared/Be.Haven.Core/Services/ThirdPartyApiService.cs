namespace Be.Haven.Core.Services;

public class ThirdPartyApiService : IThirdPartyApiService
{
    private readonly IRestClientMultipleService _restClientMultipleService;
    private readonly ILogger<ThirdPartyApiService> _logger;
    private readonly IJsonSerializerService _jsonSerializerService;

    /// <summary>
    /// Creates the shared third-party API service with REST client access, structured logging, and JSON serialization.
    /// </summary>
    /// <param name="restClientMultipleService">Service for making REST API calls.</param>
    /// <param name="logger">Logger for tracking and recording service operations.</param>
    /// <param name="jsonSerializerService">Service for JSON serialization and deserialization.</param>
    public ThirdPartyApiService(
        IRestClientMultipleService restClientMultipleService,
        ILogger<ThirdPartyApiService> logger,
        IJsonSerializerService jsonSerializerService)
    {
        _restClientMultipleService = restClientMultipleService;
        _logger = logger;
        _jsonSerializerService = jsonSerializerService;
    }

    /// <summary>
    /// Sends a request to a third-party API, handles the response, and (optionally)
    /// validates its status code.
    /// </summary>
    /// <param name="request">
    /// The request object containing the details for communicating with the third-party API,
    /// including method, headers, and query parameters.
    /// </param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response content as a string.</returns>
    public virtual async Task<string> HandleApiData<TRquest>(TRquest request)
        where TRquest : BaseThirdPartyApiRequest
    {
        var requestJson = _jsonSerializerService.Serialize(request);
        var maskedRequestJson = LogMaskingHelper.MaskAllSensitiveData(requestJson);

        _logger.LogInformation(
            LOG_START_THIRD_PARTY_CALL,
            typeof(TRquest).Name,
            maskedRequestJson);

        // Handle the request
        var response = await HandleDynamicHttpRequest(request);

        // Parse the response content
        var content = await response.Content.ReadAsStringAsync();
        var maskedContent = LogMaskingHelper.MaskAllSensitiveData(content);

        // Validate the response only when the flag is enabled
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                LOG_PROCESS_HANDLE_API_RESPONSE_ERROR,
                (int)response.StatusCode,
                response.ReasonPhrase,
                maskedContent);

            throw new HttpStatusCodeException(
                string.Format(PROCESS_HANDLE_API_RESPONSE_ERROR,
                    response.StatusCode, response.ReasonPhrase, content), (int)response.StatusCode
            );
        }

        _logger.LogInformation(
            LOG_END_THIRD_PARTY_CALL,
            typeof(TRquest).Name,
            (int)response.StatusCode,
            maskedContent);

        return content;
    }

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
    public virtual async Task<TResponse> HandleApiData<TRquest, TResponse>(TRquest request) where TRquest : BaseThirdPartyApiRequest
    {
        var requestJson = _jsonSerializerService.Serialize(request);
        var maskedRequestJson = LogMaskingHelper.MaskAllSensitiveData(requestJson);

        _logger.LogInformation(
            LOG_START_THIRD_PARTY_CALL,
            typeof(TRquest).Name,
            maskedRequestJson);

        // Handle the request
        var response = await HandleDynamicHttpRequest(request);

        // Parse the response content
        var content = await response.Content.ReadAsStringAsync();
        var maskedContent = LogMaskingHelper.MaskAllSensitiveData(content);

        // Validate the response
        if (!response.IsSuccessStatusCode)
        {
            // Throw an exception if the response is not successful
            _logger.LogError(
                LOG_PROCESS_HANDLE_API_RESPONSE_ERROR,
                response.StatusCode,
                response.ReasonPhrase,
                maskedContent);
            throw new HttpStatusCodeException(
                string.Format(PROCESS_HANDLE_API_RESPONSE_ERROR,
                    response.StatusCode, response.ReasonPhrase, content), (int)response.StatusCode
            );
        }

        _logger.LogInformation(
            LOG_END_THIRD_PARTY_CALL,
            typeof(TRquest).Name,
            (int)response.StatusCode,
            maskedContent);

        // Deserialize the response content to the specified type
        var responseData = _jsonSerializerService.Deserialize<TResponse>(content);

        return responseData;
    }

    /// <summary>
    /// Handles an HTTP request dynamically by adapting the provided request data and executing the corresponding HTTP method.
    /// </summary>
    /// <param name="request">The request object containing the details for communicating with the API, including method, endpoint, headers, and query parameters.</param>
    /// <param name="cancellationToken">The token used to cancel the outbound request.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message.</returns>
    /// <exception cref="ArgumentException">Thrown when an unsupported HTTP method is provided in the request.</exception>
    public async Task<HttpResponseMessage> HandleDynamicHttpRequest(
        BaseThirdPartyApiRequest request,
        CancellationToken cancellationToken = default)
    {
        // Adapt the request to the BaseHttpRequest model
        var requestData = request.Adapt<BaseHttpRequest>();

        if (request.File is not null)
        {
            requestData.File = request.File;
        }

        if (request.ContentStream is not null)
        {
            // Streams are pass-through payloads and should not be cloned by object mapping.
            requestData.RequestStream = request.ContentStream;
        }

        // Validate the request
        return request.Method.ToLower() switch
        {
            GET => await _restClientMultipleService.GetAsync(requestData, cancellationToken),

            POST => await _restClientMultipleService.PostAsync(requestData, cancellationToken),

            PUT => await _restClientMultipleService.PutAsync(requestData, cancellationToken),

            DELETE => await _restClientMultipleService.DeleteAsync(requestData, cancellationToken),

            _ => throw new ArgumentException(HTTP_METHOD_NOT_SUPPORTED)
        };
    }
}
