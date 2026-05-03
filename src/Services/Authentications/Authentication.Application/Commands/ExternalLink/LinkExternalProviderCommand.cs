namespace Authentication.Application.Commands.ExternalLink;

/// <summary>
/// Represents a request to link an external provider to the current user.
/// </summary>
public class LinkExternalProviderCommand : ICommand<ResponseDto<string>>
{
    /// <summary>
    /// External provider name.
    /// </summary>
    public string Provider { get; set; }

    /// <summary>
    /// External provider token issued to the client.
    /// </summary>
    public string ExternalToken { get; set; }
}
