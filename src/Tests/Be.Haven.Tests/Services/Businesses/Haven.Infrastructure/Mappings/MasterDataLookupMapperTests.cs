using Haven.Application.Dtos.Rooms.Update;
using Haven.Application.Models.MasterData;
using Haven.Application.Models.Rooms.Update;
using Haven.Infrastructure.Mappings.MasterData;

namespace Be.Haven.Tests.Services.Businesses.Haven.Infrastructure.Mappings;

public sealed class MasterDataLookupMapperTests
{
    [Fact]
    public void BuildRoomFieldLookups_Should_ResolveOnlySubmittedRoomCodes()
    {
        var fields = new RoomFieldUpdateModel
        {
            TypeCode = "ROOM",
            RentalModeCode = MASTER_CODE_RENTAL_MODE_SHARED_BED
        };

        var lookups = MasterDataLookupMapper.BuildRoomFieldLookups(
            fields,
            BuildMasterData(),
            includeAvailableStatus: true);

        lookups.UnitTypeId.Should().Be(10);
        lookups.RentalModeId.Should().Be(20);
        lookups.AvailableStatusId.Should().Be(30);
    }

    [Fact]
    public void BuildChargePolicyLookups_Should_ResolveVehicleTypeOnlyForParking()
    {
        var input = new UpdateRoomChargePolicyRequestDto
        {
            Code = MASTER_CODE_INVOICE_LINE_TYPE_PARKING,
            Name = "Phí giữ xe",
            Amount = 100000,
            CalculationMethodCode = CALCULATION_METHOD_FIXED,
            VehicleTypeCode = "MOTORBIKE"
        };

        var lookups = MasterDataLookupMapper.BuildChargePolicyLookups(
            input,
            BuildMasterData(),
            includeActiveStatus: true);

        lookups.LineTypeId.Should().Be(40);
        lookups.VehicleTypeId.Should().Be(50);
        lookups.StatusId.Should().Be(60);
    }

    [Fact]
    public void BuildChargePolicyLookups_Should_NotApplyCreationStatus_ToExistingPolicy()
    {
        var input = new UpdateRoomChargePolicyRequestDto
        {
            Name = "Tên mới"
        };

        var lookups = MasterDataLookupMapper.BuildChargePolicyLookups(
            input,
            BuildMasterData(),
            includeActiveStatus: false);

        lookups.StatusId.Should().BeNull();
    }

    [Fact]
    public void BuildUnitPackageLookups_Should_SkipCustomPackageType_When_NoNewPackageExists()
    {
        var lookups = MasterDataLookupMapper.BuildUnitPackageLookups(
            BuildMasterData(),
            includeCustomType: false);

        lookups.NoFurniturePackageType.Id.Should().Be(70);
        lookups.CustomPackageTypeId.Should().Be(0);
        lookups.ActiveStatusId.Should().Be(90);
    }

    [Fact]
    public void BuildUnitPackageLookups_Should_ResolveCustomPackageType_When_NewPackageExists()
    {
        var lookups = MasterDataLookupMapper.BuildUnitPackageLookups(
            BuildMasterData(),
            includeCustomType: true);

        lookups.CustomPackageTypeId.Should().Be(80);
    }

    private static IReadOnlyDictionary<MasterDataKeyModel, MasterDataValueModel> BuildMasterData()
    {
        return new Dictionary<MasterDataKeyModel, MasterDataValueModel>
        {
            [new(MasterDataTypeEnum.UnitType, "ROOM")] = BuildValue(10, MasterDataTypeEnum.UnitType, "ROOM"),
            [new(MasterDataTypeEnum.UnitRentalMode, MASTER_CODE_RENTAL_MODE_SHARED_BED)] = BuildValue(20, MasterDataTypeEnum.UnitRentalMode, MASTER_CODE_RENTAL_MODE_SHARED_BED),
            [new(MasterDataTypeEnum.UnitStatus, MASTER_CODE_UNIT_STATUS_AVAILABLE)] = BuildValue(30, MasterDataTypeEnum.UnitStatus, MASTER_CODE_UNIT_STATUS_AVAILABLE),
            [new(MasterDataTypeEnum.InvoiceLineType, MASTER_CODE_INVOICE_LINE_TYPE_PARKING)] = BuildValue(40, MasterDataTypeEnum.InvoiceLineType, MASTER_CODE_INVOICE_LINE_TYPE_PARKING),
            [new(MasterDataTypeEnum.VehicleType, "MOTORBIKE")] = BuildValue(50, MasterDataTypeEnum.VehicleType, "MOTORBIKE"),
            [new(MasterDataTypeEnum.CommonStatus, MASTER_CODE_ACTIVE)] = BuildValue(60, MasterDataTypeEnum.CommonStatus, MASTER_CODE_ACTIVE),
            [new(MasterDataTypeEnum.UnitPackageType, MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE)] = BuildValue(70, MasterDataTypeEnum.UnitPackageType, MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE),
            [new(MasterDataTypeEnum.UnitPackageType, MASTER_CODE_UNIT_PACKAGE_TYPE_CUSTOM)] = BuildValue(80, MasterDataTypeEnum.UnitPackageType, MASTER_CODE_UNIT_PACKAGE_TYPE_CUSTOM),
            [new(MasterDataTypeEnum.UnitPackageStatus, MASTER_CODE_UNIT_PACKAGE_STATUS_ACTIVE)] = BuildValue(90, MasterDataTypeEnum.UnitPackageStatus, MASTER_CODE_UNIT_PACKAGE_STATUS_ACTIVE)
        };
    }

    private static MasterDataValueModel BuildValue(long id, MasterDataTypeEnum type, string code)
    {
        return new MasterDataValueModel
        {
            Id = id,
            Type = type.ToString(),
            Code = code,
            Name = code
        };
    }
}
