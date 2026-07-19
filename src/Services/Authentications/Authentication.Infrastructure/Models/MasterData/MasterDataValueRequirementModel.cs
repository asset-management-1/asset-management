namespace Authentication.Infrastructure.Models.MasterData;

/// <summary>
/// Groups the key and error metadata required to resolve one mandatory master-data value.
/// </summary>
internal sealed class MasterDataValueRequirementModel
{
    /// <summary>
    /// Initialises a new instance of the <see cref="MasterDataValueRequirementModel"/> class.
    /// </summary>
    /// <param name="type">The master-data type code or name.</param>
    /// <param name="value">The master-data value code or name.</param>
    /// <param name="message">The error message used when the value cannot be resolved.</param>
    /// <param name="errorCode">The application error code used for business-flow failures.</param>
    /// <param name="statusCode">The HTTP status code used when no application error code is supplied.</param>
    public MasterDataValueRequirementModel(
        string type,
        string value,
        string message,
        string errorCode = null,
        int statusCode = StatusCodes.Status500InternalServerError)
    {
        // Store lookup failure metadata so the helper can preserve business vs configuration errors.
        Type = type;
        Value = value;
        Message = message;
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }

    /// <summary>
    /// Gets the master-data type code or name.
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Gets the master-data value code or name.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets the error message used when the value cannot be resolved.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the application error code used for business-flow failures.
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Gets the HTTP status code used when no application error code is supplied.
    /// </summary>
    public int StatusCode { get; }
}
