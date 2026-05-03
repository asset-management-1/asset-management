namespace Authentication.Application.Commands.ThirdPartyLogin;

/// <summary>
/// Represents a request to authenticate using an external identity provider token.
/// </summary>
public class ThirdPartyLoginCommand : ICommand<ResponseDto<LoginResponse>>
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
