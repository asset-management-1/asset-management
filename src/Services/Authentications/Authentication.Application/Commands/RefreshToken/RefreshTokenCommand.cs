namespace Authentication.Application.Commands.RefreshToken;

public class RefreshTokenCommand : ICommand<ResponseDto<LoginResponse>>
{
    /// <summary>
    /// Refresh token used to issue a new token pair.
    /// </summary>
    public string RefreshToken { get; set; }
}
