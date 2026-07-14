using Haven.Application.Dtos.Properties.Create;
using Haven.Application.Dtos.Properties.Detail;
using Haven.Application.Dtos.Properties.List;
using Haven.Application.Dtos.Properties.Update;
using Haven.Application.Mappings.Properties;
using Haven.Application.Mappings.Rooms;
using Haven.Application.Mappings.Tenants;
using Haven.Application.Models.Locations;
using Haven.Application.Models.MasterData;
using Haven.Application.Models.Parties;
using Haven.Application.Models.Properties.Create;
using Haven.Application.Models.Properties.Detail;
using Haven.Application.Models.Properties.List;
using Haven.Application.Models.Properties.QueryParameters;
using Haven.Application.Models.Properties.Rows;
using Haven.Application.Models.Properties.Update;
using Haven.Domain.Entities;
using Haven.Domain.Enums;
using static Haven.Application.Constants.ApplicationConstants;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Mappings;

public sealed class PropertyMappingConfigurationTests
{
    private readonly TypeAdapterConfig _config = BuildConfig();

    [Fact]
    public void PropertyUpdateRequest_Should_MapSubmittedScalarsAndResolvedForeignKeys()
    {
        var property = new Property
        {
            PublicId = Guid.NewGuid(),
            PropertyCode = "PROP_1234567890",
            Name = "Tên cũ",
            PropertyTypeId = 1,
            ProvinceId = 2,
            DistrictId = 3,
            WardId = 4,
            StreetAddress = "Địa chỉ cũ",
            FormattedAddress = "Địa chỉ cũ đầy đủ",
            Latitude = 10,
            Longitude = 106,
            TotalFloors = 5,
            TotalUnits = 20,
            StatusId = 6,
            IsPublished = true
        };
        var request = new PropertyUpdateRequestModel
        {
            Name = "Tên mới",
            PropertyTypeId = 11,
            ProvinceId = 12,
            DistrictId = 13,
            WardId = 14,
            StreetAddress = "Địa chỉ mới",
            FormattedAddress = "Địa chỉ mới đầy đủ",
            Latitude = 11,
            Longitude = 107
        };

        request.Adapt(property, _config);

        property.Name.Should().Be("Tên mới");
        property.PropertyTypeId.Should().Be(11);
        property.ProvinceId.Should().Be(12);
        property.DistrictId.Should().Be(13);
        property.WardId.Should().Be(14);
        property.StreetAddress.Should().Be("Địa chỉ mới");
        property.FormattedAddress.Should().Be("Địa chỉ mới đầy đủ");
        property.Latitude.Should().Be(11);
        property.Longitude.Should().Be(107);
        property.PropertyCode.Should().Be("PROP_1234567890");
        property.TotalFloors.Should().Be(5);
        property.TotalUnits.Should().Be(20);
        property.StatusId.Should().Be(6);
        property.IsPublished.Should().BeTrue();
    }

    [Fact]
    public void PropertyUpdateRequest_Should_PreserveExistingValues_WhenFieldsAreOmitted()
    {
        var property = new Property
        {
            Name = "Tên hiện tại",
            PropertyTypeId = 21,
            ProvinceId = 22,
            DistrictId = 23,
            WardId = 24,
            StreetAddress = "Địa chỉ hiện tại",
            FormattedAddress = "Địa chỉ hiện tại đầy đủ",
            Latitude = 10,
            Longitude = 106
        };

        new PropertyUpdateRequestModel().Adapt(property, _config);

        property.Name.Should().Be("Tên hiện tại");
        property.PropertyTypeId.Should().Be(21);
        property.ProvinceId.Should().Be(22);
        property.DistrictId.Should().Be(23);
        property.WardId.Should().Be(24);
        property.StreetAddress.Should().Be("Địa chỉ hiện tại");
        property.FormattedAddress.Should().Be("Địa chỉ hiện tại đầy đủ");
        property.Latitude.Should().Be(10);
        property.Longitude.Should().Be(106);
    }

    [Fact]
    public void PropertyLocationContext_Should_MapResolvedIdsToUpdateRequest()
    {
        var locations = new PropertyLocationContextModel
        {
            Province = new LocationLookupModel { Id = 31 },
            District = new LocationLookupModel { Id = 32 },
            Ward = new LocationLookupModel { Id = 33 }
        };
        var request = new PropertyUpdateRequestModel();

        locations.Adapt(request, _config);

        request.ProvinceId.Should().Be(31);
        request.DistrictId.Should().Be(32);
        request.WardId.Should().Be(33);
    }

    [Fact]
    public void PropertyCreationRequest_Should_MapDraftPropertyFields()
    {
        var request = BuildCreationRequest();

        var property = BuildGraphInput(request).Adapt<Property>(_config);

        property.PublicId.Should().NotBeEmpty();
        property.PropertyCode.Should().Be(request.PropertyCode);
        property.Name.Should().Be(" Tòa nhà A ");
        property.PropertyTypeId.Should().Be(100);
        property.ProvinceId.Should().Be(10);
        property.DistrictId.Should().Be(20);
        property.WardId.Should().Be(30);
        property.StreetAddress.Should().Be(" 123 Nguyễn Văn Linh ");
        property.FormattedAddress.Should().Be(" 123 Nguyễn Văn Linh, Quận 1, TP.HCM ");
        property.TotalFloors.Should().Be(2);
        property.TotalUnits.Should().Be(4);
        property.StatusId.Should().Be(101);
        property.IsPublished.Should().BeFalse();
    }

    [Fact]
    public void PropertyUnitBuildModel_Should_MapGeneratedUnitDefaults()
    {
        var property = new Property();
        var model = new PropertyUnitBuildModel
        {
            Property = property,
            FloorNumber = 2,
            UnitCode = "201",
            UnitTypeId = 102,
            RentalModeId = 103,
            StatusId = 104,
            AreaSqm = 25,
            BaseRentAmount = 5_500_000,
            DefaultDepositAmount = 5_500_000,
            BedCount = 1,
            IsPetAllowed = true
        };

        var unit = model.Adapt<Unit>(_config);

        unit.PublicId.Should().NotBeEmpty();
        unit.Property.Should().BeSameAs(property);
        unit.UnitCode.Should().Be("201");
        unit.UnitName.Should().BeNull();
        unit.UnitTypeId.Should().Be(102);
        unit.RentalModeId.Should().Be(103);
        unit.StatusId.Should().Be(104);
        unit.FloorNumber.Should().Be(2);
        unit.AreaSqm.Should().Be(25);
        unit.BaseRentAmount.Should().Be(5_500_000);
        unit.DefaultDepositAmount.Should().Be(5_500_000);
        unit.BedCount.Should().Be(1);
        unit.IsPetAllowed.Should().BeTrue();
        unit.IsPublished.Should().BeFalse();
    }

    [Fact]
    public void PropertyCreationRequest_Should_MapCreationGraphWithSharedParent()
    {
        var request = BuildCreationRequest();
        request.ChargePolicies =
        [
            new CreatePropertyChargePolicyRequestDto
            {
                Code = "ELECTRIC",
                Name = " Tiền điện ",
                Amount = 3500,
                CalculationMethodCode = CALCULATION_METHOD_METER_READING
            }
        ];
        request.Packages =
        [
            new CreatePropertyPackageRequestDto
            {
                Name = " Gói Full Nội thất ",
                PriceAdjustment = 1_500_000,
                Items =
                [
                    new CreatePropertyPackageItemRequestDto { Name = " Giường ngủ King Size " },
                    new CreatePropertyPackageItemRequestDto { Name = " Tủ lạnh 200L " }
                ]
            }
        ];

        var graph = BuildGraphInput(request).Adapt<PropertyCreationGraphModel>(_config);

        graph.Property.Should().NotBeNull();
        graph.Units.Should().HaveCount(4);
        graph.Units.Should().OnlyContain(unit => unit.Property == graph.Property);
        graph.UnitPackages.Should().HaveCount(2);
        graph.UnitPackages.Count(package => package.PackageCode == MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE).Should().Be(1);
        var customPackage = graph.UnitPackages.Single(package => package.PackageCode != MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE);
        customPackage.PackageCode.Should().MatchRegex("^PKG_\\d+$");
        graph.UnitPackages.Should().OnlyContain(package => package.Property == graph.Property && package.Unit == null && package.UnitId == null);
        customPackage.PackageName.Should().Be(" Gói Full Nội thất ");
        customPackage.PriceAdjustment.Should().Be(1_500_000);
        graph.UnitPackageItems.Should().HaveCount(2);
        graph.UnitPackageItems.Should().OnlyContain(item => item.UnitPackage == customPackage);
        graph.UnitPackageItems.Select(item => item.ItemName).Should().Contain(" Giường ngủ King Size ");
        graph.UnitPackageItems.Select(item => item.ItemName).Should().Contain(" Tủ lạnh 200L ");
        graph.PropertyParty.Property.Should().BeSameAs(graph.Property);
        graph.ChargePolicies.Should().ContainSingle();
        graph.ChargePolicies[0].Property.Should().BeSameAs(graph.Property);
        graph.ChargePolicies[0].LineTypeId.Should().Be(107);
    }

    [Fact]
    public void PropertyCreationRequest_Should_MapExplicitRoomSetup()
    {
        var request = BuildCreationRequest();
        request.StructureSetup = new CreatePropertyStructureRequestDto
        {
            Floors =
            [
                new CreatePropertyFloorRequestDto
                {
                    FloorNumber = 1,
                    Rooms =
                    [
                        new CreatePropertyRoomRequestDto
                        {
                            Name = "Phòng A101",
                            TypeCode = MASTER_CODE_UNIT_TYPE_ROOM,
                            RentalModeCode = MASTER_CODE_RENTAL_MODE_SHARED_BED,
                            AreaSqm = 28,
                            BaseRentAmount = 6_000_000,
                            DefaultDepositAmount = 6_000_000,
                            TotalBeds = 2,
                            IsPetAllowed = true
                        }
                    ]
                },
                new CreatePropertyFloorRequestDto
                {
                    FloorNumber = 2,
                    Rooms =
                    [
                        new CreatePropertyRoomRequestDto
                        {
                            Name = "Phòng A201",
                            TypeCode = MASTER_CODE_UNIT_TYPE_ROOM,
                            RentalModeCode = MASTER_CODE_RENTAL_MODE_WHOLE_UNIT,
                            BaseRentAmount = 7_000_000
                        }
                    ]
                }
            ]
        };

        var graph = BuildGraphInput(request).Adapt<PropertyCreationGraphModel>(_config);

        graph.Property.TotalFloors.Should().Be(2);
        graph.Property.TotalUnits.Should().Be(2);
        var generatedUnitCodes = graph.Units.Select(unit => unit.UnitCode).ToArray();
        generatedUnitCodes.Should().OnlyContain(code => code.StartsWith($"{ROOM_CODE_PREFIX}_", StringComparison.Ordinal));
        generatedUnitCodes.Should().OnlyHaveUniqueItems();
        graph.Units[0].UnitName.Should().Be("Phòng A101");
        graph.Units[0].FloorNumber.Should().Be(1);
        graph.Units[0].AreaSqm.Should().Be(28);
        graph.Units[0].DefaultDepositAmount.Should().Be(6_000_000);
        graph.Units[0].BedroomCount.Should().BeNull();
        graph.Units[0].BathroomCount.Should().BeNull();
        graph.Units[0].BedCount.Should().Be(2);
        graph.Units[0].IsPetAllowed.Should().BeTrue();
        graph.Units[1].FloorNumber.Should().Be(2);
        graph.Units[1].BaseRentAmount.Should().Be(7_000_000);
        graph.Units[1].BedroomCount.Should().BeNull();
        graph.Units[1].BathroomCount.Should().BeNull();
        graph.Units[1].BedCount.Should().BeNull();
    }

    [Fact]
    public void PropertyCreationRequest_Should_MapLocationKey()
    {
        var request = BuildCreationRequest();

        var key = request.Adapt<LocationKeyModel>(_config);

        key.ProvinceCode.Should().Be("HCM");
        key.DistrictCode.Should().Be("D1");
        key.WardCode.Should().Be("W1");
    }

    [Fact]
    public void PropertyPackageUpdate_Should_MergeSubmittedFields_AndPreserveOmittedFields()
    {
        var package = new UnitPackage
        {
            PackageCode = "PKG_123456",
            PackageName = "Tên cũ",
            PriceAdjustment = 500_000,
            PackageTypeId = 10,
            StatusId = 20
        };
        var request = new UpdatePropertyPackageRequestDto
        {
            Id = Guid.NewGuid(),
            Name = "Tên mới"
        };

        request.Adapt(package, _config);

        package.PackageCode.Should().Be("PKG_123456");
        package.PackageName.Should().Be("Tên mới");
        package.PriceAdjustment.Should().Be(500_000);
        package.PackageTypeId.Should().Be(10);
        package.StatusId.Should().Be(20);
    }

    [Fact]
    public void PropertyListRequest_Should_MapQueryParametersWithNormalizedFilters()
    {
        var request = new PropertyListRequestModel
        {
            CurrentParty = new CurrentPartyContextModel { PartyId = 123 },
            Search = " toa ",
            PropertyTypeCode = " BUILDING ",
            StatusCode = " DRAFT ",
            FloorNumber = 2,
            RoomStatusCode = " AVAILABLE ",
            PaymentStatusCode = " PAID ",
            PageNumber = 3,
            PageSize = 20
        };

        var parameters = request.Adapt<PropertyListQueryParametersModel>(_config);

        parameters.CurrentPartyId.Should().Be(123);
        parameters.RelationshipCodes.Should().BeEquivalentTo(GetExpectedRelationshipCodes());
        parameters.Search.Should().Be("toa");
        parameters.PropertyTypeCode.Should().Be("BUILDING");
        parameters.StatusCode.Should().Be("DRAFT");
        parameters.FloorNumber.Should().Be(2);
        parameters.RoomStatusCode.Should().Be("AVAILABLE");
        parameters.PaymentStatusCode.Should().Be("PAID");
        parameters.PageSize.Should().Be(20);
        parameters.Offset.Should().Be(40);
    }

    [Fact]
    public void PropertyListRequest_Should_MapChildRowsParametersWithNormalizedFilters()
    {
        var request = new PropertyListRequestModel
        {
            Search = " phong ",
            FloorNumber = 5,
            RoomStatusCode = " OCCUPIED ",
            PaymentStatusCode = " UNPAID "
        };

        var parameters = request.Adapt<PropertyChildRowsQueryParametersModel>(_config);

        parameters.PropertyPublicIds.Should().BeEmpty();
        parameters.Search.Should().Be("phong");
        parameters.FloorNumber.Should().Be(5);
        parameters.RoomStatusCode.Should().Be("OCCUPIED");
        parameters.PaymentStatusCode.Should().Be("UNPAID");
    }

    [Fact]
    public void PropertyDetailRequest_Should_MapScopedQueryParameters()
    {
        var propertyPublicId = Guid.NewGuid();
        var request = new PropertyDetailRequestModel
        {
            CurrentParty = new CurrentPartyContextModel { PartyId = 456 },
            PropertyPublicId = propertyPublicId
        };

        var parameters = request.Adapt<PropertyScopedQueryParametersModel>(_config);

        parameters.CurrentPartyId.Should().Be(456);
        parameters.PropertyPublicId.Should().Be(propertyPublicId);
        parameters.RelationshipCodes.Should().BeEquivalentTo(GetExpectedRelationshipCodes());
    }

    [Fact]
    public void PropertyPartyBuildModel_Should_MapCurrentLandlordRelationship()
    {
        var property = new Property();
        var currentParty = new CurrentPartyContextModel
        {
            PartyId = 123,
            PartyPublicId = Guid.NewGuid()
        };
        var model = new PropertyPartyBuildModel
        {
            Property = property,
            CurrentParty = currentParty,
            RelationshipTypeId = 105
        };

        var propertyParty = model.Adapt<PropertyParty>(_config);

        propertyParty.PublicId.Should().NotBeEmpty();
        propertyParty.Property.Should().BeSameAs(property);
        propertyParty.PartyId.Should().Be(123);
        propertyParty.RelationshipTypeId.Should().Be(105);
        propertyParty.StartDate.Should().NotBeNull();
    }

    [Fact]
    public void PropertyUnitPackageBuildModel_Should_MapDefaultPackage()
    {
        var unit = new Unit();
        var model = new PropertyUnitPackageBuildModel
        {
            Unit = unit,
            PackageType = new MasterDataValueModel
            {
                Id = 108,
                Type = MasterDataTypeEnum.UnitPackageType.ToString(),
                Code = MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE,
                Name = "Không chọn gói",
                Description = "Gói mặc định giá 0."
            },
            Status = new MasterDataValueModel
            {
                Id = 109,
                Type = MasterDataTypeEnum.UnitPackageStatus.ToString(),
                Code = MASTER_CODE_UNIT_PACKAGE_STATUS_ACTIVE,
                Name = "Hoạt động"
            },
            PackageCode = MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE,
            PackageName = "Không chọn gói",
            Description = "Gói mặc định giá 0.",
            PriceAdjustment = 0
        };

        var unitPackage = model.Adapt<UnitPackage>(_config);

        unitPackage.PublicId.Should().NotBeEmpty();
        unitPackage.Unit.Should().BeSameAs(unit);
        unitPackage.PackageTypeId.Should().Be(108);
        unitPackage.PackageCode.Should().Be(MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE);
        unitPackage.PackageName.Should().Be("Không chọn gói");
        unitPackage.Description.Should().Be("Gói mặc định giá 0.");
        unitPackage.PriceAdjustment.Should().Be(0);
        unitPackage.StatusId.Should().Be(109);
    }

    [Fact]
    public void PropertyUnitPackageItemBuildModel_Should_MapPackageItem()
    {
        var unitPackage = new UnitPackage();
        var model = new PropertyUnitPackageItemBuildModel
        {
            UnitPackage = unitPackage,
            ItemName = " Giường ngủ King Size ",
            DisplayOrder = 1
        };

        var item = model.Adapt<UnitPackageItem>(_config);

        item.PublicId.Should().NotBeEmpty();
        item.UnitPackage.Should().BeSameAs(unitPackage);
        item.ItemName.Should().Be(" Giường ngủ King Size ");
        item.DisplayOrder.Should().Be(1);
    }

    [Fact]
    public void PropertyChargePolicyBuildModel_Should_MapUsageBasedPolicy()
    {
        var property = new Property();
        var model = new PropertyChargePolicyBuildModel
        {
            Property = property,
            Policy = new CreatePropertyChargePolicyRequestDto
            {
                Code = "ELECTRIC",
                Name = " Tiền điện ",
                Amount = 3500,
                CalculationMethodCode = CALCULATION_METHOD_METER_READING
            },
            ChargeType = new MasterDataValueModel
            {
                Id = 107,
                Code = "ELECTRIC",
                Name = "Điện"
            },
            ActiveStatusId = 106
        };

        var policy = model.Adapt<RentalChargePolicy>(_config);

        policy.PublicId.Should().NotBeEmpty();
        policy.Property.Should().BeSameAs(property);
        policy.LineTypeId.Should().Be(107);
        policy.ChargeName.Should().Be(" Tiền điện ");
        policy.Amount.Should().Be(3500);
        policy.IsUsageBased.Should().BeTrue();
        policy.StatusId.Should().Be(106);
    }

    [Fact]
    public void PropertyCreationGraph_Should_MapCreatedResponseWithFloorGrouping()
    {
        var property = new Property
        {
            PublicId = Guid.NewGuid(),
            PropertyCode = "PROP-TEST",
            Name = "Tòa nhà A",
            TotalFloors = 2,
            TotalUnits = 3
        };
        var graph = new PropertyCreationGraphModel
        {
            Property = property,
            Units =
            [
                new Unit { UnitCode = "101", FloorNumber = 1 },
                new Unit { UnitCode = "102", FloorNumber = 1 },
                new Unit { UnitCode = "201", FloorNumber = 2 }
            ]
        };

        var response = PropertyResponseMapper.MapCreated(graph);

        response.Id.Should().Be(property.PublicId);
        response.PropertyCode.Should().Be("PROP-TEST");
        response.Name.Should().Be("Tòa nhà A");
        response.TotalFloors.Should().Be(2);
        response.TotalRooms.Should().Be(3);
        response.Floors.Should().HaveCount(2);
        response.Floors[0].FloorNumber.Should().Be(1);
        response.Floors[0].TotalRooms.Should().Be(2);
        response.Floors[0].RoomCodes.Should().BeEquivalentTo(["101", "102"]);
        response.Floors[1].FloorNumber.Should().Be(2);
        response.Floors[1].RoomCodes.Should().ContainSingle("201");
    }

    private static TypeAdapterConfig BuildConfig()
    {
        var config = new TypeAdapterConfig();
        config.Scan(typeof(PropertyRequestMapping).Assembly);
        return config;
    }

    private static IReadOnlyCollection<string> GetExpectedRelationshipCodes()
    {
        return Enum.GetValues<PropertyRelationshipTypeEnum>()
            .Select(x => x.ToMasterDataCode())
            .ToArray();
    }

    private static PropertyCreationRequestModel BuildCreationRequest()
    {
        return new PropertyCreationRequestModel
        {
            CurrentParty = new CurrentPartyContextModel
            {
                PartyId = 123,
                PartyPublicId = Guid.NewGuid()
            },
            PropertyCode = "PROP-TEST",
            Name = " Tòa nhà A ",
            PropertyTypeCode = "BUILDING",
            ProvinceCode = "HCM",
            DistrictCode = "D1",
            WardCode = "W1",
            StreetAddress = " 123 Nguyễn Văn Linh ",
            FormattedAddress = " 123 Nguyễn Văn Linh, Quận 1, TP.HCM ",
            StructureSetup = BuildStructure(),
            Locations = new PropertyLocationContextModel
            {
                Province = new LocationLookupModel { Id = 10, Code = "HCM" },
                District = new LocationLookupModel { Id = 20, Code = "D1" },
                Ward = new LocationLookupModel { Id = 30, Code = "W1" }
            }
        };
    }

    private static PropertyCreationGraphInputModel BuildGraphInput(PropertyCreationRequestModel request)
    {
        return new PropertyCreationGraphInputModel
        {
            Request = request,
            Lookups = BuildLookups(request)
        };
    }

    private static PropertyCreationLookupModel BuildLookups(PropertyCreationRequestModel request)
    {
        var masterData = BuildMasterData();
        var roomRows = request.StructureSetup.Floors.SelectMany(floor => floor.Rooms ?? []).ToList();
        var chargePolicies = request.ChargePolicies ?? [];

        return new PropertyCreationLookupModel
        {
            PropertyTypeId = masterData[new MasterDataKeyModel(MasterDataTypeEnum.PropertyType, "BUILDING")].Id,
            PropertyStatusId = masterData[new MasterDataKeyModel(
                MasterDataTypeEnum.PropertyStatus,
                MASTER_CODE_PROPERTY_STATUS_DRAFT)].Id,
            UnitStatusId = masterData[new MasterDataKeyModel(MasterDataTypeEnum.UnitStatus, MASTER_CODE_UNIT_STATUS_AVAILABLE)].Id,
            ActiveStatusId = masterData[new MasterDataKeyModel(MasterDataTypeEnum.CommonStatus, MASTER_CODE_ACTIVE)].Id,
            LandlordRelationshipTypeId = masterData[new MasterDataKeyModel(
                MasterDataTypeEnum.PropertyRelationshipType,
                PropertyRelationshipTypeEnum.Landlord.ToMasterDataCode())].Id,
            NoFurniturePackageType = masterData[new MasterDataKeyModel(
                MasterDataTypeEnum.UnitPackageType,
                MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE)],
            CustomPackageType = masterData[new MasterDataKeyModel(
                MasterDataTypeEnum.UnitPackageType,
                MASTER_CODE_UNIT_PACKAGE_TYPE_CUSTOM)],
            PackageStatus = masterData[new MasterDataKeyModel(
                MasterDataTypeEnum.UnitPackageStatus,
                MASTER_CODE_UNIT_PACKAGE_STATUS_ACTIVE)],
            UnitTypeIds = roomRows
                .Select(room => room.TypeCode)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    code => code,
                    code => masterData[new MasterDataKeyModel(MasterDataTypeEnum.UnitType, code)].Id,
                    StringComparer.OrdinalIgnoreCase),
            RentalModeIds = roomRows
                .Select(room => room.RentalModeCode)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    code => code,
                    code => masterData[new MasterDataKeyModel(MasterDataTypeEnum.UnitRentalMode, code)].Id,
                    StringComparer.OrdinalIgnoreCase),
            ChargeTypes = chargePolicies
                .Select(policy => policy.Code)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    code => code,
                    code => masterData[new MasterDataKeyModel(MasterDataTypeEnum.InvoiceLineType, code)],
                    StringComparer.OrdinalIgnoreCase),
            VehicleTypeIds = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase)
        };
    }

    private static CreatePropertyStructureRequestDto BuildStructure()
    {
        return new CreatePropertyStructureRequestDto
        {
            Floors =
            [
                new CreatePropertyFloorRequestDto
                {
                    FloorNumber = 1,
                    Rooms =
                    [
                        new CreatePropertyRoomRequestDto
                        {
                            Name = "Phòng 101",
                            TypeCode = MASTER_CODE_UNIT_TYPE_ROOM,
                            RentalModeCode = MASTER_CODE_RENTAL_MODE_WHOLE_UNIT,
                            AreaSqm = 25,
                            BaseRentAmount = 5_500_000,
                            DefaultDepositAmount = 5_500_000,
                            IsPetAllowed = true
                        },
                        new CreatePropertyRoomRequestDto
                        {
                            Name = "Phòng 102",
                            TypeCode = MASTER_CODE_UNIT_TYPE_ROOM,
                            RentalModeCode = MASTER_CODE_RENTAL_MODE_WHOLE_UNIT,
                            AreaSqm = 25,
                            BaseRentAmount = 5_500_000,
                            DefaultDepositAmount = 5_500_000,
                            IsPetAllowed = true
                        }
                    ]
                },
                new CreatePropertyFloorRequestDto
                {
                    FloorNumber = 2,
                    Rooms =
                    [
                        new CreatePropertyRoomRequestDto
                        {
                            Name = "Phòng 201",
                            TypeCode = MASTER_CODE_UNIT_TYPE_ROOM,
                            RentalModeCode = MASTER_CODE_RENTAL_MODE_WHOLE_UNIT,
                            AreaSqm = 25,
                            BaseRentAmount = 5_500_000,
                            DefaultDepositAmount = 5_500_000,
                            IsPetAllowed = true
                        },
                        new CreatePropertyRoomRequestDto
                        {
                            Name = "Phòng 202",
                            TypeCode = MASTER_CODE_UNIT_TYPE_ROOM,
                            RentalModeCode = MASTER_CODE_RENTAL_MODE_WHOLE_UNIT,
                            AreaSqm = 25,
                            BaseRentAmount = 5_500_000,
                            DefaultDepositAmount = 5_500_000,
                            IsPetAllowed = true
                        }
                    ]
                }
            ]
        };
    }

    private static IReadOnlyDictionary<MasterDataKeyModel, MasterDataValueModel> BuildMasterData()
    {
        var values = new[]
        {
            BuildMasterData(MasterDataTypeEnum.PropertyType.ToString(), "BUILDING", 100),
            BuildMasterData(MasterDataTypeEnum.PropertyStatus.ToString(), MASTER_CODE_PROPERTY_STATUS_DRAFT, 101),
            BuildMasterData(MasterDataTypeEnum.UnitType.ToString(), MASTER_CODE_UNIT_TYPE_ROOM, 102),
            BuildMasterData(MasterDataTypeEnum.UnitRentalMode.ToString(), MASTER_CODE_RENTAL_MODE_WHOLE_UNIT, 103),
            BuildMasterData(MasterDataTypeEnum.UnitRentalMode.ToString(), MASTER_CODE_RENTAL_MODE_SHARED_BED, 111),
            BuildMasterData(MasterDataTypeEnum.UnitStatus.ToString(), MASTER_CODE_UNIT_STATUS_AVAILABLE, 104),
            BuildMasterData(MasterDataTypeEnum.PropertyRelationshipType.ToString(), PropertyRelationshipTypeEnum.Landlord.ToMasterDataCode(), 105),
            BuildMasterData(MasterDataTypeEnum.CommonStatus.ToString(), MASTER_CODE_ACTIVE, 106),
            BuildMasterData(MasterDataTypeEnum.InvoiceLineType.ToString(), "ELECTRIC", 107),
            BuildMasterData(MasterDataTypeEnum.UnitPackageType.ToString(), MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE, 108),
            BuildMasterData(MasterDataTypeEnum.UnitPackageStatus.ToString(), MASTER_CODE_UNIT_PACKAGE_STATUS_ACTIVE, 109),
            BuildMasterData(MasterDataTypeEnum.UnitPackageType.ToString(), MASTER_CODE_UNIT_PACKAGE_TYPE_CUSTOM, 110)
        };

        return values.ToDictionary(
            x => new MasterDataKeyModel(x.Type, x.Code),
            x => x);
    }

    private static MasterDataValueModel BuildMasterData(string type, string code, long id)
    {
        return new MasterDataValueModel
        {
            Id = id,
            Type = type,
            Code = code,
            Name = code
        };
    }
}

