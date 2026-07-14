namespace Haven.Application.Mappings.Properties;

/// <summary>
/// Registers Mapster projections used to build a new property aggregate.
/// </summary>
public sealed class PropertyCreationMapping : IRegister
{
    /// <summary>
    /// Registers entity projections for the validated property creation graph.
    /// </summary>
    /// <param name="config">The Mapster configuration instance.</param>
    public void Register(TypeAdapterConfig config)
    {
        // Creation graph mapping keeps lookup resolution and generated identifiers in prepared models.
        config.NewConfig<PropertyCreationGraphInputModel, PropertyCreationGraphModel>()
            .MapWith(src => BuildCreationGraph(src, config));

        config.NewConfig<PropertyCreationGraphInputModel, Property>()
            .Map(dest => dest.PublicId, src => Guid.NewGuid())
            .Map(dest => dest.PropertyCode, src => src.Request.PropertyCode)
            .Map(dest => dest.Name, src => src.Request.Name)
            .Map(dest => dest.PropertyTypeId, src => src.Lookups.PropertyTypeId)
            .Map(dest => dest.ProvinceId, src => src.Request.Locations.Province == null ? (long?)null : src.Request.Locations.Province.Id)
            .Map(dest => dest.DistrictId, src => src.Request.Locations.District == null ? (long?)null : src.Request.Locations.District.Id)
            .Map(dest => dest.WardId, src => src.Request.Locations.Ward == null ? (long?)null : src.Request.Locations.Ward.Id)
            .Map(dest => dest.StreetAddress, src => src.Request.StreetAddress)
            .Map(dest => dest.FormattedAddress, src => src.Request.FormattedAddress)
            .Map(dest => dest.TotalFloors, src => src.Request.StructureSetup.Floors.Select(floor => floor.FloorNumber).Distinct().Count())
            .Map(dest => dest.TotalUnits, src => src.Request.StructureSetup.Floors.Sum(floor => (floor.Rooms ?? Array.Empty<CreatePropertyRoomRequestDto>()).Count))
            .Map(dest => dest.StatusId, src => src.Lookups.PropertyStatusId)
            .Map(dest => dest.IsPublished, src => false);

        config.NewConfig<PropertyUnitBuildModel, Unit>()
            .Map(dest => dest.PublicId, src => Guid.NewGuid())
            .Ignore(dest => dest.Property)
            .Map(dest => dest.UnitCode, src => src.UnitCode)
            .Map(dest => dest.UnitName, src => src.UnitName)
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
            .Ignore(dest => dest.Property)
            .Ignore(dest => dest.Unit)
            .Map(dest => dest.PackageTypeId, src => src.PackageType.Id)
            .Map(dest => dest.PackageCode, src => src.PackageCode)
            .Map(dest => dest.PackageName, src => src.PackageName)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.PriceAdjustment, src => src.PriceAdjustment)
            .Map(dest => dest.StatusId, src => src.Status.Id)
            .AfterMapping((src, dest) =>
            {
                dest.Property = src.Property;
                dest.Unit = src.Unit;
            });

        config.NewConfig<PropertyUnitPackageItemBuildModel, UnitPackageItem>()
            .Map(dest => dest.PublicId, src => Guid.NewGuid())
            .Ignore(dest => dest.UnitPackage)
            .Map(dest => dest.ItemName, src => src.ItemName)
            .Map(dest => dest.DisplayOrder, src => src.DisplayOrder)
            .AfterMapping((src, dest) => dest.UnitPackage = src.UnitPackage);

        config.NewConfig<PropertyPartyBuildModel, PropertyParty>()
            .Ignore(dest => dest.PublicId)
            .Ignore(dest => dest.Property)
            .Map(dest => dest.PartyId, src => src.CurrentParty.PartyId)
            .Map(dest => dest.RelationshipTypeId, src => src.RelationshipTypeId)
            .Ignore(dest => dest.StartDate)
            .AfterMapping((src, dest) =>
            {
                dest.PublicId = Guid.NewGuid();
                dest.Property = src.Property;
                dest.StartDate = DateOnly.FromDateTime(DateTime.UtcNow);
            });

        config.NewConfig<PropertyChargePolicyBuildModel, RentalChargePolicy>()
            .Map(dest => dest.PublicId, src => Guid.NewGuid())
            .Ignore(dest => dest.Property)
            .Map(dest => dest.LineTypeId, src => src.ChargeType.Id)
            .Map(dest => dest.VehicleTypeId, src => src.VehicleTypeId)
            .Map(dest => dest.ChargeName, src => src.Policy.Name ?? src.ChargeType.Name)
            .Map(
                dest => dest.IsUsageBased,
                src => string.Equals(
                    src.Policy.CalculationMethodCode,
                    CALCULATION_METHOD_METER_READING,
                    StringComparison.OrdinalIgnoreCase))
            .Map(dest => dest.Amount, src => src.Policy.Amount.GetValueOrDefault())
            .Map(dest => dest.StatusId, src => src.ActiveStatusId)
            .AfterMapping((src, dest) => dest.Property = src.Property);
    }

    /// <summary>
    /// Builds the full property aggregate draft from the validated create payload.
    /// </summary>
    /// <param name="input">The request and lookup data needed to build the graph.</param>
    /// <param name="config">The Mapster configuration instance used for entity projections.</param>
    /// <returns>The draft property graph that will be persisted in one transaction.</returns>
    private static PropertyCreationGraphModel BuildCreationGraph(
        PropertyCreationGraphInputModel input,
        TypeAdapterConfig config)
    {
        var request = input.Request;

        // Reuse one property instance so EF persists one aggregate root and its children.
        var property = input.Adapt<Property>(config);
        property.TotalFloors = request.StructureSetup.Floors.Select(floor => floor.FloorNumber).Distinct().Count();
        property.TotalUnits = request.StructureSetup.Floors.Sum(floor => (floor.Rooms ?? Array.Empty<CreatePropertyRoomRequestDto>()).Count);

        var units = BuildUnits(input, property, config);
        var unitPackages = BuildUnitPackages(input, property, config);

        // Assemble the prepared child collections after each mapping has resolved its own IDs.
        return new PropertyCreationGraphModel
        {
            Property = property,
            Units = units,
            UnitPackages = unitPackages,
            UnitPackageItems = BuildUnitPackageItems(input.Request, unitPackages, config),
            PropertyParty = BuildPropertyParty(input, property),
            ChargePolicies = BuildChargePolicies(input, property, config)
        };
    }

    /// <summary>
    /// Builds room/unit entities from the final floor and room structure supplied by FE.
    /// </summary>
    /// <param name="input">The request and resolved lookup identifiers.</param>
    /// <param name="property">The parent property entity shared by all units.</param>
    /// <param name="config">The Mapster configuration instance used for entity projections.</param>
    /// <returns>The unit entities created from the submitted rooms.</returns>
    private static IReadOnlyList<Unit> BuildUnits(
        PropertyCreationGraphInputModel input,
        Property property,
        TypeAdapterConfig config)
    {
        // FE owns room display names; the backend owns unique business codes.
        var buildModels = BuildUnitModels(input, property);

        return buildModels.Adapt<List<Unit>>(config);
    }

    /// <summary>
    /// Builds intermediate unit models from submitted rooms, resolved IDs, and generated business codes.
    /// </summary>
    /// <param name="input">The request and lookup data needed for room creation.</param>
    /// <param name="property">The parent property entity shared by all rooms.</param>
    /// <returns>The room build models ready for Mapster projection.</returns>
    private static IReadOnlyList<PropertyUnitBuildModel> BuildUnitModels(
        PropertyCreationGraphInputModel input,
        Property property)
    {
        var request = input.Request;
        var lookups = input.Lookups;
        var usedUnitCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Quick setup and numbering remain FE concerns; only the final room names reach this model.
        return request.StructureSetup.Floors
            .SelectMany(floor => (floor.Rooms ?? []).Select(room => new PropertyUnitBuildModel
            {
                Property = property,
                FloorNumber = floor.FloorNumber,
                UnitCode = GenerateUnitCode(usedUnitCodes),
                UnitName = room.Name,
                UnitTypeId = lookups.UnitTypeIds[room.TypeCode],
                RentalModeId = lookups.RentalModeIds[room.RentalModeCode],
                StatusId = lookups.UnitStatusId,
                AreaSqm = room.AreaSqm,
                BaseRentAmount = room.BaseRentAmount ?? 0,
                DefaultDepositAmount = room.DefaultDepositAmount ?? 0,
                BedCount = string.Equals(
                    room.RentalModeCode,
                    MASTER_CODE_RENTAL_MODE_SHARED_BED,
                    StringComparison.OrdinalIgnoreCase)
                    ? room.TotalBeds
                    : null,
                IsPetAllowed = room.IsPetAllowed
            }))
            .ToList();
    }

    /// <summary>
    /// Generates one unique room business code within the new in-memory property graph.
    /// </summary>
    /// <param name="usedUnitCodes">The generated codes already assigned inside the graph.</param>
    /// <returns>The generated room/unit business code.</returns>
    private static string GenerateUnitCode(ISet<string> usedUnitCodes)
    {
        // A new property has no persisted room rows, so uniqueness is scoped to this graph.
        for (var attempt = 0; attempt < ROOM_CODE_MAX_ATTEMPTS; attempt++)
        {
            var candidate = CodeGenerationHelper.GenerateCode(ROOM_CODE_PREFIX, ROOM_CODE_RANDOM_LENGTH);

            if (usedUnitCodes.Add(candidate))
            {
                return candidate;
            }
        }

        throw new ApiException(ApplicationErrorConstants.RoomErrors.ERROR_ROOM_CODE_GENERATION_FAILED, BAD_REQUEST);
    }

    /// <summary>
    /// Builds property-level common package templates for the new property.
    /// </summary>
    /// <param name="input">The request and package lookup values.</param>
    /// <param name="property">The parent property entity.</param>
    /// <param name="config">The Mapster configuration instance used for entity projections.</param>
    /// <returns>The property-level package templates.</returns>
    private static IReadOnlyList<UnitPackage> BuildUnitPackages(
        PropertyCreationGraphInputModel input,
        Property property,
        TypeAdapterConfig config)
    {
        var request = input.Request;
        var lookups = input.Lookups;
        List<PropertyUnitPackageBuildModel> packageBuildModels =
        [
            new()
            {
                Property = property,
                PackageType = lookups.NoFurniturePackageType,
                Status = lookups.PackageStatus,
                PackageCode = lookups.NoFurniturePackageType.Code,
                PackageName = lookups.NoFurniturePackageType.Name,
                Description = lookups.NoFurniturePackageType.Description,
                PriceAdjustment = 0
            }
        ];

        // Add requested common packages beside the mandatory NO_FURNITURE fallback.
        var requestedPackages = request.Packages ?? [];
        packageBuildModels.AddRange(requestedPackages.Select(
            package => new PropertyUnitPackageBuildModel
            {
                Property = property,
                PackageType = lookups.CustomPackageType,
                Status = lookups.PackageStatus,
                PackageCode = CodeGenerationHelper.GenerateCode(UNIT_PACKAGE_CODE_PREFIX, UNIT_PACKAGE_CODE_RANDOM_LENGTH),
                PackageName = package.Name,
                PriceAdjustment = package.PriceAdjustment
            }));

        return packageBuildModels.Adapt<List<UnitPackage>>(config);
    }

    /// <summary>
    /// Builds item rows for each requested custom package template.
    /// </summary>
    /// <param name="request">The create-property request containing package item labels.</param>
    /// <param name="unitPackages">The created package template entities.</param>
    /// <param name="config">The Mapster configuration instance used for entity projections.</param>
    /// <returns>The created package item entities.</returns>
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

        var customPackages = unitPackages
            .Where(package => !string.Equals(
                package.PackageCode,
                MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE,
                StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Item rows belong only to user-defined packages, not the NO_FURNITURE fallback.
        return customPackages
            .Zip(requestedPackages)
            .SelectMany(pair => (pair.Second.Items ?? [])
                .Select((item, index) => new PropertyUnitPackageItemBuildModel
                {
                    UnitPackage = pair.First,
                    ItemName = item.Name,
                    DisplayOrder = index + 1
                }))
            .Adapt<List<UnitPackageItem>>(config);
    }

    /// <summary>
    /// Builds the landlord relationship link for the newly created property.
    /// </summary>
    /// <param name="input">The request and relationship lookup data.</param>
    /// <param name="property">The parent property entity.</param>
    /// <returns>The created property-party link entity.</returns>
    private static PropertyParty BuildPropertyParty(
        PropertyCreationGraphInputModel input,
        Property property)
    {
        // The link is a small aggregate edge, so construct it directly with resolved IDs.
        return new PropertyParty
        {
            PublicId = Guid.NewGuid(),
            Property = property,
            PartyId = input.Request.CurrentParty.PartyId,
            RelationshipTypeId = input.Lookups.LandlordRelationshipTypeId,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };
    }

    /// <summary>
    /// Builds property-level common charge policies from the setup payload.
    /// </summary>
    /// <param name="input">The request and resolved lookup values.</param>
    /// <param name="property">The parent property entity.</param>
    /// <param name="config">The Mapster configuration instance used for entity projections.</param>
    /// <returns>The created rental charge policy entities.</returns>
    private static IReadOnlyList<RentalChargePolicy> BuildChargePolicies(
        PropertyCreationGraphInputModel input,
        Property property,
        TypeAdapterConfig config)
    {
        var request = input.Request;
        var lookups = input.Lookups;

        // Prepared lookup IDs let Mapster project the entity without repository calls.
        return (request.ChargePolicies ?? [])
            .Select(policy =>
            {
                var chargeType = lookups.ChargeTypes[policy.Code];
                var isParkingPolicy = string.Equals(
                    policy.Code,
                    MASTER_CODE_INVOICE_LINE_TYPE_PARKING,
                    StringComparison.OrdinalIgnoreCase);
                var vehicleTypeId = isParkingPolicy
                    ? lookups.VehicleTypeIds[policy.VehicleTypeCode]
                    : (long?)null;

                return new PropertyChargePolicyBuildModel
                {
                    Property = property,
                    Policy = policy,
                    ChargeType = chargeType,
                    VehicleTypeId = vehicleTypeId,
                    ActiveStatusId = lookups.ActiveStatusId
                };
            })
            .Adapt<List<RentalChargePolicy>>(config);
    }
}
