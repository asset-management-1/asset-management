namespace Authentication.Application.Mappings.Authentications;

/// <summary>
/// Registers mappings between authentication DTOs and response DTOs.
/// </summary>
public class AuthenticationDtoMapping : IRegister
{
    /// <summary>
    /// Registers Mapster configuration for authentication DTO projections.
    /// </summary>
    /// <param name="config">The Mapster configuration instance.</param>
    public void Register(TypeAdapterConfig config)
    {
        // Email send results expose a single success flag, so bool-to-response mapping keeps service returns compact.
        config.NewConfig<bool, OtpEmailResponseDto>()
            .Map(dest => dest.IsSuccess, src => src);

        // Register request fields are reused by uniqueness checks and register session payloads.
        config.NewConfig<RegisterRequestDto, RegistrationUniquenessRequestDto>();

        config.NewConfig<RegisterRequestDto, PendingRegisterCacheRequestDto>()
            .Ignore(dest => dest.PasswordHash)
            .Ignore(dest => dest.CreatedAt);

        config.NewConfig<PendingRegisterCacheRequestDto, RegisterAccountProvisionRequestDto>()
            .Ignore(dest => dest.PartyTypeId)
            .Ignore(dest => dest.PartyStatusId)
            .Ignore(dest => dest.UserStatusId);

        // Change-email notification request has the same email shape as the prepared change-email response.
        config.NewConfig<ChangeEmailStartResponseDto, ChangeEmailSecurityNotificationRequestDto>();

        // Provider profiles expose only normalised prefill fields for first-time registration.
        config.NewConfig<ExternalIdentityProfileResponseDto, ExternalRegistrationPrefillDto>()
            .Map(dest => dest.Email, src => src.Email.NormalizeEmail());

        // Existing external identities reuse the standard login payload inside one stable response envelope.
        config.NewConfig<LoginResponseDto, ExternalLoginResponseDto>()
            .Map(dest => dest.IsNewRegistration, _ => false)
            .Map(dest => dest.Login, src => src)
            .Map(dest => dest.Registration, _ => (ExternalRegistrationPrefillDto)null);
    }
}
