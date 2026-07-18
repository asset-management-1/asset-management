namespace Authentication.Application.Mappings.Authentications;

/// <summary>
/// Registers mappings from mediator commands into authentication request DTOs.
/// </summary>
public class AuthenticationRequestMapping : IRegister
{
    /// <summary>
    /// Registers Mapster configuration for authentication request projections.
    /// </summary>
    /// <param name="config">The Mapster configuration instance.</param>
    public void Register(TypeAdapterConfig config)
    {
        // Flat request mappings apply only the approved username/email normalization and enum conversions.
        config.NewConfig<LoginCommand, LoginRequestDto>()
            .Map(dest => dest.UserName, src => src.UserName.NormalizeUserName());

        config.NewConfig<RegisterCommand, RegisterRequestDto>()
            .Map(dest => dest.PartyType, src => src.PartyType.ToMasterDataCode())
            .Map(dest => dest.UserName, src => src.UserName.NormalizeUserName())
            .Map(dest => dest.Email, src => src.Email.NormalizeEmail());

        config.NewConfig<VerifyRegisterEmailCommand, VerifyRegisterEmailRequestDto>()
            .Map(dest => dest.Email, src => src.Email.NormalizeEmail());

        config.NewConfig<RefreshTokenCommand, RefreshTokenRequestDto>();

        config.NewConfig<ExternalLoginCommand, ExternalLoginRequestDto>();

        config.NewConfig<ForgotPasswordCommand, ForgotPasswordRequestDto>()
            .Map(dest => dest.Email, src => src.Email.NormalizeEmail());

        config.NewConfig<VerifyForgotPasswordOtpCommand, VerifyForgotPasswordOtpRequestDto>()
            .Map(dest => dest.Email, src => src.Email.NormalizeEmail());

        config.NewConfig<ChangeForgotPasswordCommand, ChangeForgotPasswordRequestDto>()
            .Map(dest => dest.Email, src => src.Email.NormalizeEmail());

        config.NewConfig<ChangePasswordCommand, ChangePasswordRequestDto>();

        config.NewConfig<LinkExternalProviderCommand, LinkExternalProviderRequestDto>();

        config.NewConfig<UnlinkExternalProviderCommand, UnlinkExternalProviderRequestDto>();

        config.NewConfig<SwitchPartyCommand, SwitchPartyRequestDto>()
            .Map(dest => dest.TargetContext, src => src.TargetContext.ToString().ToLowerInvariant());

        config.NewConfig<UpdateUserInfoCommand, UpdateUserInfoRequestDto>()
            .Map(dest => dest.Gender, src => src.Gender.HasValue ? src.Gender.Value.ToMasterDataCode() : null);

        config.NewConfig<ChangeEmailCommand, ChangeEmailRequestDto>()
            .Map(dest => dest.NewEmail, src => src.NewEmail.NormalizeEmail());

        config.NewConfig<VerifyChangeEmailOtpCommand, VerifyChangeEmailOtpRequestDto>()
            .Map(dest => dest.NewEmail, src => src.NewEmail.NormalizeEmail());

        config.NewConfig<SubmitKycCommand, SubmitKycRequestDto>()
            .Map(dest => dest.IdentifierType, src => src.IdentifierType.ToMasterDataCode())
            .Map(dest => dest.GenderOnDocument, src => src.GenderOnDocument.ToMasterDataCode());
    }
}
