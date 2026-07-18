using Haven.Application.Dtos.Properties.Create;
using Haven.Application.Dtos.Properties.Detail;
using Haven.Application.Dtos.Properties.List;
using Haven.Application.Mappings.Properties;
using Haven.Application.Models.Properties.Rows;
using Haven.Domain.Entities;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Mappings;

public sealed class PropertyResponseMapperTests
{
    static PropertyResponseMapperTests()
    {
    }

    [Fact]
    public void MapList_Should_AssembleFloorsAndRoomsByProperty()
    {
        var firstPropertyPublicId = Guid.NewGuid();
        var secondPropertyPublicId = Guid.NewGuid();
        var properties = new[]
        {
            BuildPropertyRow(firstPropertyPublicId, "PROP-A"),
            BuildPropertyRow(secondPropertyPublicId, "PROP-B")
        };
        var floors = new[]
        {
            BuildFloorRow(firstPropertyPublicId, 1),
            BuildFloorRow(firstPropertyPublicId, 2),
            BuildFloorRow(secondPropertyPublicId, 1)
        };
        var rooms = new[]
        {
            BuildRoomRow(firstPropertyPublicId, 1, "101"),
            BuildRoomRow(firstPropertyPublicId, 2, "201"),
            BuildRoomRow(secondPropertyPublicId, 1, "102")
        };

        var result = PropertyResponseMapper.MapList(properties, floors, rooms, []);

        result.Should().HaveCount(2);
        result[0].Id.Should().Be(firstPropertyPublicId);
        result[0].Address.Should().Be("Ward 1, District 1, Ho Chi Minh");
        result[0].TotalRooms.Should().Be(2);
        result[0].AvailableRooms.Should().Be(1);
        result[0].OccupancyRate.Should().Be(0.5m);
        result[0].Floors.Should().HaveCount(2);
        result[0].Floors[0].Rooms.Should().ContainSingle(x => x.Code == "101");
        result[0].Floors[1].Rooms.Should().ContainSingle(x => x.Code == "201");
        result[0].Floors[0].Rooms[0].RentalModeCode.Should().Be("WHOLE_UNIT");
        result[1].Id.Should().Be(secondPropertyPublicId);
        result[1].Floors.Should().ContainSingle();
        result[1].Floors[0].Rooms.Should().ContainSingle(x => x.Code == "102");
    }

    [Fact]
    public void MapList_Should_ReturnEmptyRoomLists_When_RoomRowsAreEmpty()
    {
        var propertyPublicId = Guid.NewGuid();
        var properties = new[] { BuildPropertyRow(propertyPublicId, "PROP-A") };
        var floors = new[] { BuildFloorRow(propertyPublicId, 1) };

        var result = PropertyResponseMapper.MapList(properties, floors, [], []);

        result.Should().ContainSingle();
        result[0].Floors.Should().ContainSingle();
        result[0].Floors[0].Rooms.Should().BeEmpty();
    }

    [Fact]
    public void MapList_Should_ReturnRooms_When_FloorRoomsAreRentedTogether()
    {
        var propertyPublicId = Guid.NewGuid();
        var properties = new[] { BuildPropertyRow(propertyPublicId, "PROP-A") };
        var floors = new[] { BuildFloorRow(propertyPublicId, 2) };
        var rooms = new[]
        {
            BuildRoomRow(propertyPublicId, 2, "201"),
            BuildRoomRow(propertyPublicId, 2, "202")
        };

        var result = PropertyResponseMapper.MapList(properties, floors, rooms, []);
        var floorPropertyNames = typeof(PropertyListFloorResponseDto)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();

        result.Should().ContainSingle();
        result[0].Floors.Should().ContainSingle();
        result[0].Floors[0].Rooms.Should().HaveCount(2);
        result[0].Floors[0].Rooms.Select(room => room.Code).Should().Equal("201", "202");
        floorPropertyNames.Should().NotContain(["IsWholeFloorRented", "WholeFloorRentalSummary"]);
    }

    [Fact]
    public void MapList_Should_ReturnTenantsForEveryOccupiedRoom()
    {
        var propertyPublicId = Guid.NewGuid();
        var sharedRoomPublicId = Guid.NewGuid();
        var wholeRoomPublicId = Guid.NewGuid();
        var vacantRoomPublicId = Guid.NewGuid();
        var wholeRoomTenantPublicId = Guid.NewGuid();
        var sharedRoomTenantPublicId = Guid.NewGuid();
        var properties = new[] { BuildPropertyRow(propertyPublicId, "PROP-A") };
        var floors = new[] { BuildFloorRow(propertyPublicId, 1) };
        var rooms = new[]
        {
            BuildRoomRow(propertyPublicId, 1, "101", wholeRoomPublicId, MASTER_CODE_RENTAL_MODE_WHOLE_UNIT),
            BuildRoomRow(propertyPublicId, 1, "102", sharedRoomPublicId, MASTER_CODE_RENTAL_MODE_SHARED_BED),
            BuildRoomRow(propertyPublicId, 1, "103", vacantRoomPublicId, MASTER_CODE_RENTAL_MODE_WHOLE_UNIT)
        };
        var tenants = new[]
        {
            BuildTenantRow(propertyPublicId, sharedRoomPublicId, "Nguyễn Văn A", sharedRoomTenantPublicId),
            BuildTenantRow(propertyPublicId, sharedRoomPublicId, "Trần Thị B"),
            BuildTenantRow(propertyPublicId, wholeRoomPublicId, "Người đại diện", wholeRoomTenantPublicId)
        };

        var result = PropertyResponseMapper.MapList(properties, floors, rooms, tenants);
        var wholeRoom = result[0].Floors[0].Rooms.Single(room => room.Code == "101");
        var sharedRoom = result[0].Floors[0].Rooms.Single(room => room.Code == "102");
        var vacantRoom = result[0].Floors[0].Rooms.Single(room => room.Code == "103");

        wholeRoom.Tenants.Should().ContainSingle();
        wholeRoom.Tenants[0].Id.Should().Be(wholeRoomTenantPublicId);
        wholeRoom.Tenants[0].Tenant.Should().Be("Người đại diện");
        sharedRoom.Tenants.Should().HaveCount(2);
        sharedRoom.Tenants[0].Id.Should().Be(sharedRoomTenantPublicId);
        sharedRoom.Tenants.Select(tenant => tenant.Tenant).Should().Equal("Nguyễn Văn A", "Trần Thị B");
        vacantRoom.Tenants.Should().BeEmpty();
    }

    [Fact]
    public void PropertyListItemResponseDto_Should_NotExposeDetailOnlyFields()
    {
        var propertyNames = typeof(PropertyListItemResponseDto)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();

        propertyNames.Should().NotContain(
            [
                "PropertyCode",
                "PropertyType",
                "Status",
                "AddressText",
                "TotalUnits",
                "AvailableUnitCount",
                "Summary",
                "OccupiedUnitCount",
                "MaintenanceUnitCount",
                "PublishedUnitCount",
                "ThumbnailUrl"
            ]);
    }

    [Fact]
    public void PropertyListRoomResponseDto_Should_KeepTenantDataOnlyInTenants()
    {
        var propertyNames = typeof(PropertyListRoomResponseDto)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();

        propertyNames.Should().Contain("Tenants");
        propertyNames.Should().Contain("TotalOccupiedBeds");
        propertyNames.Should().Contain("TotalBeds");
        propertyNames.Should().NotContain(["OccupiedBedCount", "BedCount"]);
        propertyNames.Should().NotContain(["UnitCode", "UnitName", "CurrentTenantName", "ContractStartDate", "ContractEndDate"]);
    }

    [Fact]
    public void PropertyListFloorResponseDto_Should_UseFigmaSearchNames()
    {
        var propertyNames = typeof(PropertyListFloorResponseDto)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();

        propertyNames.Should().Contain(["Number", "TotalRooms", "AvailableRooms"]);
        propertyNames.Should().NotContain(["FloorNumber", "RoomCount", "UnitCount", "AvailableUnitCount"]);
    }

    [Fact]
    public void PropertyListRoomTenantResponseDto_Should_UseFigmaSearchNames()
    {
        var propertyNames = typeof(PropertyListRoomTenantResponseDto)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();

        propertyNames.Should().Contain("Tenant");
        propertyNames.Should().NotContain("TenantName");
    }

    [Fact]
    public void PropertyDetailRoomResponseDto_Should_ExposeEditRoomShape()
    {
        var propertyNames = typeof(PropertyDetailRoomResponseDto)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();

        propertyNames.Should().Contain("TotalBeds");
        propertyNames.Should().Contain("DefaultDepositAmount");
        propertyNames.Should().Contain(["Code", "Name", "TypeCode", "TypeName"]);
        propertyNames.Should().NotContain(["TotalBedrooms", "TotalBathrooms"]);
        propertyNames.Should().NotContain("DeleteInfo");
        propertyNames.Should().NotContain(["UnitCode", "UnitName", "UnitTypeCode", "UnitTypeName", "FloorNumber"]);
    }

    [Fact]
    public void PropertyDetailResponseDtos_Should_UseFrontendFriendlyNames()
    {
        var detailNames = typeof(PropertyDetailResponseDto)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();
        var basicInfoNames = typeof(PropertyDetailBasicInfoResponseDto)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();
        var floorNames = typeof(PropertyDetailFloorResponseDto)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();
        var chargePolicyNames = typeof(PropertyChargePolicyResponseDto)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();
        var packageNames = typeof(PropertyPackageTemplateResponseDto)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();
        var packageItemNames = typeof(PropertyPackageTemplateItemResponseDto)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();
        var wholeBuildingNames = typeof(PropertyWholeBuildingRentalResponseDto)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();

        detailNames.Should().Contain(["BasicInfo", "Structure", "WholeBuildingRental", "ChargePolicies", "PackageTemplates"]);
        detailNames.Should().NotContain(["Address", "ManagementSummary"]);
        basicInfoNames.Should().Contain(["Code", "ProvinceCode", "DistrictCode", "WardCode", "StreetAddress", "FormattedAddress"]);
        basicInfoNames.Should().NotContain([
            "PropertyCode",
            "Description",
            "IsPublished",
            "IsPendingDelete",
            "DeleteScheduledAt",
            "DeleteInfo",
            "ThumbnailUrl"
        ]);
        floorNames.Should().Contain("Number");
        floorNames.Should().NotContain(["FloorNumber", "DeleteInfo"]);
        chargePolicyNames.Should().Contain(["Code", "Name", "CalculationMethodCode"]);
        chargePolicyNames.Should().NotContain(["ChargeTypeCode", "ChargeTypeName", "ChargeName", "IsUsageBased"]);
        packageNames.Should().Contain(["Id", "Name"]);
        packageNames.Should().NotContain(["PackageCode", "PackageName"]);
        packageItemNames.Should().Contain("Name");
        packageItemNames.Should().NotContain("ItemName");
        wholeBuildingNames.Should().Contain("Contract");
        wholeBuildingNames.Should().NotContain("CanCreate");
    }

    [Fact]
    public void MapDetail_Should_AssembleNestedSections()
    {
        var propertyPublicId = Guid.NewGuid();
        var row = BuildPropertyRow(propertyPublicId, "PROP-A");
        var floors = new[] { BuildFloorRow(propertyPublicId, 1) };
        var rooms = new[]
        {
            BuildRoomRow(propertyPublicId, 1, "101"),
            BuildRoomRow(propertyPublicId, 1, "102")
        };
        var chargePolicies = new[]
        {
            new PropertyChargePolicyRowModel
            {
                PolicyPublicId = Guid.NewGuid(),
                ChargeTypeCode = "ELECTRIC",
                ChargeTypeName = "Electricity",
                ChargeName = "Electricity",
                Amount = 3500,
                StatusCode = "ACTIVE",
                StatusName = "Active"
            }
        };
        var packagePublicId = Guid.NewGuid();
        var packageTemplates = new[]
        {
            new PropertyPackageTemplateRowModel
            {
                PackagePublicId = packagePublicId,
                PackageName = "Gói Full Nội thất",
                PriceAdjustment = 1_500_000,
                ItemName = "Giường ngủ King Size",
                DisplayOrder = 1
            },
            new PropertyPackageTemplateRowModel
            {
                PackagePublicId = packagePublicId,
                PackageName = "Gói Full Nội thất",
                PriceAdjustment = 1_500_000,
                ItemName = "Tủ lạnh 200L",
                DisplayOrder = 2
            }
        };

        var result = PropertyResponseMapper.MapDetail(row, floors, rooms, chargePolicies, packageTemplates, null);

        result.BasicInfo.Id.Should().Be(propertyPublicId);
        result.BasicInfo.Code.Should().Be("PROP-A");
        result.BasicInfo.ProvinceCode.Should().Be("HCM");
        result.BasicInfo.DistrictCode.Should().Be("D1");
        result.BasicInfo.WardCode.Should().Be("W1");
        result.Structure.TotalFloors.Should().Be(2);
        result.Structure.Floors.Should().ContainSingle();
        result.Structure.Floors[0].Number.Should().Be(1);
        result.Structure.Floors[0].Rooms.Should().HaveCount(2);
        result.Structure.Floors[0].Rooms[0].Code.Should().Be("101");
        result.Structure.Floors[0].Rooms[0].TypeCode.Should().Be("ROOM");
        result.Structure.Floors[0].Rooms[0].DefaultDepositAmount.Should().Be(5_500_000);
        result.ChargePolicies.Should().ContainSingle();
        result.ChargePolicies[0].Code.Should().Be("ELECTRIC");
        result.ChargePolicies[0].Name.Should().Be("Electricity");
        result.ChargePolicies[0].CalculationMethodCode.Should().Be(CALCULATION_METHOD_FIXED);
        result.PackageTemplates.Should().ContainSingle();
        result.PackageTemplates[0].Id.Should().Be(packagePublicId);
        result.PackageTemplates[0].Name.Should().Be("Gói Full Nội thất");
        result.PackageTemplates[0].Items.Select(item => item.Name).Should().Equal("Giường ngủ King Size", "Tủ lạnh 200L");
    }

    [Fact]
    public void MapDetail_Should_ReturnWholeBuildingRentalCta_When_BuildingUnitExistsAndNoRentalExists()
    {
        var row = BuildPropertyRow(Guid.NewGuid(), "PROP-A");
        var wholeBuildingRental = new PropertyWholeBuildingRentalRowModel();

        var result = PropertyResponseMapper.MapDetail(row, [], [], [], [], wholeBuildingRental);

        result.WholeBuildingRental.Should().NotBeNull();
        result.WholeBuildingRental.Contract.Should().BeNull();
    }

    [Fact]
    public void MapDetail_Should_ReturnWholeBuildingRentalContract_When_BuildingUnitHasContract()
    {
        var row = BuildPropertyRow(Guid.NewGuid(), "PROP-A");
        var contractPublicId = Guid.NewGuid();
        var tenantPublicId = Guid.NewGuid();
        var wholeBuildingRental = new PropertyWholeBuildingRentalRowModel
        {
            ContractPublicId = contractPublicId,
            ContractCode = "HD-001",
            TenantPublicId = tenantPublicId,
            TenantName = "NextGen Globals Inc.",
            StatusCode = "ACTIVE",
            StatusName = "Đang hiệu lực",
            TotalRentAmount = 45_000_000,
            StartDate = new DateTime(2026, 10, 12),
            EndDate = new DateTime(2027, 10, 12)
        };

        var result = PropertyResponseMapper.MapDetail(
            row,
            [],
            [],
            [],
            [],
            wholeBuildingRental);

        result.WholeBuildingRental.Should().NotBeNull();
        result.WholeBuildingRental.Contract.Id.Should().Be(contractPublicId);
        result.WholeBuildingRental.Contract.TenantId.Should().Be(tenantPublicId);
        result.WholeBuildingRental.Contract.Tenant.Should().Be("NextGen Globals Inc.");
        result.WholeBuildingRental.Contract.TotalRentAmount.Should().Be(45_000_000);
    }

    [Fact]
    public void MapDetail_Should_HideWholeBuildingRental_When_PropertyHasOnlyNormalRoomRentals()
    {
        var row = BuildPropertyRow(Guid.NewGuid(), "PROP-A");

        var result = PropertyResponseMapper.MapDetail(
            row,
            [],
            [],
            [],
            [],
            null);

        result.WholeBuildingRental.Should().BeNull();
    }

    [Fact]
    public void PropertyResponseDtos_Should_ExposeSafeIdentifiersAsId()
    {
        var responseTypes = new[]
        {
            typeof(PropertyListItemResponseDto),
            typeof(PropertyListRoomResponseDto),
            typeof(PropertyListRoomTenantResponseDto),
            typeof(CreatedPropertyResponseDto),
            typeof(PropertyDetailBasicInfoResponseDto),
            typeof(PropertyDetailRoomResponseDto),
            typeof(PropertyChargePolicyResponseDto),
            typeof(PropertyWholeBuildingRentalContractResponseDto)
        };

        var propertyNames = responseTypes
            .SelectMany(type => type.GetProperties())
            .Select(property => property.Name)
            .ToArray();

        propertyNames.Should().NotContain(name => name.Contains("PublicId", StringComparison.Ordinal));
        responseTypes.Should().OnlyContain(type => type.GetProperties().Any(property => property.Name == "Id"));
    }

    [Fact]
    public void MapCreatedFloors_Should_GroupGeneratedUnitCodesByFloor()
    {
        var units = new[]
        {
            new Unit { UnitCode = "102", FloorNumber = 1 },
            new Unit { UnitCode = "101", FloorNumber = 1 },
            new Unit { UnitCode = "201", FloorNumber = 2 }
        };

        var result = PropertyResponseMapper.MapCreatedFloors(units);

        result.Should().HaveCount(2);
        result[0].FloorNumber.Should().Be(1);
        result[0].TotalRooms.Should().Be(2);
        result[0].RoomCodes.Should().Equal("101", "102");
        result[1].FloorNumber.Should().Be(2);
        result[1].RoomCodes.Should().ContainSingle("201");
    }

    private static PropertyRowModel BuildPropertyRow(Guid propertyPublicId, string propertyCode)
    {
        return new PropertyRowModel
        {
            PropertyPublicId = propertyPublicId,
            PropertyCode = propertyCode,
            Name = propertyCode,
            PropertyTypeCode = "BUILDING",
            PropertyTypeName = "Building",
            StatusCode = "DRAFT",
            StatusName = "Draft",
            ProvinceCode = "HCM",
            ProvinceName = "Ho Chi Minh",
            DistrictCode = "D1",
            DistrictName = "District 1",
            WardCode = "W1",
            WardName = "Ward 1",
            TotalFloors = 2,
            TotalUnits = 2,
            AvailableUnitCount = 1,
            OccupiedUnitCount = 1,
            OccupancyRate = 0.5m
        };
    }

    private static PropertyFloorRowModel BuildFloorRow(Guid propertyPublicId, int floorNumber)
    {
        return new PropertyFloorRowModel
        {
            PropertyPublicId = propertyPublicId,
            FloorNumber = floorNumber,
            UnitCount = 1,
            AvailableUnitCount = 1
        };
    }

    private static PropertyRoomRowModel BuildRoomRow(
        Guid propertyPublicId,
        int floorNumber,
        string unitCode,
        Guid? unitPublicId = null,
        string rentalModeCode = MASTER_CODE_RENTAL_MODE_WHOLE_UNIT)
    {
        return new PropertyRoomRowModel
        {
            PropertyPublicId = propertyPublicId,
            UnitPublicId = unitPublicId ?? Guid.NewGuid(),
            UnitCode = unitCode,
            UnitName = $"Phòng {unitCode}",
            FloorNumber = floorNumber,
            BaseRentAmount = 5_500_000,
            DefaultDepositAmount = 5_500_000,
            TotalRentAmount = 5_500_000,
            UnitTypeCode = "ROOM",
            UnitTypeName = "Room",
            RentalModeCode = rentalModeCode,
            RentalModeName = rentalModeCode,
            StatusCode = "AVAILABLE",
            StatusName = "Available",
            OccupiedBedCount = 0,
            BedCount = 4
        };
    }

    private static PropertyRoomTenantRowModel BuildTenantRow(
        Guid propertyPublicId,
        Guid unitPublicId,
        string tenantName,
        Guid? tenantPublicId = null)
    {
        return new PropertyRoomTenantRowModel
        {
            PropertyPublicId = propertyPublicId,
            UnitPublicId = unitPublicId,
            OccupancyPublicId = tenantPublicId ?? Guid.NewGuid(),
            TenantPublicId = tenantPublicId ?? Guid.NewGuid(),
            TenantName = tenantName,
            RoleCode = TENANT_ROLE_PRIMARY,
            ContractStartDate = new DateTime(2026, 1, 1),
            ContractEndDate = new DateTime(2026, 12, 31),
            OccupancyStatusCode = "ACTIVE",
            OccupancyStatusName = "Đang thuê"
        };
    }
}
