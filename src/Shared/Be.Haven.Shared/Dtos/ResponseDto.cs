namespace Be.Haven.Shared.Dtos;

/// <summary>
/// Represents a response with a generic data type.
/// </summary>
/// <typeparam name="T">The type of the data.</typeparam>
public class ResponseDto<T>
{
    private T _data;
    private ErrorDto _error;

    /// <summary>
    /// Indicates whether the response is successful (no error).
    /// </summary>
    public bool Success { get; private set; }

    /// <summary>
    /// Gets or sets the data included in the response.
    /// </summary>
    public T Data
    {
        get => _data;
        set
        {
            _data = value;
            UpdateState();
        }
    }

    /// <summary>
    /// Gets or sets the error information for the response.
    /// </summary>
    public ErrorDto Error
    {
        get => _error;
        set
        {
            _error = value;
            UpdateState();
        }
    }

    /// <summary>
    /// Gets or sets the metadata associated with the response.
    /// </summary>
    public MetaDetailDto Meta { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResponseDto{T}"/> class.
    /// </summary>
    public ResponseDto()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResponseDto{T}"/> class with the specified data and optional metadata.
    /// </summary>
    /// <param name="data">The data to include in the response.</param>
    public ResponseDto(T data)
    {
        Data = data;
        UpdateState();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResponseDto{T}"/> class with the specified error code, message, details, and optional metadata.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="details">Optional list of error details.</param>
    /// <param name="meta">Optional metadata associated with the response.</param>
    public ResponseDto(
        string code,
        string message,
        List<ErrorDetailDto> details = null,
        MetaDetailDto meta = null)
    {
        Data = default;
        Error = new ErrorDto
        {
            Code = code,
            Message = message,
            Details = details
        };
        Meta = meta ?? new MetaDetailDto();
        UpdateState();
    }

    /// <summary>
    /// Updates the success state based on the presence of an error.
    /// </summary>
    private void UpdateState()
    {
        Success = Error == null;
    }
}