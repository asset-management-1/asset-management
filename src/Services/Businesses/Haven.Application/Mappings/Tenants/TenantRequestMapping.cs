namespace Haven.Application.Mappings.Tenants;

/// <summary>
/// Registers tenant transport and query request mappings.
/// </summary>
public sealed class TenantRequestMapping : IRegister
{
    /// <summary>
    /// Registers request-to-model mappings for tenant workflows.
    /// </summary>
    /// <param name="config">The Mapster configuration instance.</param>
    public void Register(TypeAdapterConfig config)
    {
        // Commands and queries map into service request models; handlers supply party context.
        config.NewConfig<CreateTenantCommand, TenantCreateRequestModel>()
            .Map(dest => dest.TenantProfile, src => src.TenantProfile)
            .Ignore(dest => dest.CurrentParty);

        config.NewConfig<TenantProfileRequestDto, TenantProfileModel>();

        config.NewConfig<GetTenantsQuery, TenantListRequestModel>()
            .Ignore(dest => dest.CurrentParty);

        config.NewConfig<TenantListRequestModel, TenantListQueryParametersModel>()
            .Map(dest => dest.CurrentPartyId, src => src.CurrentParty.PartyId)
            .Map(dest => dest.RelationshipCodes, src => PROPERTY_ACCESS_RELATIONSHIP_CODES)
            .Map(dest => dest.Search, src => src.Search.NormalizeOptional())
            .Map(dest => dest.RoleCode, src => src.RoleCode.NormalizeOptional())
            .Map(dest => dest.OccupancyStatusCode, src => src.OccupancyStatusCode.NormalizeOptional())
            .Map(dest => dest.Offset, src => (src.PageNumber - 1) * src.PageSize);

        config.NewConfig<TenantDetailRequestModel, TenantScopedQueryParametersModel>()
            .Map(dest => dest.CurrentPartyId, src => src.CurrentParty.PartyId)
            .Map(dest => dest.RelationshipCodes, src => PROPERTY_ACCESS_RELATIONSHIP_CODES);

        config.NewConfig<ConfirmTenantJoinCommand, TenantJoinConfirmRequestModel>()
            .Ignore(dest => dest.CurrentParty);
    }
}
