namespace Authentication.Application.Commands.ExternalLogin;

/// <summary>
/// Represents a request to authenticate using an external identity provider token.
/// </summary>
public class ExternalLoginCommand : ICommand<ResponseDto<ExternalLoginResponseDto>>
{
    /// <summary>
    /// Gets or sets the external provider name. Supported values are <c>google</c> and <c>facebook</c>.
    /// </summary>
    public string Provider { get; set; }

    /// <summary>
    /// Gets or sets the external provider token issued to the client.
    /// </summary>
    public string ExternalToken { get; set; }
}
