namespace Authentication.Application.Commands.RefreshToken;

/// <summary>
/// Represents a request to exchange a refresh token for a new token pair.
/// </summary>
public class RefreshTokenCommand : ICommand<ResponseDto<LoginResponseDto>>
{
    /// <summary>
    /// Refresh token used to issue a new token pair.
    /// </summary>
    public string RefreshToken { get; set; }
}
