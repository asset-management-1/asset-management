namespace Authentication.Application.Commands.ExternalLink;

/// <summary>
/// Represents a request to link an external provider to the current user.
/// </summary>
public class LinkExternalProviderCommand : ICommand<ResponseDto<OperationStatusResponseDto>>
{
    /// <summary>
    /// External provider name. Supported values depend on configuration, such as <c>google</c>, <c>apple</c>, <c>microsoft</c>, or <c>facebook</c>.
    /// </summary>
    public string Provider { get; set; }

    /// <summary>
    /// External provider token issued to the client.
    /// </summary>
    public string ExternalToken { get; set; }
}
