namespace Authentication.Application.Dtos.Users.ExternalProviders;

/// <summary>
/// Represents a linked external provider in the user profile response.
/// </summary>
public class ExternalProviderResponseDto
{
    /// <summary>
    /// External provider name.
    /// </summary>
    public string Provider { get; set; }

    /// <summary>
    /// Indicates whether the provider is currently linked.
    /// </summary>
    public bool IsLinked { get; set; }
}
