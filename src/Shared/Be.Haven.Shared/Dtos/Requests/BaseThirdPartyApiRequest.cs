namespace Be.Haven.Shared.Dtos.Requests;

public class BaseThirdPartyApiRequest
{
    /// <summary>
    /// Gets or sets the name of the HTTP client used for making requests.
    /// </summary>
    public string HttpClientName { get; set; } = DEFAULT_CLIENT;

    /// <summary>
    /// The base URL for the API.
    /// </summary>
    public string BaseUrl { get; set; }

    /// <summary>
    /// The API endpoint (target URL).
    /// </summary>
    public string Endpoint { get; set; }

    /// <summary>
    /// HTTP method (GET, POST, PUT, DELETE, etc.).
    /// If not specified, you should provide a default value (e.g., POST).
    /// </summary>
    public string Method { get; set; }

    /// <summary>
    /// The request body content. This is typically serialized data, such as JSON.
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// Represents the form data to be sent with the request as key-value pairs.
    /// </summary>
    public IEnumerable<KeyValuePair<string, string>> FormData { get; set; }

    /// <summary>
    /// Represents the value of the Authorization header used in HTTP requests.
    /// </summary>
    public AuthenticationHeaderValue AuthenticationValue { get; set; }

    /// <summary>
    /// The content type of the request body.
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// Dictionary containing header key-value pairs required for the request.
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = [];

    /// <summary>
    /// Query parameters for the request (optional).
    /// </summary>
    public Dictionary<string, string> QueryParameters { get; set; }

    /// <summary>
    /// Gets or sets the file to be uploaded as part of the HTTP request.
    /// </summary>
    public IFormFile File { get; set; }
}