namespace Be.Haven.Core.Services;

public class RestClientMultipleService : IRestClientMultipleService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<RestClientMultipleService> _logger;

    /// <summary>
    /// Service for making dynamic REST API calls (GET, POST, PUT, DELETE) utilizing an HTTP client factory.
    /// </summary>
    public RestClientMultipleService(
        IHttpClientFactory httpClientFactory,
        ILogger<RestClientMultipleService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Sends a GET request using the specified BaseHttpRequest object and returns the HttpResponseMessage.
    /// </summary>
    /// <param name="request">The request object containing the URI, base address, headers, and query parameters.</param>
    /// <param name="cancellationToken">The token used to cancel the outbound request.</param>
    /// <returns>The HTTP response message resulting from the GET request.</returns>
    public async Task<HttpResponseMessage> GetAsync(
        BaseHttpRequest request,
        CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient(request.HttpClientName);

        // Set the base address if provided
        SetBaseAddress(httpClient, request.BaseAddress);

        // Set the authentication header if provided
        if (request.AuthenticationValue != null)
        {
            httpClient.DefaultRequestHeaders.Authorization = request.AuthenticationValue;
        }

        // Append query parameters to the request URI
        request.RequestUri = AppendQueryParameters(request.RequestUri, request.AdditionalQueryParams);

        // Perform the GET request
        AddHeaders(httpClient, request.AdditionalHeaders);
        var httpResponseMessage = await httpClient.GetAsync(request.RequestUri, cancellationToken);
        return httpResponseMessage;
    }

    /// <summary>
    /// Sends a POST request to the specified URI with the provided request data and headers.
    /// </summary>
    /// <param name="request"> An instance of BaseHttpRequest containing the request data, headers, and optional base address. </param>
    /// <param name="cancellationToken">The token used to cancel the outbound request.</param>
    /// <returns> The HTTP response message received from the server. </returns>
    public async Task<HttpResponseMessage> PostAsync(
        BaseHttpRequest request,
        CancellationToken cancellationToken = default)
    {
        // Set the timeout for the request
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(TIME_RUN_REQUEST));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);
        
        try
        {
            var httpClient = _httpClientFactory.CreateClient(request.HttpClientName);

            // Set the base address if provided
            SetBaseAddress(httpClient, request.BaseAddress);

            // Set the authentication header if provided
            if (request.AuthenticationValue != null)
            {
                httpClient.DefaultRequestHeaders.Authorization = request.AuthenticationValue;
            }

            // Perform the POST request
            AddHeaders(httpClient, request.AdditionalHeaders);

            HttpResponseMessage httpResponseMessage;
            if (request.ContentType == APPLICATION_FORM_URLENCODED)
            {
                // Send the request with form data
                httpResponseMessage = await httpClient.PostAsync(
                    request.RequestUri,
                    new FormUrlEncodedContent(request.RequestFormData), linkedCts.Token);
            }
            else if (request.ContentType == MULTIPART_FORM_DATA)
            {
                using var form = new MultipartFormDataContent();

                // Add file
                AddFile(form, request);

                httpResponseMessage = await httpClient.PostAsync(
                    request.RequestUri,
                    form, linkedCts.Token
                );
            }
            else
            {
                // Create the HTTP content for the request.
                using var httpContent = CreateHttpContent(request);

                // Send the request
                httpResponseMessage = await httpClient.PostAsync(
                    request.RequestUri,
                    httpContent, linkedCts.Token);
            }

            return httpResponseMessage;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(
                ex,
                LOG_HTTP_REQUEST_TIMEOUT,
                request.HttpClientName,
                HttpMethod.Post.Method,
                request.RequestUri,
                TIME_RUN_REQUEST,
                cts.IsCancellationRequested);

            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.GatewayTimeout,
                Content = new StringContent(string.Format(ERROR_REQUEST_TIMEOUT, TIME_RUN_REQUEST,  ex.Message))
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                LOG_HTTP_REQUEST_FAILED,
                request.HttpClientName,
                HttpMethod.Post.Method,
                request.RequestUri);

            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.GatewayTimeout,
                Content = new StringContent(ex.Message)
            };
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogError(
                ex,
                LOG_HTTP_CIRCUIT_OPEN,
                request.HttpClientName,
                HttpMethod.Post.Method,
                request.RequestUri);
            
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.ServiceUnavailable,
                Content = new StringContent(ex.Message)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                LOG_HTTP_UNEXPECTED_ERROR,
                request.HttpClientName,
                HttpMethod.Post.Method,
                request.RequestUri);
            
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent(ex.Message)
            };
        }
    }

    /// <summary>
    /// Sends a PUT request to the specified URI with the provided request data.
    /// </summary>
    /// <param name="request">Contains the request details including URI, data, headers, base address, and content type.</param>
    /// <param name="cancellationToken">The token used to cancel the outbound request.</param>
    /// <returns>The HTTP response message received from the server.</returns>
    public async Task<HttpResponseMessage> PutAsync(
        BaseHttpRequest request,
        CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient(request.HttpClientName);

        // Set the base address if provided
        SetBaseAddress(httpClient, request.BaseAddress);

        // Set the authentication header if provided
        if (request.AuthenticationValue != null)
        {
            httpClient.DefaultRequestHeaders.Authorization = request.AuthenticationValue;
        }

        // Perform the GET request
        AddHeaders(httpClient, request.AdditionalHeaders);

        // Create the HTTP content for the request
        using var httpContent = CreateHttpContent(request);
        
        // Set the timeout for the request
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(TIME_RUN_REQUEST));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);

        try
        {
            // Send the request
            var httpResponseMessage = await httpClient.PutAsync(request.RequestUri, httpContent, linkedCts.Token);

            return httpResponseMessage;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(
                ex,
                LOG_HTTP_REQUEST_TIMEOUT,
                request.HttpClientName,
                HttpMethod.Put.Method,
                request.RequestUri,
                TIME_RUN_REQUEST,
                cts.IsCancellationRequested);

            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.GatewayTimeout,
                Content = new StringContent(string.Format(ERROR_REQUEST_TIMEOUT, TIME_RUN_REQUEST,  ex.Message))
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                LOG_HTTP_REQUEST_FAILED,
                request.HttpClientName,
                HttpMethod.Put.Method,
                request.RequestUri);
            
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.GatewayTimeout,
                Content = new StringContent(ex.Message)
            };
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogError(
                ex,
                LOG_HTTP_CIRCUIT_OPEN,
                request.HttpClientName,
                HttpMethod.Put.Method,
                request.RequestUri);
            
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.ServiceUnavailable,
                Content = new StringContent(ex.Message)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                LOG_HTTP_UNEXPECTED_ERROR,
                request.HttpClientName,
                HttpMethod.Put.Method,
                request.RequestUri);
            
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent(ex.Message)
            };
        }
    }

    /// <summary>
    /// Sends a DELETE request to the specified URI with optional headers and query parameters.
    /// </summary>
    /// <param name="request"> An object containing details of the request, such as the URI, base address, headers, and query parameters. </param>
    /// <param name="cancellationToken">The token used to cancel the outbound request.</param>
    /// <returns> The HTTP response message received from the DELETE request. </returns>
    public async Task<HttpResponseMessage> DeleteAsync(
        BaseHttpRequest request,
        CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient(request.HttpClientName);

        // Set the base address if provided
        SetBaseAddress(httpClient, request.BaseAddress);

        // Set the authentication header if provided
        if (request.AuthenticationValue != null)
        {
            httpClient.DefaultRequestHeaders.Authorization = request.AuthenticationValue;
        }

        // Append query parameters to the request URI
        request.RequestUri = AppendQueryParameters(request.RequestUri, request.AdditionalQueryParams);

        // Perform the GET request
        AddHeaders(httpClient, request.AdditionalHeaders);
        var httpResponseMessage = await httpClient.DeleteAsync(request.RequestUri, cancellationToken);
        return httpResponseMessage;
    }

    #region Private Methods
    /// <summary>
    /// Adds the specified headers to the provided HttpClient and optionally to the HttpContent object.
    /// </summary>
    /// <param name="httpClient">The HttpClient instance to which the headers will be added.</param>
    /// <param name="additionalHeaders">The dictionary of headers to add, where the key is the header name and the value is the header value.</param>
    private static void AddHeaders(
        HttpClient httpClient,
        Dictionary<string, string> additionalHeaders)
    {
        if (additionalHeaders is not { Count: > 0 })
        {
            return;
        }

        foreach (var (key, value) in additionalHeaders)
        {
            // Add general headers if not already present
            if (httpClient.DefaultRequestHeaders.Contains(key))
            {
                httpClient.DefaultRequestHeaders.Remove(key);
            }

            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(key, value);
        }
    }

    /// <summary>
    /// Adds a file to the multipart/form-data content if the request contains a file.
    /// </summary>
    /// <param name="form">
    /// The <see cref="MultipartFormDataContent"/> object that will hold the uploaded file.
    /// </param>
    /// <param name="request">
    /// The base HTTP request containing the uploaded file information.
    /// </param>
    private static void AddFile(MultipartFormDataContent form, BaseHttpRequest request)
    {
        // If no file is provided, skip adding anything to the multipart form
        if (request.File is null)
            return;

        // Open the file stream for reading
        var stream = request.File.OpenReadStream();

        // Wrap the stream in HttpContent so it can be uploaded
        var fileContent = new StreamContent(stream);

        // Set the Content-Type (MIME type) from the uploaded file
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(request.File.ContentType);

        // Add the file to the multipart form with the specified field name and original file name
        form.Add(fileContent, FILE_NAME, request.File.FileName);
    }

    /// <summary>
    /// Creates a <see cref="HttpContent"/> instance using the provided request payload and content type.
    /// </summary>
    /// <param name="request">The request containing either a string body or a raw stream body.</param>
    /// <returns>
    /// A <see cref="HttpContent"/> object with the specified content and appropriate headers.
    /// </returns>
    private static HttpContent CreateHttpContent(BaseHttpRequest request)
    {
        // Prefer raw stream content for object-storage uploads and other non-JSON payloads.
        if (request.RequestStream is not null)
        {
            var streamContent = new StreamContent(request.RequestStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(request.ContentType);

            return streamContent;
        }

        // Fall back to string content for JSON/XML style integrations.
        var content = new StringContent(request.RequestData, UTF8);

        var mediaType = request.ContentType switch
        {
            TEXT_XML => TEXT_XML,
            _ => TEXT_JSON
        };

        content.Headers.ContentType = new MediaTypeHeaderValue(mediaType)
        {
            CharSet = UTF8.WebName
        };

        return content;
    }


    /// <summary>
    /// Appends query parameters to the provided base URI to create a full URL with query string.
    /// </summary>
    /// <param name="baseUri"> The base URI to which the query parameters should be appended. </param>
    /// <param name="queryParams"> A dictionary of query parameters, where each key-value pair represents a parameter name and value. </param>
    /// <returns> The full URI containing the base URI and the appended query string. </returns>
    private static string AppendQueryParameters(
        string baseUri,
        Dictionary<string, string> queryParams)
    {
        // Return the original URI when the caller has no query data to append.
        if (queryParams is null || queryParams.Count == 0)
        {
            return baseUri;
        }

        // Encode each key/value pair before joining the final query string.
        var queryString = string.Join("&", queryParams.Select(kvp =>
            $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        return $"{baseUri}?{queryString}";
    }

    /// <summary>
    /// Sets the base address for the specified HttpClient if a valid base address is provided.
    /// </summary>
    /// <param name="httpClient">The HttpClient instance on which the base address will be set.</param>
    /// <param name="baseAddress">The base address to set. If null or empty, no action is taken.</param>
    private static void SetBaseAddress(HttpClient httpClient, string baseAddress)
    {
        // Skip base address configuration for clients that already resolve absolute request URIs.
        if (string.IsNullOrWhiteSpace(baseAddress))
        {
            return;
        }

        // Normalize the base address so relative request paths combine correctly.
        if (!baseAddress.EndsWith('/'))
        {
            baseAddress += "/";
        }

        httpClient.BaseAddress = new Uri(baseAddress);
    }
    #endregion
}
