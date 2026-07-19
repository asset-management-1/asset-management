namespace Authentication.Application.Interfaces.Services;

/// <summary>
/// Provides infrastructure-backed operations for external authentication flows.
/// </summary>
public interface IExternalAuthenticationService
{
    /// <summary>
    /// Authenticates with an external provider.
    /// </summary>
    /// <param name="request">The external-login request payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The existing-account login payload or first-time registration prefill.</returns>
    Task<ExternalLoginResponseDto> LoginAsync(
        ExternalLoginRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revalidates provider identity and completes first-time Haven account registration.
    /// </summary>
    /// <param name="request">The external-registration details and provider credential.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The issued Haven login token payload after registration completes.</returns>
    Task<LoginResponseDto> CompleteRegistrationAsync(
        CompleteExternalRegistrationRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Links an external provider to the current user.
    /// </summary>
    /// <param name="request">The external-link request payload.</param>
    /// <param name="currentUserPublicId">The current authenticated user's public identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The external-link result.</returns>
    Task<OperationStatusResponseDto> LinkAsync(
        LinkExternalProviderRequestDto request,
        Guid currentUserPublicId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Unlinks an external provider from the current user.
    /// </summary>
    /// <param name="request">The external-unlink request payload.</param>
    /// <param name="currentUserPublicId">The current authenticated user's public identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The external-unlink result.</returns>
    Task<OperationStatusResponseDto> UnlinkAsync(
        UnlinkExternalProviderRequestDto request,
        Guid currentUserPublicId,
        CancellationToken cancellationToken = default);
}
