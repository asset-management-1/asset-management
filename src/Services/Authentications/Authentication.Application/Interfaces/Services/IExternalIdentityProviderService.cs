namespace Authentication.Application.Interfaces.Services;

/// <summary>
/// Retrieves normalised identity claims after validating provider credentials trusted by Haven.
/// </summary>
public interface IExternalIdentityProviderService
{
    /// <summary>
    /// Gets normalised trusted identity claims after validating one supported provider credential.
    /// </summary>
    /// <param name="provider">The configured provider name.</param>
    /// <param name="externalToken">The provider credential supplied by the mobile client.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The normalised identity profile.</returns>
    Task<ExternalIdentityProfileResponseDto> GetValidatedIdentityAsync(
        string provider,
        string externalToken,
        CancellationToken cancellationToken = default);
}
