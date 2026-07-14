namespace Haven.Application.Mappings.Properties;

/// <summary>
/// Registers property transport, request-model, and query-parameter mappings.
/// </summary>
public sealed class PropertyRequestMapping : IRegister
{
    /// <summary>
    /// Registers request-side property mappings without applying business lookups.
    /// </summary>
    /// <param name="config">The Mapster configuration instance.</param>
    public void Register(TypeAdapterConfig config)
    {
        // Handlers map transport commands and queries into party-scoped service requests.
        config.NewConfig<GetPropertiesQuery, PropertyListRequestModel>()
            .Ignore(dest => dest.CurrentParty);

        config.NewConfig<GetPropertyDetailQuery, PropertyDetailRequestModel>()
            .Ignore(dest => dest.CurrentParty);

        config.NewConfig<CreatePropertyCommand, PropertyCreationRequestModel>()
            .Ignore(dest => dest.CurrentParty)
            .Ignore(dest => dest.PropertyCode)
            .Ignore(dest => dest.Locations);

        config.NewConfig<UpdatePropertyCommand, PropertyUpdateRequestModel>()
            .Map(dest => dest.PropertyPublicId, src => src.Id)
            .Ignore(dest => dest.CurrentParty)
            .Ignore(dest => dest.PropertyTypeId)
            .Ignore(dest => dest.ProvinceId)
            .Ignore(dest => dest.DistrictId)
            .Ignore(dest => dest.WardId)
            .Ignore(dest => dest.Locations);

        config.NewConfig<PropertyUpdateRoomModel, RoomFieldUpdateModel>()
            .Map(dest => dest.FloorNumber, src => src.FloorNumber)
            .Map(dest => dest.Name, src => src.Room.Name)
            .Map(dest => dest.TypeCode, src => src.Room.TypeCode)
            .Map(dest => dest.RentalModeCode, src => src.Room.RentalModeCode)
            .Map(dest => dest.AreaSqm, src => src.Room.AreaSqm)
            .Map(dest => dest.BaseRentAmount, src => src.Room.BaseRentAmount)
            .Map(dest => dest.DefaultDepositAmount, src => src.Room.DefaultDepositAmount)
            .Map(dest => dest.TotalBeds, src => src.Room.TotalBeds)
            .Map(dest => dest.IsPetAllowed, src => src.Room.IsPetAllowed);

        config.NewConfig<PropertyCreationRequestModel, LocationKeyModel>();
        config.NewConfig<PropertyUpdateRequestModel, LocationKeyModel>();

        // Resolved location rows prepare nullable persistence IDs for the property merge.
        config.NewConfig<PropertyLocationContextModel, PropertyUpdateRequestModel>()
            .IgnoreNullValues(true)
            .Map(dest => dest.ProvinceId, src => src.Province == null ? (long?)null : src.Province.Id)
            .Map(dest => dest.DistrictId, src => src.District == null ? (long?)null : src.District.Id)
            .Map(dest => dest.WardId, src => src.Ward == null ? (long?)null : src.Ward.Id);

        config.NewConfig<PropertyListRequestModel, PropertyListQueryParametersModel>()
            .Map(dest => dest.CurrentPartyId, src => src.CurrentParty.PartyId)
            .Map(dest => dest.RelationshipCodes, src => PROPERTY_ACCESS_RELATIONSHIP_CODES)
            .Map(dest => dest.Search, src => src.Search.NormalizeOptional())
            .Map(dest => dest.PropertyTypeCode, src => src.PropertyTypeCode.NormalizeOptional())
            .Map(dest => dest.StatusCode, src => src.StatusCode.NormalizeOptional())
            .Map(dest => dest.RoomStatusCode, src => src.RoomStatusCode.NormalizeOptional())
            .Map(dest => dest.PaymentStatusCode, src => src.PaymentStatusCode.NormalizeOptional())
            .Map(dest => dest.Offset, src => (src.PageNumber - 1) * src.PageSize);

        config.NewConfig<PropertyListRequestModel, PropertyChildRowsQueryParametersModel>()
            .Ignore(dest => dest.PropertyPublicIds)
            .Map(dest => dest.Search, src => src.Search.NormalizeOptional())
            .Map(dest => dest.RoomStatusCode, src => src.RoomStatusCode.NormalizeOptional())
            .Map(dest => dest.PaymentStatusCode, src => src.PaymentStatusCode.NormalizeOptional());

        config.NewConfig<PropertyDetailRequestModel, PropertyScopedQueryParametersModel>()
            .Map(dest => dest.CurrentPartyId, src => src.CurrentParty.PartyId)
            .Map(dest => dest.RelationshipCodes, src => PROPERTY_ACCESS_RELATIONSHIP_CODES);
    }
}
