namespace Authentication.Application.Interfaces.Services;

/// <summary>
/// Provides infrastructure-backed operations for external authentication flows.
/// </summary>
public interface IExternalAuthenticationService
{
    /// <summary>
    /// Returns all configured external provider names.
    /// </summary>
    /// <returns>The supported external provider names.</returns>
    IReadOnlyCollection<string> GetSupportedProviderNames();

    /// <summary>
    /// Authenticates with an external provider.
    /// </summary>
    /// <param name="request">The external-login request payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The issued Haven login token payload.</returns>
    Task<LoginResponseDto> LoginAsync(
        ExternalLoginRequestDto request,
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
