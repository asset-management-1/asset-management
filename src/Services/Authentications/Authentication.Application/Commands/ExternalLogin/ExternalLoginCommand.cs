namespace Authentication.Application.Commands.ExternalLogin;

/// <summary>
/// Represents a request to authenticate using an external identity provider token.
/// </summary>
public class ExternalLoginCommand : ICommand<ResponseDto<LoginResponseDto>>
{
    /// <summary>
    /// Gets or sets the external provider name. Supported values depend on configuration, such as <c>google</c>, <c>apple</c>, <c>microsoft</c>, or <c>facebook</c>.
    /// </summary>
    public string Provider { get; set; }

    /// <summary>
    /// Gets or sets the external provider token issued to the client.
    /// </summary>
    public string ExternalToken { get; set; }
}
