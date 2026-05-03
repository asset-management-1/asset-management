namespace Authentication.Application.Interfaces;

public interface IAuthenticationService
{
    /// <summary>
    /// Authenticates a user by username and password, then issues access and refresh tokens.
    /// </summary>
    Task<ResponseDto<LoginResponse>> LoginAsync(string userName, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a new user account and sends an OTP to verify email ownership.
    /// </summary>
    Task<ResponseDto<string>> RegisterAsync(RegisterCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies register OTP and completes account creation.
    /// </summary>
    Task<ResponseDto<string>> VerifyRegisterEmailAsync(VerifyRegisterEmailCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Issues a new access and refresh token pair from a valid refresh token.
    /// </summary>
    Task<ResponseDto<LoginResponse>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes refresh token(s) for the current user as part of logout.
    /// </summary>
    Task<ResponseDto<string>> LogoutAsync(Guid userPublicId, string refreshToken, bool logoutAllSessions, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates a user through a third-party provider and issues tokens.
    /// </summary>
    Task<ResponseDto<LoginResponse>> LoginByThirdPartyAsync(ThirdPartyLoginCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Handles forgot-password flow by sending an OTP email.
    /// </summary>
    Task<ResponseDto<string>> ForgotPasswordAsync(ForgotPasswordCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies forgot-password OTP and opens a reset session.
    /// </summary>
    Task<ResponseDto<string>> VerifyForgotPasswordOtpAsync(VerifyForgotPasswordOtpCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes password after forgot-password OTP verification.
    /// </summary>
    Task<ResponseDto<string>> ChangeForgotPasswordAsync(ChangeForgotPasswordCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes password for the current authenticated user.
    /// </summary>
    Task<ResponseDto<string>> ChangePasswordAsync(ChangePasswordCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Links an external provider to the current authenticated user.
    /// </summary>
    Task<ResponseDto<string>> LinkExternalProviderAsync(LinkExternalProviderCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets current authenticated user information.
    /// </summary>
    Task<ResponseDto<UserInfoResponse>> GetUserInfoAsync(CancellationToken cancellationToken = default);
}
