namespace Authentication.Application.Dtos.Users.ExternalProviders;

/// <summary>
/// Represents a single string value projected from a read-model query result.
/// </summary>
public class StringValueResponseDto
{
    /// <summary>
    /// Gets or sets the projected string value.
    /// </summary>
    public string Value { get; set; }
}
