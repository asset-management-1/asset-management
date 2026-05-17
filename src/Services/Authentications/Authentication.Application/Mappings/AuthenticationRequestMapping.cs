namespace Authentication.Application.Mappings;

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
        // Request mappings own input normalization so handlers and services can trust DTO shape.
        config.NewConfig<LoginCommand, LoginRequestDto>()
            .Map(dest => dest.UserName, src => src.UserName.NormalizeOptional());

        config.NewConfig<RegisterCommand, RegisterRequestDto>()
            .Map(dest => dest.UserName, src => src.UserName.NormalizeOptional())
            .Map(dest => dest.PartyType, src => src.PartyType.ToMasterDataCode())
            .Map(dest => dest.Email, src => src.Email.NormalizeEmail())
            .Map(dest => dest.PhoneNumber, src => src.PhoneNumber.NormalizeOptional())
            .Map(dest => dest.FullName, src => src.FullName.NormalizeOptional());

        config.NewConfig<VerifyRegisterEmailCommand, VerifyRegisterEmailRequestDto>()
            .Map(dest => dest.Email, src => src.Email.NormalizeEmail())
            .Map(dest => dest.Otp, src => src.Otp.NormalizeOptional());

        config.NewConfig<RefreshTokenCommand, RefreshTokenRequestDto>();

        config.NewConfig<ExternalLoginCommand, ExternalLoginRequestDto>()
            .Map(dest => dest.Provider, src => src.Provider.NormalizeOptional());

        config.NewConfig<ForgotPasswordCommand, ForgotPasswordRequestDto>()
            .Map(dest => dest.Email, src => src.Email.NormalizeEmail());

        config.NewConfig<VerifyForgotPasswordOtpCommand, VerifyForgotPasswordOtpRequestDto>()
            .Map(dest => dest.Email, src => src.Email.NormalizeEmail())
            .Map(dest => dest.Otp, src => src.Otp.NormalizeOptional());

        config.NewConfig<ChangeForgotPasswordCommand, ChangeForgotPasswordRequestDto>()
            .Map(dest => dest.Email, src => src.Email.NormalizeEmail());

        config.NewConfig<ChangePasswordCommand, ChangePasswordRequestDto>();

        config.NewConfig<LinkExternalProviderCommand, LinkExternalProviderRequestDto>()
            .Map(dest => dest.Provider, src => src.Provider.NormalizeOptional());

        config.NewConfig<UnlinkExternalProviderCommand, UnlinkExternalProviderRequestDto>()
            .Map(dest => dest.Provider, src => src.Provider.NormalizeOptional());

        config.NewConfig<SwitchPartyCommand, SwitchPartyRequestDto>()
            .Map(dest => dest.TargetContext, src => src.TargetContext.ToContextValue());

        config.NewConfig<UpdateUserInfoCommand, UpdateUserInfoRequestDto>()
            .Map(dest => dest.FullName, src => src.FullName.NormalizeOptional())
            .Map(dest => dest.PhoneNumber, src => src.PhoneNumber.NormalizeOptional())
            .Map(dest => dest.Gender, src => src.Gender.HasValue ? src.Gender.Value.ToMasterDataCode() : null);

        config.NewConfig<ChangeEmailCommand, ChangeEmailRequestDto>()
            .Map(dest => dest.NewEmail, src => src.NewEmail.NormalizeEmail());

        config.NewConfig<VerifyChangeEmailOtpCommand, VerifyChangeEmailOtpRequestDto>()
            .Map(dest => dest.NewEmail, src => src.NewEmail.NormalizeEmail())
            .Map(dest => dest.Otp, src => src.Otp.NormalizeOptional());

        config.NewConfig<SubmitKycCommand, SubmitKycRequestDto>()
            .Map(dest => dest.IdentifierType, src => src.IdentifierType.ToMasterDataCode())
            .Map(dest => dest.IdentifierValue, src => src.IdentifierValue.NormalizeOptional())
            .Map(dest => dest.FullNameOnDocument, src => src.FullNameOnDocument.NormalizeOptional())
            .Map(dest => dest.GenderOnDocument, src => src.GenderOnDocument.ToMasterDataCode())
            .Map(dest => dest.RegisteredAddress, src => src.RegisteredAddress.NormalizeOptional())
            .Map(dest => dest.IssuedBy, src => src.IssuedBy.NormalizeOptional());

        config.NewConfig<RegisterUserVehicleCommand, RegisterUserVehicleRequestDto>()
            .Map(dest => dest.VehicleType, src => src.VehicleType.ToMasterDataCode())
            .Map(dest => dest.VehicleName, src => src.VehicleName.NormalizeOptional())
            .Map(dest => dest.LicensePlate, src => src.LicensePlate.NormalizeOptional());
    }
}
