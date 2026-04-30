namespace Be.Haven.Shared.Dtos;

public class ErrorDto
{
    /// <summary>
    /// The error code representing the type of error.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// The HTTP status code associated with the error. Ignored during JSON serialization.
    /// </summary>
    [JsonIgnore]
    public int StatusCode { get; set; }

    /// <summary>
    /// The error message describing the issue.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// A list of detailed error information.
    /// </summary>
    public List<ErrorDetailDto> Details { get; set; } = new();
}