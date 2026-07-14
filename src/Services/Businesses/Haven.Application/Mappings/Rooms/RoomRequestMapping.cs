namespace Haven.Application.Mappings.Rooms;

/// <summary>
/// Registers room transport and query request mappings.
/// </summary>
public sealed class RoomRequestMapping : IRegister
{
    /// <summary>
    /// Registers request-to-model mappings for room workflows.
    /// </summary>
    /// <param name="config">The Mapster configuration instance.</param>
    public void Register(TypeAdapterConfig config)
    {
        // Handlers map transport commands and queries into party-scoped service requests.
        config.NewConfig<GetRoomsQuery, RoomListRequestModel>()
            .Ignore(dest => dest.CurrentParty);

        config.NewConfig<UpdateRoomCommand, RoomUpdateRequestModel>()
            .Map(dest => dest.RoomPublicId, src => src.Id)
            .Map(
                dest => dest.ChargePolicyMode,
                src => string.IsNullOrWhiteSpace(src.ChargePolicyMode)
                    ? null
                    : src.ChargePolicyMode.Trim().ToUpperInvariant())
            .Map(
                dest => dest.PackageMode,
                src => string.IsNullOrWhiteSpace(src.PackageMode)
                    ? null
                    : src.PackageMode.Trim().ToUpperInvariant())
            .Ignore(dest => dest.CurrentParty);

        config.NewConfig<RoomUpdateRequestModel, RoomFieldUpdateModel>();

        config.NewConfig<RoomListRequestModel, RoomListQueryParametersModel>()
            .Map(dest => dest.CurrentPartyId, src => src.CurrentParty.PartyId)
            .Map(dest => dest.RelationshipCodes, src => PROPERTY_ACCESS_RELATIONSHIP_CODES)
            .Map(dest => dest.PropertySearch, src => src.PropertySearch.NormalizeOptional())
            .Map(dest => dest.Search, src => src.Search.NormalizeOptional())
            .Map(dest => dest.RoomStatusCode, src => src.RoomStatusCode.NormalizeOptional())
            .Map(dest => dest.RentalModeCode, src => src.RentalModeCode.NormalizeOptional())
            .Map(dest => dest.PaymentStatusCode, src => src.PaymentStatusCode.NormalizeOptional())
            .Map(dest => dest.Offset, src => (src.PageNumber - 1) * src.PageSize);

        config.NewConfig<RoomDetailRequestModel, RoomScopedQueryParametersModel>()
            .Map(dest => dest.CurrentPartyId, src => src.CurrentParty.PartyId)
            .Map(dest => dest.RelationshipCodes, src => PROPERTY_ACCESS_RELATIONSHIP_CODES)
            .Map(dest => dest.NoFurniturePackageTypeCode, src => MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE);

        config.NewConfig<RoomDeleteRequestModel, RoomScopedQueryParametersModel>()
            .Map(dest => dest.CurrentPartyId, src => src.CurrentParty.PartyId)
            .Map(dest => dest.RelationshipCodes, src => PROPERTY_ACCESS_RELATIONSHIP_CODES)
            .Map(dest => dest.NoFurniturePackageTypeCode, src => MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE);

        config.NewConfig<RoomUpdateRequestModel, RoomScopedQueryParametersModel>()
            .Map(dest => dest.CurrentPartyId, src => src.CurrentParty.PartyId)
            .Map(dest => dest.RelationshipCodes, src => PROPERTY_ACCESS_RELATIONSHIP_CODES)
            .Map(dest => dest.NoFurniturePackageTypeCode, src => MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE);
    }
}
