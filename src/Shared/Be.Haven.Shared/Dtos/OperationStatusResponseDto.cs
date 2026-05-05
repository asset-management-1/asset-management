namespace Be.Haven.Shared.Dtos;

/// <summary>
/// Represents a generic operation result with explicit success state and message.
/// </summary>
public class OperationStatusResponseDto
{
    /// <summary>
    /// Gets or sets a value indicating whether the operation completed successfully.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets the human-readable operation message.
    /// </summary>
    public string Message { get; set; }
}
