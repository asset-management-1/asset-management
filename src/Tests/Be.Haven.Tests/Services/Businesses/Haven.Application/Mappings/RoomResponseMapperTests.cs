using Haven.Application.Dtos.Rooms.Detail;
using Haven.Application.Dtos.Rooms.List;
using Haven.Application.Mappings.Properties;
using Haven.Application.Mappings.Rooms;
using Haven.Application.Mappings.Tenants;
using Haven.Application.Models.Rooms.Rows;
using static Haven.Application.Constants.ApplicationConstants;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Mappings;

public sealed class RoomResponseMapperTests
{
    static RoomResponseMapperTests()
    {
    }

    [Fact]
    public void MapList_Should_GroupRoomsByPropertyAndFloor()
    {
        var propertyId = Guid.NewGuid();
        var firstRoomId = Guid.NewGuid();
        var secondRoomId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var rooms = new[]
        {
            BuildRoomRow(propertyId, firstRoomId, 1, "101"),
            BuildRoomRow(propertyId, secondRoomId, 2, "201")
        };
        var tenants = new[]
        {
            new RoomTenantRowModel
            {
                UnitPublicId = firstRoomId,
                OccupancyPublicId = tenantId,
                TenantPublicId = tenantId,
                TenantName = "Nguyễn Văn A",
                RoleCode = TENANT_ROLE_PRIMARY,
                ContractStartDate = new DateTime(2026, 1, 1),
                ContractEndDate = new DateTime(2026, 12, 31),
                OccupancyStatusCode = MASTER_CODE_ACTIVE,
                OccupancyStatusName = "Đang thuê"
            }
        };

        var result = RoomResponseMapper.MapList(rooms, tenants);

        result.Should().ContainSingle();
        result[0].Id.Should().Be(propertyId);
        result[0].Code.Should().Be("PROP_001");
        result[0].TotalRooms.Should().Be(12);
        result[0].AvailableRooms.Should().Be(3);
        result[0].Floors.Should().HaveCount(2);
        result[0].Floors[0].Number.Should().Be(1);
        result[0].Floors[0].Rooms.Should().ContainSingle(room => room.Code == "101");
        result[0].Floors[0].Rooms[0].Id.Should().Be(firstRoomId);
        result[0].Floors[0].Rooms[0].Tenants.Should().ContainSingle();
        result[0].Floors[0].Rooms[0].Tenants[0].Id.Should().Be(tenantId);
        result[0].Floors[0].Rooms[0].Tenants[0].Tenant.Should().Be("Nguyễn Văn A");
        result[0].Floors[1].Rooms.Should().ContainSingle(room => room.Code == "201");
    }

    [Fact]
    public void MapDetail_Should_MapBasicInformationAndEffectivePoliciesOnly()
    {
        var roomId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();
        var row = new RoomDetailRowModel
        {
            UnitPublicId = roomId,
            UnitCode = "101",
            UnitName = "Phòng 101",
            FloorNumber = 1,
            PropertyPublicId = propertyId,
            PropertyCode = "PROP_001",
            PropertyName = "Atlas Plaza",
            PropertyAddress = "122 Đường Sáng Tạo",
            UnitTypeCode = MASTER_CODE_UNIT_TYPE_ROOM,
            UnitTypeName = "Phòng",
            RentalModeCode = MASTER_CODE_RENTAL_MODE_WHOLE_UNIT,
            RentalModeName = "Thuê nguyên phòng",
            StatusCode = MASTER_CODE_UNIT_STATUS_AVAILABLE,
            StatusName = "Còn trống",
            BaseRentAmount = 5_500_000,
            DefaultDepositAmount = 5_500_000,
            TotalBeds = null,
            IsPetAllowed = true,
            UsesCommonChargePolicies = true,
            UsesCommonPackages = false
        };

        var result = RoomResponseMapper.MapDetail(
            row,
            [
                new RoomChargePolicyRowModel
                {
                    PolicyPublicId = Guid.NewGuid(),
                    ChargeTypeCode = "ELECTRIC",
                    ChargeTypeName = "Tiền điện",
                    ChargeName = "Tiền điện",
                    IsUsageBased = true,
                    Amount = 3500,
                    StatusCode = MASTER_CODE_ACTIVE,
                    StatusName = "Active"
                }
            ]);

        result.BasicInfo.Id.Should().Be(roomId);
        result.BasicInfo.PropertyId.Should().Be(propertyId);
        result.BasicInfo.UsesCommonChargePolicies.Should().BeTrue();
        result.BasicInfo.UsesCommonPackages.Should().BeFalse();
        result.ChargePolicies.Should().ContainSingle();
        result.ChargePolicies[0].CalculationMethodCode.Should().Be(CALCULATION_METHOD_METER_READING);

        typeof(RoomDetailResponseDto)
            .GetProperties()
            .Select(property => property.Name)
            .Should()
            .BeEquivalentTo(["BasicInfo", "ChargePolicies"]);
    }

    [Fact]
    public void RoomResponseDtos_Should_ExposeSafeIdentifiersAsId()
    {
        var responseTypes = new[]
        {
            typeof(RoomListPropertyResponseDto),
            typeof(RoomListRoomResponseDto),
            typeof(RoomListTenantResponseDto),
            typeof(RoomDetailBasicInfoResponseDto),
            typeof(RoomChargePolicyResponseDto)
        };

        var propertyNames = responseTypes
            .SelectMany(type => type.GetProperties())
            .Select(property => property.Name)
            .ToArray();

        propertyNames.Should().NotContain(name => name.Contains("PublicId", StringComparison.Ordinal));
        responseTypes.Should().OnlyContain(type => type.GetProperties().Any(property => property.Name == "Id"));
    }

    private static RoomListRoomRowModel BuildRoomRow(Guid propertyId, Guid roomId, int floorNumber, string roomCode)
    {
        return new RoomListRoomRowModel
        {
            PropertyPublicId = propertyId,
            PropertyCode = "PROP_001",
            PropertyName = "Atlas Plaza",
            PropertyAddress = "122 Đường Sáng Tạo",
            PropertyTotalRooms = 12,
            PropertyAvailableRooms = 3,
            FloorNumber = floorNumber,
            FloorTotalRooms = 2,
            FloorAvailableRooms = 1,
            UnitPublicId = roomId,
            UnitCode = roomCode,
            UnitName = $"Phòng {roomCode}",
            UnitTypeCode = MASTER_CODE_UNIT_TYPE_ROOM,
            UnitTypeName = "Phòng",
            RentalModeCode = MASTER_CODE_RENTAL_MODE_WHOLE_UNIT,
            RentalModeName = "Thuê nguyên phòng",
            StatusCode = MASTER_CODE_UNIT_STATUS_AVAILABLE,
            StatusName = "Còn trống",
            TotalRentAmount = 5_500_000,
            TotalOccupiedBeds = 0,
            TotalBeds = null,
            UsesCommonChargePolicies = true,
            UsesCommonPackages = true
        };
    }
}
