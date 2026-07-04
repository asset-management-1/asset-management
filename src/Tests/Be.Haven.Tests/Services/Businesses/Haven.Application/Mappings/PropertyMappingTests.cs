using Haven.Application.Dtos.Properties.Common;
using Haven.Application.Dtos.Properties.Create;
using Haven.Application.Dtos.Properties.Detail;
using Haven.Application.Dtos.Properties.List;
using Haven.Application.Helpers;
using Haven.Application.Mappings;
using Haven.Application.Models.Locations;
using Haven.Application.Models.MasterData;
using Haven.Application.Models.Parties;
using Haven.Application.Models.Properties.Create;
using Haven.Application.Models.Properties.Detail;
using Haven.Application.Models.Properties.List;
using Haven.Application.Models.Properties.QueryParameters;
using Haven.Application.Models.Properties.Rows;
using Haven.Domain.Entities;
using Haven.Domain.Enums;
using static Haven.Application.Constants.ApplicationConstants;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Mappings;

public sealed class PropertyMappingTests
{
    private readonly TypeAdapterConfig _config = BuildConfig();

    [Fact]
    public void PropertyCreationRequest_Should_MapDraftPropertyFields()
    {
        var request = BuildCreationRequest();

        var property = request.Adapt<Property>(_config);

        property.PublicId.Should().NotBeEmpty();
        property.PropertyCode.Should().Be(request.PropertyCode);
        property.Name.Should().Be("Tòa nhà A");
        property.PropertyTypeId.Should().Be(100);
        property.ProvinceId.Should().Be(10);
        property.DistrictId.Should().Be(20);
        property.WardId.Should().Be(30);
        property.StreetAddress.Should().Be("123 Nguyễn Văn Linh");
        property.FormattedAddress.Should().Be("123 Nguyễn Văn Linh, Quận 1, TP.HCM");
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
        unit.UnitName.Should().Be("Phòng 201");
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

        var graph = request.Adapt<PropertyCreationGraphModel>(_config);

        graph.Property.Should().NotBeNull();
        graph.Units.Should().HaveCount(4);
        graph.Units.Should().OnlyContain(unit => unit.Property == graph.Property);
        graph.UnitPackages.Should().HaveCount(8);
        graph.UnitPackages.Count(package => package.PackageCode == MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE).Should().Be(4);
        graph.UnitPackages.Count(package => package.PackageCode == "PKG-001").Should().Be(4);
        graph.UnitPackages.Where(package => package.PackageCode == "PKG-001")
            .Should().OnlyContain(package => package.PackageName == "Gói Full Nội thất" && package.PriceAdjustment == 1_500_000);
        graph.UnitPackageItems.Should().HaveCount(8);
        graph.UnitPackageItems.Should().OnlyContain(item => item.UnitPackage.PackageCode == "PKG-001");
        graph.UnitPackageItems.Select(item => item.ItemName).Should().Contain("Giường ngủ King Size");
        graph.UnitPackageItems.Select(item => item.ItemName).Should().Contain("Tủ lạnh 200L");
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
                            Code = "A101",
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
                            Code = "A201",
                            Name = "Phòng A201",
                            TypeCode = MASTER_CODE_UNIT_TYPE_ROOM,
                            RentalModeCode = MASTER_CODE_RENTAL_MODE_WHOLE_UNIT,
                            BaseRentAmount = 7_000_000
                        }
                    ]
                }
            ]
        };

        var graph = request.Adapt<PropertyCreationGraphModel>(_config);

        graph.Property.TotalFloors.Should().Be(2);
        graph.Property.TotalUnits.Should().Be(2);
        graph.Units.Select(unit => unit.UnitCode).Should().Equal("A101", "A201");
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
                Type = MASTER_TYPE_UNIT_PACKAGE_TYPE,
                Code = MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE,
                Name = "Không chọn gói",
                Description = "Gói mặc định giá 0."
            },
            Status = new MasterDataValueModel
            {
                Id = 109,
                Type = MASTER_TYPE_UNIT_PACKAGE_STATUS,
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
        item.ItemName.Should().Be("Giường ngủ King Size");
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
        policy.ChargeName.Should().Be("Tiền điện");
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

    [Theory]
    [InlineData(null, 2, 5, "205")]
    [InlineData("", 3, 7, "307")]
    [InlineData("F{floor}-R{room}", 4, 9, "F4-R09")]
    [InlineData("T{FLOOR}.{ROOM}", 12, 3, "T12.03")]
    public void PropertyUnitCodeHelper_Should_GenerateCodeFromConfiguredPattern(
        string pattern,
        int floor,
        int room,
        string expected)
    {
        var result = PropertyUnitCodeHelper.Generate(pattern, floor, room);

        result.Should().Be(expected);
    }

    private static TypeAdapterConfig BuildConfig()
    {
        var config = new TypeAdapterConfig();
        new PropertyMapping().Register(config);
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
            MasterData = BuildMasterData(),
            Locations = new PropertyLocationContextModel
            {
                Province = new LocationLookupModel { Id = 10, Code = "HCM" },
                District = new LocationLookupModel { Id = 20, Code = "D1" },
                Ward = new LocationLookupModel { Id = 30, Code = "W1" }
            }
        };
    }

    private static CreatePropertyStructureRequestDto BuildStructure()
    {
        return new CreatePropertyStructureRequestDto
        {
            TotalFloors = 2,
            RoomsPerFloor = 2,
            RoomNumberingPattern = DEFAULT_ROOM_NUMBERING_PATTERN,
            DefaultUnitTypeCode = MASTER_CODE_UNIT_TYPE_ROOM,
            DefaultRentalModeCode = MASTER_CODE_RENTAL_MODE_WHOLE_UNIT,
            DefaultAreaSqm = 25,
            DefaultBaseRentAmount = 5_500_000,
            DefaultDepositAmount = 5_500_000,
            DefaultIsPetAllowed = true
        };
    }

    private static IReadOnlyDictionary<MasterDataKeyModel, MasterDataValueModel> BuildMasterData()
    {
        var values = new[]
        {
            BuildMasterData(MASTER_TYPE_PROPERTY_TYPE, "BUILDING", 100),
            BuildMasterData(MASTER_TYPE_PROPERTY_STATUS, MASTER_CODE_PROPERTY_STATUS_DRAFT, 101),
            BuildMasterData(MASTER_TYPE_UNIT_TYPE, MASTER_CODE_UNIT_TYPE_ROOM, 102),
            BuildMasterData(MASTER_TYPE_UNIT_RENTAL_MODE, MASTER_CODE_RENTAL_MODE_WHOLE_UNIT, 103),
            BuildMasterData(MASTER_TYPE_UNIT_RENTAL_MODE, MASTER_CODE_RENTAL_MODE_SHARED_BED, 111),
            BuildMasterData(MASTER_TYPE_UNIT_STATUS, MASTER_CODE_UNIT_STATUS_AVAILABLE, 104),
            BuildMasterData(MASTER_TYPE_PROPERTY_RELATIONSHIP_TYPE, PropertyRelationshipTypeEnum.Landlord.ToMasterDataCode(), 105),
            BuildMasterData(MASTER_TYPE_COMMON_STATUS, MASTER_CODE_ACTIVE, 106),
            BuildMasterData(MASTER_TYPE_INVOICE_LINE_TYPE, "ELECTRIC", 107),
            BuildMasterData(MASTER_TYPE_UNIT_PACKAGE_TYPE, MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE, 108),
            BuildMasterData(MASTER_TYPE_UNIT_PACKAGE_STATUS, MASTER_CODE_UNIT_PACKAGE_STATUS_ACTIVE, 109),
            BuildMasterData(MASTER_TYPE_UNIT_PACKAGE_TYPE, MASTER_CODE_UNIT_PACKAGE_TYPE_CUSTOM, 110)
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

