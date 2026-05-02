using Authentication.Application.Commands.ForgotPassword;
using Authentication.Application.Commands.Register;
using Authentication.Application.Commands.ThirdPartyLogin;

namespace Authentication.Application.Interfaces;

public interface IAuthenticationService
{
    /// <summary>
    /// Authenticates a user by username and password, then issues access and refresh tokens.
    /// </summary>
    Task<ResponseDto<LoginResponse>> LoginAsync(string userName, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a new user account and issues access and refresh tokens.
    /// </summary>
    Task<ResponseDto<LoginResponse>> RegisterAsync(RegisterCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Issues a new access and refresh token pair from a valid refresh token.
    /// </summary>
    Task<ResponseDto<LoginResponse>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes refresh token(s) for the current user as part of logout.
    /// </summary>
    Task<ResponseDto<string>> LogoutAsync(long userId, string refreshToken, bool logoutAllSessions, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates a user through a third-party provider and issues tokens.
    /// </summary>
    Task<ResponseDto<LoginResponse>> LoginByThirdPartyAsync(ThirdPartyLoginCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Handles forgot-password flow by resetting the account password.
    /// </summary>
    Task<ResponseDto<string>> ForgotPasswordAsync(ForgotPasswordCommand command, CancellationToken cancellationToken = default);
}
