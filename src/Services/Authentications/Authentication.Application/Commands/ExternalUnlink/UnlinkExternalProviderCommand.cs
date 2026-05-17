namespace Authentication.Application.Commands.ExternalUnlink;

/// <summary>
/// Represents a request to unlink an external provider from the current user.
/// </summary>
public class UnlinkExternalProviderCommand : ICommand<ResponseDto<OperationStatusResponseDto>>
{
    /// <summary>
    /// External provider name. Supported values depend on configuration, such as <c>google</c>, <c>apple</c>, <c>microsoft</c>, or <c>facebook</c>.
    /// </summary>
    public string Provider { get; set; }
}
