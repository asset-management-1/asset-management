namespace Authentication.Application.Mappings;

/// <summary>
/// Registers mappings from prepared authentication provision DTOs into domain entities.
/// </summary>
public class AuthenticationEntityMapping : IRegister
{
    /// <summary>
    /// Registers Mapster configuration for Party and User entity projection.
    /// </summary>
    /// <param name="config">The Mapster configuration instance.</param>
    public void Register(TypeAdapterConfig config)
    {
        // Provision mappings only copy prepared persistence values; generated IDs and timestamps stay EF/audit-owned.
        config.NewConfig<RegisterAccountProvisionRequestDto, Party>()
            .Map(dest => dest.PartyTypeId, src => src.PartyTypeId)
            .Map(dest => dest.DisplayName, src => src.FullName)
            .Map(dest => dest.PrimaryEmail, src => src.Email)
            .Map(dest => dest.PrimaryPhone, src => src.PhoneNumber)
            .Map(dest => dest.StatusId, src => src.PartyStatusId);

        config.NewConfig<RegisterAccountProvisionRequestDto, User>()
            .Map(dest => dest.UserName, src => src.UserName)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.PhoneNumber, src => src.PhoneNumber)
            .Map(dest => dest.PasswordHash, src => src.PasswordHash)
            .Map(dest => dest.EmailConfirmed, _ => true)
            .Map(dest => dest.FullName, src => src.FullName)
            .Map(dest => dest.StatusId, src => src.UserStatusId);

        config.NewConfig<ExternalAccountProvisionRequestDto, Party>()
            .Map(dest => dest.PartyTypeId, src => src.PartyTypeId)
            .Map(dest => dest.DisplayName, src => src.FullName)
            .Map(dest => dest.PrimaryEmail, src => src.Email)
            .Map(dest => dest.StatusId, src => src.PartyStatusId);

        config.NewConfig<ExternalAccountProvisionRequestDto, User>()
            .Map(dest => dest.UserName, src => src.UserName)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.FullName, src => src.FullName)
            .Map(dest => dest.EmailConfirmed, src => src.EmailConfirmed)
            .Map(dest => dest.StatusId, src => src.UserStatusId);

        // Password hashing only needs a normalized identity-shaped user object, not persistence state.
        config.NewConfig<RegisterRequestDto, User>()
            .Map(dest => dest.UserName, src => src.UserName)
            .Map(dest => dest.Email, src => src.Email);

        // Device metadata maps through Mapster; service code owns session id, token hash, and timestamps.
        config.NewConfig<ClientDeviceContextModel, RefreshToken>()
            .Map(dest => dest.DeviceId, src => src.DeviceId)
            .Map(dest => dest.DeviceName, src => src.DeviceName)
            .Map(dest => dest.DeviceType, src => src.DeviceType)
            .Map(dest => dest.UserAgent, src => src.UserAgent)
            .Map(dest => dest.IpAddress, src => src.IpAddress);

        // Party context projection exposes only the party type fields needed by context guards.
        config.NewConfig<Party, CurrentPartyContextModel>()
            .Map(dest => dest.PartyId, src => src.Id)
            .Map(dest => dest.PartyTypeCode, src => src.PartyType.Code)
            .Map(dest => dest.PartyTypeName, src => src.PartyType.Name);

        // KYC submit maps document fields only; status and gender master-data ids stay service-owned.
        config.NewConfig<SubmitKycRequestDto, PartyIdentifier>()
            .Ignore(dest => dest.IdentifierType)
            .Ignore(dest => dest.GenderOnDocumentId)
            .Ignore(dest => dest.GenderOnDocument)
            .Ignore(dest => dest.StatusId)
            .Ignore(dest => dest.Status)
            .Map(dest => dest.FullNameOnDocument, src => src.FullNameOnDocument)
            .Map(dest => dest.DateOfBirthOnDocument, src => src.DateOfBirthOnDocument)
            .Map(dest => dest.RegisteredAddress, src => src.RegisteredAddress)
            .Map(dest => dest.IssuedDate, src => src.IssuedDate)
            .Map(dest => dest.ExpiredDate, src => src.ExpiredDate)
            .Map(dest => dest.IssuedBy, src => src.IssuedBy);
    }
}
