namespace Haven.Application.Mappings;

/// <summary>
/// Registers Mapster mappings for property read and write projections.
/// </summary>
public class PropertyMapping : IRegister
{
    /// <summary>
    /// Registers property read and creation mappings for property workflows.
    /// </summary>
    /// <param name="config">The Mapster configuration instance.</param>
    public void Register(TypeAdapterConfig config)
    {
        // Handlers map transport commands/queries into party-scoped service requests.
        config.NewConfig<GetPropertiesQuery, PropertyListRequestModel>()
            .Ignore(dest => dest.CurrentParty);

        config.NewConfig<GetPropertyDetailQuery, PropertyDetailRequestModel>()
            .Ignore(dest => dest.CurrentParty);

        config.NewConfig<CreatePropertyCommand, PropertyCreationRequestModel>()
            .Ignore(dest => dest.CurrentParty)
            .Ignore(dest => dest.PropertyCode)
            .Ignore(dest => dest.MasterData)
            .Ignore(dest => dest.Locations);

        config.NewConfig<PropertyCreationRequestModel, LocationKeyModel>();

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

        // Property headers map through Mapster; floor/package grouping is added by PropertyResponseMapper.
        config.NewConfig<PropertyRowModel, PropertyListItemResponseDto>()
            .Map(dest => dest.Id, src => src.PropertyPublicId)
            .Map(dest => dest.Address, src => BuildAddressText(src))
            .Map(dest => dest.TotalRooms, src => src.TotalUnits)
            .Map(dest => dest.AvailableRooms, src => src.AvailableUnitCount)
            .Ignore(dest => dest.Floors);

        config.NewConfig<PropertyRowModel, PropertyDetailResponseDto>()
            .Map(dest => dest.BasicInfo, src => src.Adapt<PropertyDetailBasicInfoResponseDto>())
            .Map(dest => dest.Address, src => src.Adapt<PropertyAddressResponseDto>())
            .Ignore(dest => dest.Structure)
            .Ignore(dest => dest.ChargePolicies)
            .Ignore(dest => dest.PackageTemplates)
            .Ignore(dest => dest.ManagementSummary);

        config.NewConfig<PropertyRowModel, PropertyAddressResponseDto>();
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
            .Map(dest => dest.Id, src => src.TenantPublicId)
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

        config.NewConfig<PropertyManagementSummaryRowModel, PropertyManagementSummaryResponseDto>()
            .Map(dest => dest.TotalTenants, src => src.TenantCount)
            .Map(dest => dest.TotalActiveContracts, src => src.ActiveContractCount)
            .Map(dest => dest.TotalDocuments, src => src.DocumentCount)
            .Map(dest => dest.TotalUnpaidInvoices, src => src.UnpaidInvoiceCount)
            .Map(dest => dest.TotalOverdueInvoices, src => src.OverdueInvoiceCount);

        // Creation request data maps to draft property entity fields after service-owned lookup resolution.
        config.NewConfig<PropertyCreationRequestModel, PropertyCreationGraphModel>()
            .MapWith(src => BuildCreationGraph(src, config));

        config.NewConfig<PropertyCreationRequestModel, Property>()
            .Map(dest => dest.PublicId, src => Guid.NewGuid())
            .Map(dest => dest.PropertyCode, src => src.PropertyCode)
            .Map(dest => dest.Name, src => src.Name.NormalizeOptional())
            .Map(dest => dest.PropertyTypeId, src => src.MasterData.RequireValue(MASTER_TYPE_PROPERTY_TYPE, src.PropertyTypeCode).Id)
            .Map(dest => dest.ProvinceId, src => src.Locations.Province == null ? (long?)null : src.Locations.Province.Id)
            .Map(dest => dest.DistrictId, src => src.Locations.District == null ? (long?)null : src.Locations.District.Id)
            .Map(dest => dest.WardId, src => src.Locations.Ward == null ? (long?)null : src.Locations.Ward.Id)
            .Map(dest => dest.StreetAddress, src => src.StreetAddress.NormalizeOptional())
            .Map(dest => dest.FormattedAddress, src => src.FormattedAddress.NormalizeOptional())
            .Map(dest => dest.TotalFloors, src => GetTotalFloors(src.StructureSetup))
            .Map(dest => dest.TotalUnits, src => GetTotalUnits(src.StructureSetup))
            .Map(dest => dest.StatusId, src => src.MasterData.RequireValue(MASTER_TYPE_PROPERTY_STATUS, MASTER_CODE_PROPERTY_STATUS_DRAFT).Id)
            .Map(dest => dest.IsPublished, src => false);

        config.NewConfig<PropertyUnitBuildModel, Unit>()
            .Map(dest => dest.PublicId, src => Guid.NewGuid())
            .Ignore(dest => dest.Property)
            .Map(dest => dest.UnitCode, src => src.UnitCode)
            .Map(dest => dest.UnitName, src => src.UnitName.NormalizeOptional() ?? $"Phòng {src.UnitCode}")
            .Map(dest => dest.UnitTypeId, src => src.UnitTypeId)
            .Map(dest => dest.RentalModeId, src => src.RentalModeId)
            .Map(dest => dest.StatusId, src => src.StatusId)
            .Map(dest => dest.FloorNumber, src => src.FloorNumber)
            .Map(dest => dest.AreaSqm, src => src.AreaSqm)
            .Map(dest => dest.BedCount, src => src.BedCount)
            .Map(dest => dest.IsPetAllowed, src => src.IsPetAllowed)
            .Map(dest => dest.BaseRentAmount, src => src.BaseRentAmount)
            .Map(dest => dest.DefaultDepositAmount, src => src.DefaultDepositAmount)
            .Map(dest => dest.IsPublished, src => false)
            .AfterMapping((src, dest) => dest.Property = src.Property);

        config.NewConfig<PropertyUnitPackageBuildModel, UnitPackage>()
            .Map(dest => dest.PublicId, src => Guid.NewGuid())
            .Ignore(dest => dest.Unit)
            .Map(dest => dest.PackageTypeId, src => src.PackageType.Id)
            .Map(dest => dest.PackageCode, src => src.PackageCode)
            .Map(dest => dest.PackageName, src => src.PackageName.NormalizeOptional())
            .Map(dest => dest.Description, src => src.Description.NormalizeOptional())
            .Map(dest => dest.PriceAdjustment, src => src.PriceAdjustment)
            .Map(dest => dest.StatusId, src => src.Status.Id)
            .AfterMapping((src, dest) => dest.Unit = src.Unit);

        config.NewConfig<PropertyUnitPackageItemBuildModel, UnitPackageItem>()
            .Map(dest => dest.PublicId, src => Guid.NewGuid())
            .Ignore(dest => dest.UnitPackage)
            .Map(dest => dest.ItemName, src => src.ItemName.NormalizeOptional())
            .Map(dest => dest.DisplayOrder, src => src.DisplayOrder)
            .AfterMapping((src, dest) => dest.UnitPackage = src.UnitPackage);

        config.NewConfig<PropertyPartyBuildModel, PropertyParty>()
            .Map(dest => dest.PublicId, src => Guid.NewGuid())
            .Ignore(dest => dest.Property)
            .Map(dest => dest.PartyId, src => src.CurrentParty.PartyId)
            .Map(dest => dest.RelationshipTypeId, src => src.RelationshipTypeId)
            .Map(dest => dest.StartDate, src => DateOnly.FromDateTime(DateTime.UtcNow))
            .AfterMapping((src, dest) => dest.Property = src.Property);

        config.NewConfig<PropertyChargePolicyBuildModel, RentalChargePolicy>()
            .Map(dest => dest.PublicId, src => Guid.NewGuid())
            .Ignore(dest => dest.Property)
            .Map(dest => dest.LineTypeId, src => src.ChargeType.Id)
            .Map(dest => dest.VehicleTypeId, src => src.VehicleTypeId)
            .Map(dest => dest.ChargeName, src => string.IsNullOrWhiteSpace(src.Policy.Name) ? src.ChargeType.Name : src.Policy.Name.Trim())
            .Map(
                dest => dest.IsUsageBased,
                src => string.Equals(
                    src.Policy.CalculationMethodCode,
                    CALCULATION_METHOD_METER_READING,
                    StringComparison.OrdinalIgnoreCase))
            .Map(dest => dest.Amount, src => src.Policy.Amount)
            .Map(dest => dest.StatusId, src => src.ActiveStatusId)
            .AfterMapping((src, dest) => dest.Property = src.Property);

    }

    private static PropertyCreationGraphModel BuildCreationGraph(PropertyCreationRequestModel request, TypeAdapterConfig config)
    {
        // A single parent property instance must be reused across the graph so EF persists one aggregate.
        var property = request.Adapt<Property>(config);
        var units = BuildUnits(request, property, config);
        var unitPackages = BuildUnitPackages(request, units, config);

        return new PropertyCreationGraphModel
        {
            Property = property,
            Units = units,
            UnitPackages = unitPackages,
            UnitPackageItems = BuildUnitPackageItems(request, unitPackages, config),
            PropertyParty = BuildPropertyParty(request, property, config),
            ChargePolicies = BuildChargePolicies(request, property, config)
        };
    }

    private static IReadOnlyList<Unit> BuildUnits(
        PropertyCreationRequestModel request,
        Property property,
        TypeAdapterConfig config)
    {
        var structure = request.StructureSetup;
        var unitStatusId = request.MasterData.RequireValue(MASTER_TYPE_UNIT_STATUS, MASTER_CODE_UNIT_STATUS_AVAILABLE).Id;

        // Generate build models first so Mapster owns the Unit entity projection for both setup modes.
        var buildModels = HasExplicitStructure(structure)
            ? BuildExplicitUnitModels(request, property, unitStatusId)
            : BuildQuickUnitModels(request, property, unitStatusId);

        return buildModels.Adapt<List<Unit>>(config);
    }

    private static IReadOnlyList<PropertyUnitBuildModel> BuildQuickUnitModels(
        PropertyCreationRequestModel request,
        Property property,
        long unitStatusId)
    {
        var structure = request.StructureSetup;
        var unitTypeId = request.MasterData.RequireValue(MASTER_TYPE_UNIT_TYPE, structure.DefaultUnitTypeCode).Id;
        var rentalModeId = request.MasterData.RequireValue(MASTER_TYPE_UNIT_RENTAL_MODE, structure.DefaultRentalModeCode).Id;

        // Quick setup expands a uniform floor x room grid from default values.
        return Enumerable.Range(1, structure.TotalFloors)
            .SelectMany(floor => Enumerable.Range(1, structure.RoomsPerFloor)
                .Select(room => new PropertyUnitBuildModel
                {
                    Property = property,
                    FloorNumber = floor,
                    UnitCode = PropertyUnitCodeHelper.Generate(structure.RoomNumberingPattern, floor, room),
                    UnitTypeId = unitTypeId,
                    RentalModeId = rentalModeId,
                    StatusId = unitStatusId,
                    AreaSqm = structure.DefaultAreaSqm,
                    BaseRentAmount = structure.DefaultBaseRentAmount ?? 0,
                    DefaultDepositAmount = structure.DefaultDepositAmount ?? 0,
                    BedCount = GetSharedBedCount(structure.DefaultRentalModeCode, structure.DefaultTotalBeds),
                    IsPetAllowed = structure.DefaultIsPetAllowed
                }))
            .ToList();
    }

    private static IReadOnlyList<PropertyUnitBuildModel> BuildExplicitUnitModels(
        PropertyCreationRequestModel request,
        Property property,
        long unitStatusId)
    {
        // Explicit setup trusts the FE-composed room list after validator and master-data checks.
        return request.StructureSetup.Floors
            .SelectMany(floor => (floor.Rooms ?? []).Select(room => new PropertyUnitBuildModel
            {
                Property = property,
                FloorNumber = floor.FloorNumber,
                UnitCode = room.Code,
                UnitName = room.Name,
                UnitTypeId = request.MasterData.RequireValue(MASTER_TYPE_UNIT_TYPE, room.TypeCode).Id,
                RentalModeId = request.MasterData.RequireValue(MASTER_TYPE_UNIT_RENTAL_MODE, room.RentalModeCode).Id,
                StatusId = unitStatusId,
                AreaSqm = room.AreaSqm,
                BaseRentAmount = room.BaseRentAmount ?? 0,
                DefaultDepositAmount = room.DefaultDepositAmount ?? 0,
                BedCount = GetSharedBedCount(room.RentalModeCode, room.TotalBeds),
                IsPetAllowed = room.IsPetAllowed
            }))
            .ToList();
    }

    private static IReadOnlyList<UnitPackage> BuildUnitPackages(
        PropertyCreationRequestModel request,
        IReadOnlyList<Unit> units,
        TypeAdapterConfig config)
    {
        var defaultPackageType = request.MasterData.RequireValue(
            MASTER_TYPE_UNIT_PACKAGE_TYPE,
            MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE);
        var packageStatus = request.MasterData.RequireValue(
            MASTER_TYPE_UNIT_PACKAGE_STATUS,
            MASTER_CODE_UNIT_PACKAGE_STATUS_ACTIVE);
        var packageBuildModels = new List<PropertyUnitPackageBuildModel>();

        // Each generated room gets the zero-price default package instead of a null package selection.
        packageBuildModels.AddRange(units.Select(unit => new PropertyUnitPackageBuildModel
            {
                Unit = unit,
                PackageType = defaultPackageType,
                Status = packageStatus,
                PackageCode = defaultPackageType.Code,
                PackageName = defaultPackageType.Name,
                Description = defaultPackageType.Description,
                PriceAdjustment = 0
            }));

        var requestedPackages = request.Packages ?? [];
        if (requestedPackages.Count > 0)
        {
            var customPackageType = request.MasterData.RequireValue(
                MASTER_TYPE_UNIT_PACKAGE_TYPE,
                MASTER_CODE_UNIT_PACKAGE_TYPE_CUSTOM);

            packageBuildModels.AddRange(units.SelectMany(unit => requestedPackages.Select(
                (package, index) => new PropertyUnitPackageBuildModel
                {
                    Unit = unit,
                    PackageType = customPackageType,
                    Status = packageStatus,
                    PackageCode = BuildPackageCode(index),
                    PackageName = package.Name,
                    PriceAdjustment = package.PriceAdjustment
                })));
        }

        return packageBuildModels.Adapt<List<UnitPackage>>(config);
    }

    private static IReadOnlyList<UnitPackageItem> BuildUnitPackageItems(
        PropertyCreationRequestModel request,
        IReadOnlyList<UnitPackage> unitPackages,
        TypeAdapterConfig config)
    {
        var requestedPackages = request.Packages ?? [];
        if (requestedPackages.Count == 0)
        {
            return [];
        }

        var itemsByPackageCode = requestedPackages
            .Select((package, index) => new
            {
                PackageCode = BuildPackageCode(index),
                Items = package.Items ?? []
            })
            .ToDictionary(x => x.PackageCode, x => x.Items);

        // Package item rows belong only to user-defined packages, not the default NO_FURNITURE package.
        return unitPackages
            .Where(package => itemsByPackageCode.ContainsKey(package.PackageCode))
            .SelectMany(package => itemsByPackageCode[package.PackageCode]
                .Select((item, index) => new PropertyUnitPackageItemBuildModel
                {
                    UnitPackage = package,
                    ItemName = item.Name,
                    DisplayOrder = index + 1
                }))
            .Adapt<List<UnitPackageItem>>(config);
    }

    private static PropertyParty BuildPropertyParty(
        PropertyCreationRequestModel request,
        Property property,
        TypeAdapterConfig config)
    {
        // Relationship type is resolved once from master data and mapped into the link entity.
        return new PropertyPartyBuildModel
        {
            Property = property,
            CurrentParty = request.CurrentParty,
            RelationshipTypeId = request.MasterData.RequireValue(
                MASTER_TYPE_PROPERTY_RELATIONSHIP_TYPE,
                PropertyRelationshipTypeEnum.Landlord.ToMasterDataCode()).Id
        }.Adapt<PropertyParty>(config);
    }

    private static IReadOnlyList<RentalChargePolicy> BuildChargePolicies(
        PropertyCreationRequestModel request,
        Property property,
        TypeAdapterConfig config)
    {
        var activeStatusId = request.MasterData.RequireValue(MASTER_TYPE_COMMON_STATUS, MASTER_CODE_ACTIVE).Id;

        // Charge policy build models carry resolved lookup values so Mapster can project entities cleanly.
        return (request.ChargePolicies ?? [])
            .Select(policy =>
            {
                var chargeType = request.MasterData.RequireValue(MASTER_TYPE_INVOICE_LINE_TYPE, policy.Code);
                var isParkingPolicy = string.Equals(
                    policy.Code,
                    MASTER_CODE_INVOICE_LINE_TYPE_PARKING,
                    StringComparison.OrdinalIgnoreCase);
                var vehicleTypeId = isParkingPolicy
                    ? request.MasterData.RequireValue(MASTER_TYPE_VEHICLE_TYPE, policy.VehicleTypeCode).Id
                    : (long?)null;

                return new PropertyChargePolicyBuildModel
                {
                    Property = property,
                    Policy = policy,
                    ChargeType = chargeType,
                    VehicleTypeId = vehicleTypeId,
                    ActiveStatusId = activeStatusId
                };
            })
            .Adapt<List<RentalChargePolicy>>(config);
    }

    private static string BuildPackageCode(int index)
    {
        return $"{UNIT_PACKAGE_CODE_PREFIX}-{index + 1:000}";
    }

    private static string BuildAddressText(PropertyRowModel row)
    {
        if (!string.IsNullOrWhiteSpace(row.FormattedAddress))
        {
            return row.FormattedAddress;
        }

        // List cards need one readable address string, not the full editable address object.
        return string.Join(
            ", ",
            new[]
            {
                row.StreetAddress,
                row.WardName,
                row.DistrictName,
                row.ProvinceName
            }.Where(value => !string.IsNullOrWhiteSpace(value)));
    }

    private static int GetTotalFloors(CreatePropertyStructureRequestDto structure)
    {
        return HasExplicitStructure(structure)
            ? structure.Floors.Select(floor => floor.FloorNumber).Distinct().Count()
            : structure.TotalFloors;
    }

    private static int GetTotalUnits(CreatePropertyStructureRequestDto structure)
    {
        return HasExplicitStructure(structure)
            ? structure.Floors.Sum(floor => (floor.Rooms ?? []).Count)
            : structure.TotalFloors * structure.RoomsPerFloor;
    }

    /// <summary>
    /// Returns bed capacity only for shared-bed/KTX rental rooms.
    /// </summary>
    /// <param name="rentalModeCode">The room rental mode code.</param>
    /// <param name="totalBeds">The requested total bed capacity.</param>
    /// <returns>The requested bed capacity for shared-bed rooms; otherwise <c>null</c>.</returns>
    private static int? GetSharedBedCount(string rentalModeCode, int? totalBeds)
    {
        // Bed capacity belongs only to KTX/shared-bed rooms in the property creation flow.
        return string.Equals(
            rentalModeCode,
            MASTER_CODE_RENTAL_MODE_SHARED_BED,
            StringComparison.OrdinalIgnoreCase)
            ? totalBeds
            : null;
    }

    private static bool HasExplicitStructure(CreatePropertyStructureRequestDto structure)
    {
        return (structure.Floors ?? []).Count > 0;
    }
}
