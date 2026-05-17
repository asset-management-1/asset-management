namespace Be.Haven.Shared.Dtos;

/// <summary>
/// Represents field-level error details for validation and business-rule failures.
/// </summary>
public class ErrorDetailDto
{
    /// <summary>
    /// The name of the field where the error occurred.
    /// </summary>
    public string Field { get; set; }

    /// <summary>
    /// The issue or validation error related to the field.
    /// </summary>
    public object Issue { get; set; }
}
