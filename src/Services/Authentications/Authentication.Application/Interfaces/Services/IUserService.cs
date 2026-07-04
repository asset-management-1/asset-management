namespace Authentication.Application.Interfaces.Services;

/// <summary>
/// Provides infrastructure-backed operations for current-user profile and context flows.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Loads current-user profile information.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The current user's profile, context, and external-link state.</returns>
    Task<UserInfoResponseDto> GetUserInfoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates profile fields for the current authenticated user.
    /// </summary>
    /// <param name="request">The profile update payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The profile update result.</returns>
    Task<OperationStatusResponseDto> UpdateUserInfoAsync(
        UpdateUserInfoRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a change-email request and returns notification data.
    /// </summary>
    /// <param name="request">The change-email request payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The validated change-email data.</returns>
    Task<ChangeEmailStartResponseDto> PrepareChangeEmailAsync(
        ChangeEmailRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies a verified change-email request to the current user.
    /// </summary>
    /// <param name="request">The verified change-email payload.</param>
    /// <param name="currentUserPublicId">The current authenticated user's public identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The email-change result.</returns>
    Task<OperationStatusResponseDto> VerifyChangeEmailAsync(
        VerifyChangeEmailOtpRequestDto request,
        Guid currentUserPublicId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Submits identity-document KYC data and private document metadata for manual admin review.
    /// </summary>
    /// <param name="request">The KYC submission payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The KYC submission result.</returns>
    Task<KycSubmissionResponseDto> SubmitKycAsync(
        SubmitKycRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Switches the current user's active party context.
    /// </summary>
    /// <param name="request">The target party-context switch payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The resolved current context and available contexts after switching.</returns>
    Task<SwitchPartyResponseDto> SwitchPartyAsync(
        SwitchPartyRequestDto request,
        CancellationToken cancellationToken = default);
}
