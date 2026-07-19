namespace Authentication.Application.Interfaces.Services;

/// <summary>
/// Provides infrastructure-backed operations for local authentication flows.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticates a local username/password login and issues a token pair.
    /// </summary>
    /// <param name="request">The local login request payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The issued access and refresh token payload.</returns>
    Task<LoginResponseDto> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds the pending registration payload after validating uniqueness and hashing the password.
    /// </summary>
    /// <param name="request">The normalised registration request payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The pending registration payload to cache for email verification.</returns>
    Task<PendingRegisterCacheRequestDto> BuildPendingRegisterAsync(
        RegisterRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether a non-deleted user exists for the supplied email.
    /// </summary>
    /// <param name="normalizedEmail">The normalised email address.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns><c>true</c> when a matching user exists; otherwise <c>false</c>.</returns>
    Task<bool> UserExistsByEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes account creation from the pending register payload.
    /// </summary>
    /// <param name="pendingRegister">The verified pending registration payload from cache.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The registration completion result.</returns>
    Task<OperationStatusResponseDto> CompleteRegistrationAsync(
        PendingRegisterCacheRequestDto pendingRegister,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends one OTP email.
    /// </summary>
    /// <param name="request">The OTP email payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The OTP email send result.</returns>
    Task<OtpEmailResponseDto> SendOtpEmailAsync(
        OtpEmailRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a security notification to the old email when a change-email request starts.
    /// </summary>
    /// <param name="request">The security notification payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The email send result.</returns>
    Task<OtpEmailResponseDto> SendChangeEmailSecurityNotificationAsync(
        ChangeEmailSecurityNotificationRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exchanges a refresh token for a new login token pair.
    /// </summary>
    /// <param name="request">The refresh-token request payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The rotated access and refresh token payload.</returns>
    Task<LoginResponseDto> RefreshTokenAsync(
        RefreshTokenRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes refresh token state for logout.
    /// </summary>
    /// <param name="currentUserPublicId">The current authenticated user's public identifier.</param>
    /// <param name="currentSessionPublicId">The current authenticated session's public identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The logout result.</returns>
    Task<OperationStatusResponseDto> LogoutAsync(
        Guid currentUserPublicId,
        Guid currentSessionPublicId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes a password after forgot-password verification.
    /// </summary>
    /// <param name="request">The forgot-password change-password payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The password-change result.</returns>
    Task<OperationStatusResponseDto> ChangeForgotPasswordAsync(
        ChangeForgotPasswordRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes the current authenticated user's password.
    /// </summary>
    /// <param name="currentUserPublicId">The current authenticated user's public identifier.</param>
    /// <param name="currentSessionPublicId">The current authenticated session's public identifier.</param>
    /// <param name="request">The authenticated change-password payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The password-change result.</returns>
    Task<ChangePasswordResponseDto> ChangePasswordAsync(
        Guid currentUserPublicId,
        Guid currentSessionPublicId,
        ChangePasswordRequestDto request,
        CancellationToken cancellationToken = default);
}
