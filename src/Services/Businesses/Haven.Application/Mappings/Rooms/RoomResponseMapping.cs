namespace Haven.Application.Mappings.Rooms;

/// <summary>
/// Registers flat room read-model to response mappings.
/// </summary>
public sealed class RoomResponseMapping : IRegister
{
    /// <summary>
    /// Registers room list and detail response projections.
    /// </summary>
    /// <param name="config">The Mapster configuration instance.</param>
    public void Register(TypeAdapterConfig config)
    {
        // Row models keep database names explicit; response DTOs use the public API shape.
        config.NewConfig<RoomListRoomRowModel, RoomListRoomResponseDto>()
            .Map(dest => dest.Id, src => src.UnitPublicId)
            .Map(dest => dest.Code, src => src.UnitCode)
            .Map(dest => dest.Name, src => src.UnitName)
            .Map(dest => dest.FloorNumber, src => src.FloorNumber)
            .Map(dest => dest.TypeCode, src => src.UnitTypeCode)
            .Map(dest => dest.TypeName, src => src.UnitTypeName)
            .Ignore(dest => dest.Tenants);

        config.NewConfig<RoomTenantRowModel, RoomListTenantResponseDto>()
            .Map(dest => dest.Id, src => src.OccupancyPublicId)
            .Map(dest => dest.TenantId, src => src.TenantPublicId)
            .Map(dest => dest.Tenant, src => src.TenantName);

        config.NewConfig<RoomDetailRowModel, RoomDetailBasicInfoResponseDto>()
            .Map(dest => dest.Id, src => src.UnitPublicId)
            .Map(dest => dest.Code, src => src.UnitCode)
            .Map(dest => dest.Name, src => src.UnitName)
            .Map(dest => dest.PropertyId, src => src.PropertyPublicId)
            .Map(dest => dest.TypeCode, src => src.UnitTypeCode)
            .Map(dest => dest.TypeName, src => src.UnitTypeName);

        config.NewConfig<RoomChargePolicyRowModel, RoomChargePolicyResponseDto>()
            .Map(dest => dest.Id, src => src.PolicyPublicId)
            .Map(dest => dest.Code, src => src.ChargeTypeCode)
            .Map(dest => dest.Name, src => string.IsNullOrWhiteSpace(src.ChargeName) ? src.ChargeTypeName : src.ChargeName)
            .Map(
                dest => dest.CalculationMethodCode,
                src => src.IsUsageBased ? CALCULATION_METHOD_METER_READING : CALCULATION_METHOD_FIXED);

        config.NewConfig<RoomPackageTemplateRowModel, RoomPackageTemplateResponseDto>()
            .Map(dest => dest.Id, src => src.PackagePublicId)
            .Map(dest => dest.Name, src => src.PackageName)
            .Ignore(dest => dest.Items);

        config.NewConfig<RoomPackageTemplateRowModel, RoomPackageTemplateItemResponseDto>()
            .Map(dest => dest.Name, src => src.ItemName);
    }
}
