namespace Haven.Application.Mappings.Tenants;

/// <summary>
/// Registers tenant workflow projections involving domain entities.
/// </summary>
public sealed class TenantEntityMapping : IRegister
{
    /// <summary>
    /// Registers profile and tracked-entity summary mappings used by tenant flows.
    /// </summary>
    /// <param name="config">The Mapster configuration instance.</param>
    public void Register(TypeAdapterConfig config)
    {
        // Profile input changes editable party contact fields only; identity and lifecycle stay backend-owned.
        config.NewConfig<TenantProfileModel, Party>()
            .Map(dest => dest.DisplayName, src => src.FullName)
            .Map(dest => dest.PrimaryPhone, src => src.Phone)
            .Map(dest => dest.PrimaryEmail, src => src.Email)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.PublicId)
            .Ignore(dest => dest.PartyTypeId)
            .Ignore(dest => dest.StatusId)
            .Ignore(dest => dest.UserParties)
            .Ignore(dest => dest.Occupancies);

        config.NewConfig<Property, TenantPropertySummaryResponseDto>()
            .Map(dest => dest.Id, src => src.PublicId);

        config.NewConfig<Unit, TenantRoomSummaryResponseDto>()
            .Map(dest => dest.Id, src => src.PublicId)
            .Map(dest => dest.Code, src => src.UnitCode)
            .Map(dest => dest.Name, src => src.UnitName);
    }
}
