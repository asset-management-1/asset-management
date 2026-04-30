namespace Be.Haven.Shared.Dtos.Requests;

public class BaseHttpRequest
{
    /// <summary>
    /// Gets or sets the name of the HTTP client used for making requests.
    /// </summary>
    public string HttpClientName { get; set; } = DEFAULT_CLIENT;

    /// <summary>
    /// Gets or sets the URI of the HTTP request.
    /// </summary>
    public string RequestUri { get; set; }

    /// <summary>
    /// Gets or sets the data payload to be sent with the HTTP request.
    /// </summary>
    public string RequestData { get; set; }

    /// <summary>
    /// Gets or sets the form data of the HTTP request as a collection of key-value pairs.
    /// </summary>
    public IEnumerable<KeyValuePair<string, string>> RequestFormData { get; set; }

    /// <summary>
    /// Represents the value of the Authorization header used in HTTP requests.
    /// </summary>
    public AuthenticationHeaderValue AuthenticationValue { get; set; }

    /// <summary>
    /// Gets or sets the base address of the HTTP request.
    /// </summary>
    public string BaseAddress { get; set; }

    /// <summary>
    /// Gets or sets the content type of the HTTP request.
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// Gets or sets a collection of additional headers to include in the HTTP request.
    /// </summary>
    public Dictionary<string, string> AdditionalHeaders { get; set; }

    /// <summary>
    /// Gets or sets the collection of additional query parameters to be included in the HTTP request.
    /// </summary>
    public Dictionary<string, string> AdditionalQueryParams { get; set; }

    /// <summary>
    /// Gets or sets the file to be uploaded as part of the HTTP request.
    /// </summary>
    public IFormFile File { get; set; }
}