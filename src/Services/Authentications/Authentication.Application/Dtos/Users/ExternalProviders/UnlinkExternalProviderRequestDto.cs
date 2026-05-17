namespace Authentication.Application.Dtos.Users.ExternalProviders;

/// <summary>
/// Represents the external provider the current user wants to unlink.
/// </summary>
public class UnlinkExternalProviderRequestDto
{
    /// <summary>
    /// External provider name.
    /// </summary>
    public string Provider { get; set; }
}
