namespace Haven.Application.Mappings.Properties;

/// <summary>
/// Registers flat property read-model to response mappings.
/// </summary>
public sealed class PropertyResponseMapping : IRegister
{
    /// <summary>
    /// Registers row projections used by property list and detail responses.
    /// </summary>
    /// <param name="config">The Mapster configuration instance.</param>
    public void Register(TypeAdapterConfig config)
    {
        // Property headers map through Mapster; nested grouping remains in PropertyResponseMapper.
        config.NewConfig<PropertyRowModel, PropertyListItemResponseDto>()
            .Map(dest => dest.Id, src => src.PropertyPublicId)
            .Map(
                dest => dest.Address,
                src => !string.IsNullOrWhiteSpace(src.FormattedAddress)
                    ? src.FormattedAddress
                    : string.Join(
                        COMMA_SPACE_SEPARATOR,
                        new[]
                        {
                            src.StreetAddress,
                            src.WardName,
                            src.DistrictName,
                            src.ProvinceName
                        }.Where(value => !string.IsNullOrWhiteSpace(value))))
            .Map(dest => dest.TotalRooms, src => src.TotalUnits)
            .Map(dest => dest.AvailableRooms, src => src.AvailableUnitCount)
            .Ignore(dest => dest.Floors);

        config.NewConfig<PropertyRowModel, PropertyDetailResponseDto>()
            .Map(dest => dest.BasicInfo, src => src.Adapt<PropertyDetailBasicInfoResponseDto>())
            .Ignore(dest => dest.Structure)
            .Ignore(dest => dest.WholeBuildingRental)
            .Ignore(dest => dest.ChargePolicies)
            .Ignore(dest => dest.PackageTemplates);

        config.NewConfig<PropertyRowModel, PropertyDetailBasicInfoResponseDto>()
            .Map(dest => dest.Id, src => src.PropertyPublicId)
            .Map(dest => dest.Code, src => src.PropertyCode);

        config.NewConfig<PropertyRowModel, PropertyDetailStructureResponseDto>()
            .Map(dest => dest.TotalRooms, src => src.TotalUnits)
            .Ignore(dest => dest.Floors);

        config.NewConfig<PropertyFloorRowModel, PropertyListFloorResponseDto>()
            .Map(dest => dest.Number, src => src.FloorNumber)
            .Map(dest => dest.TotalRooms, src => src.UnitCount)
            .Map(dest => dest.AvailableRooms, src => src.AvailableUnitCount)
            .Ignore(dest => dest.Rooms);

        config.NewConfig<PropertyRoomRowModel, PropertyListRoomResponseDto>()
            .Map(dest => dest.Id, src => src.UnitPublicId)
            .Map(dest => dest.Code, src => src.UnitCode)
            .Map(dest => dest.Name, src => src.UnitName)
            .Map(dest => dest.TotalOccupiedBeds, src => src.OccupiedBedCount)
            .Map(dest => dest.TotalBeds, src => src.BedCount)
            .Ignore(dest => dest.Tenants);

        config.NewConfig<PropertyRoomTenantRowModel, PropertyListRoomTenantResponseDto>()
            .Map(dest => dest.Id, src => src.OccupancyPublicId)
            .Map(dest => dest.TenantId, src => src.TenantPublicId)
            .Map(dest => dest.Tenant, src => src.TenantName);

        config.NewConfig<PropertyFloorRowModel, PropertyDetailFloorResponseDto>()
            .Map(dest => dest.Number, src => src.FloorNumber)
            .Ignore(dest => dest.Rooms);

        config.NewConfig<PropertyRoomRowModel, PropertyDetailRoomResponseDto>()
            .Map(dest => dest.Id, src => src.UnitPublicId)
            .Map(dest => dest.Code, src => src.UnitCode)
            .Map(dest => dest.Name, src => src.UnitName)
            .Map(dest => dest.TypeCode, src => src.UnitTypeCode)
            .Map(dest => dest.TypeName, src => src.UnitTypeName)
            .Map(dest => dest.TotalBeds, src => src.BedCount);

        config.NewConfig<PropertyChargePolicyRowModel, PropertyChargePolicyResponseDto>()
            .Map(dest => dest.Id, src => src.PolicyPublicId)
            .Map(dest => dest.Code, src => src.ChargeTypeCode)
            .Map(dest => dest.Name, src => string.IsNullOrWhiteSpace(src.ChargeName) ? src.ChargeTypeName : src.ChargeName)
            .Map(
                dest => dest.CalculationMethodCode,
                src => src.IsUsageBased ? CALCULATION_METHOD_METER_READING : CALCULATION_METHOD_FIXED);

        config.NewConfig<PropertyPackageTemplateRowModel, PropertyPackageTemplateResponseDto>()
            .Map(dest => dest.Id, src => src.PackagePublicId)
            .Map(dest => dest.Name, src => src.PackageName)
            .Ignore(dest => dest.Items);

        config.NewConfig<PropertyPackageTemplateRowModel, PropertyPackageTemplateItemResponseDto>()
            .Map(dest => dest.Name, src => src.ItemName);

        config.NewConfig<PropertyWholeBuildingRentalRowModel, PropertyWholeBuildingRentalContractResponseDto>()
            .Map(dest => dest.Id, src => src.ContractPublicId.GetValueOrDefault())
            .Map(dest => dest.Code, src => src.ContractCode)
            .Map(dest => dest.TenantId, src => src.TenantPublicId)
            .Map(dest => dest.Tenant, src => src.TenantName);
    }
}
