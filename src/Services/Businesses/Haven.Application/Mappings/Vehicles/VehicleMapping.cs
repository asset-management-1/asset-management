namespace Haven.Application.Mappings.Vehicles;

/// <summary>
/// Registers flat vehicle command, entity, and response mappings.
/// </summary>
public sealed class VehicleMapping : IRegister
{
    /// <summary>
    /// Registers vehicle mappings that do not require lookups or side effects.
    /// </summary>
    /// <param name="config">The Mapster configuration registry.</param>
    public void Register(TypeAdapterConfig config)
    {
        // Multipart commands and service requests have the same transport fields; party context stays handler-owned.
        config.NewConfig<CreateVehicleCommand, VehicleCreateRequestModel>().Ignore(dest => dest.CurrentParty);
        config.NewConfig<UpdateVehicleCommand, VehicleUpdateRequestModel>().Ignore(dest => dest.CurrentParty);

        config.NewConfig<VehicleCreateRequestModel, PartyVehicle>()
            .Map(dest => dest.VehicleName, src => src.Name)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.PublicId)
            .Ignore(dest => dest.PartyId)
            .Ignore(dest => dest.UnitId)
            .Ignore(dest => dest.VehicleTypeId)
            .Ignore(dest => dest.RegistrationFrontImageUrl)
            .Ignore(dest => dest.RegistrationSideImageUrl)
            .Ignore(dest => dest.VehicleFrontImageUrl)
            .Ignore(dest => dest.VehicleSideImageUrl);

        // Partial updates map only flat submitted scalar fields; room, payer, type IDs, and image URLs are workflow-owned.
        config.NewConfig<VehicleUpdateRequestModel, PartyVehicle>()
            .IgnoreNullValues(true)
            .Map(dest => dest.VehicleName, src => src.Name)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.PublicId)
            .Ignore(dest => dest.PartyId)
            .Ignore(dest => dest.UnitId)
            .Ignore(dest => dest.VehicleTypeId)
            .Ignore(dest => dest.RegistrationFrontImageUrl)
            .Ignore(dest => dest.RegistrationSideImageUrl)
            .Ignore(dest => dest.VehicleFrontImageUrl)
            .Ignore(dest => dest.VehicleSideImageUrl);

        config.NewConfig<VehicleDetailRowModel, VehicleDetailResponseDto>()
            .Map(dest => dest.Id, src => src.VehiclePublicId)
            .Map(dest => dest.Room.Id, src => src.RoomPublicId)
            .Map(dest => dest.Room.Name, src => src.RoomName)
            .Map(dest => dest.Payer.Id, src => src.PayerTenantPublicId)
            .Map(dest => dest.Payer.Tenant, src => src.PayerTenantName)
            .Map(dest => dest.Name, src => src.VehicleName);

        config.NewConfig<GetVehiclesQuery, VehicleListRequestModel>()
            .Ignore(dest => dest.CurrentParty);

        config.NewConfig<VehicleListRequestModel, VehicleListQueryParametersModel>()
            .Map(dest => dest.CurrentPartyId, src => src.CurrentParty.PartyId)
            .Map(dest => dest.RelationshipCodes, src => PROPERTY_ACCESS_RELATIONSHIP_CODES);

        config.NewConfig<VehicleListRowModel, VehicleListItemResponseDto>()
            .Map(dest => dest.Id, src => src.VehiclePublicId)
            .Map(dest => dest.Payer.Id, src => src.PayerTenantPublicId)
            .Map(dest => dest.Payer.Tenant, src => src.PayerTenantName)
            .Map(dest => dest.Name, src => src.VehicleName);
    }
}
