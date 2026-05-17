namespace Authentication.Application.Mappings;

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

        // Vehicle responses expose public enum contracts while DB rows still store master-data codes.
        config.NewConfig<PartyVehicle, UserVehicleResponseDto>()
            .Map(dest => dest.VehicleType, src => ApiEnumContractMapper.ToRequiredVehicleType(
                src.VehicleType.Code != null && src.VehicleType.Code != string.Empty
                    ? src.VehicleType.Code
                    : src.VehicleType.Name))
            .Map(dest => dest.VehicleTypeDisplayName, src => src.VehicleType.Name)
            .Map(dest => dest.Status, _ => UserVehicleStatusEnum.Active)
            .Map(dest => dest.ThumbnailUrl, src => src.FrontImageUrl ?? src.SideImageUrl);
    }
}
