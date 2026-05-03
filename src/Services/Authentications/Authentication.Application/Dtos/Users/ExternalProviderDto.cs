namespace Authentication.Application.Dtos.Users;

/// <summary>
/// Represents a linked external provider in the user profile response.
/// </summary>
public class ExternalProviderDto
{
    /// <summary>
    /// External provider name.
    /// </summary>
    public string Provider { get; set; }
}
