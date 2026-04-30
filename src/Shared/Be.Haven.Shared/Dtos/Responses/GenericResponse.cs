using Be.Haven.Shared.Constants;

namespace Be.Haven.Shared.Dtos.Responses;

/// <summary>
/// The object represent success or not of operation, contains the data if available
/// </summary>
/// <typeparam name="T">The type of data this instance contains</typeparam>
public class GenericResponse<T>
{
    private string _errorMessage;
    private int _errorCode;
    private T _data;

    /// <summary>
    /// Default constructor
    /// </summary>
    public GenericResponse()
    {
    }

    /// <summary>
    /// Constructor to initialize with Data
    /// </summary>
    /// <param name="data">The data to initialize</param>
    public GenericResponse(T data) => Data = data;

    /// <summary>
    /// Constructor to initialize with error (failure case)
    /// </summary>
    /// <param name="errorMessage">The error message</param>
    /// <param name="errorCode">The error code</param>
    public GenericResponse(
        string errorMessage,
        int errorCode = AppConstants.SystemCode.BAD_REQUEST)
    {
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Represents data
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
    /// Indicates if the operation was successful
    /// </summary>
    public bool IsSuccess { get; private set; }

    /// <summary>
    /// Indicates if the operation was a failure
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Represents the error message
    /// </summary>
    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            UpdateState();
        }
    }

    /// <summary>
    /// Represents the error code
    /// </summary>
    public int ErrorCode
    {
        get => _errorCode;
        set
        {
            _errorCode = value;
            UpdateState();
        }
    }

    /// <summary>
    /// Updates the IsSuccess state based on ErrorMessage and ErrorCode
    /// </summary>
    private void UpdateState()
    {
        IsSuccess = string.IsNullOrEmpty(ErrorMessage) && ErrorCode <= 0;
    }
}