using Haven.Application.Dtos.Rooms.Update;
using Haven.Application.Mappings.Rooms;
using Haven.Application.Models.Rooms.Update;
using Haven.Domain.Entities;
using Haven.Infrastructure.Mappings.Rooms;
using static Haven.Application.Constants.ApplicationConstants;

namespace Be.Haven.Tests.Services.Businesses.Haven.Infrastructure.Mappings;

public sealed class RoomFieldMutationMapperTests
{
    static RoomFieldMutationMapperTests()
    {
        // Register the production Mapster config because RoomFieldMutationMapper applies into a tracked entity.
    }

    [Fact]
    public void Apply_Should_UpdateSubmittedScalarFields_And_PreserveOmittedValues()
    {
        var room = new Unit
        {
            FloorNumber = 1,
            UnitCode = "101",
            UnitName = "Phòng 101",
            AreaSqm = 25,
            BaseRentAmount = 5_000_000,
            DefaultDepositAmount = 5_000_000,
            BedCount = 4,
            IsPetAllowed = true
        };
        var fields = new RoomFieldUpdateModel
        {
            FloorNumber = 2,
            Name = " ",
            BaseRentAmount = 5_500_000,
            IsPetAllowed = false
        };

        RoomFieldMutationMapper.Apply(room, fields, new RoomFieldLookupModel());

        room.FloorNumber.Should().Be(2);
        room.UnitCode.Should().Be("101");
        room.UnitName.Should().Be(" ");
        room.AreaSqm.Should().Be(25);
        room.BaseRentAmount.Should().Be(5_500_000);
        room.DefaultDepositAmount.Should().Be(5_000_000);
        room.BedCount.Should().Be(4);
        room.IsPetAllowed.Should().BeFalse();
    }

    [Fact]
    public void Apply_Should_ClearBeds_WhenRentalModeChangesAwayFromSharedBed()
    {
        var room = new Unit
        {
            RentalModeId = 1,
            BedCount = 4
        };
        var fields = new RoomFieldUpdateModel { RentalModeCode = "WHOLE_UNIT" };

        RoomFieldMutationMapper.Apply(room, fields, BuildLookups(rentalModeId: 2));

        room.RentalModeId.Should().Be(2);
        room.BedCount.Should().BeNull();
    }

    [Fact]
    public void Apply_Should_KeepSubmittedBeds_WhenRentalModeChangesToSharedBed()
    {
        var room = new Unit
        {
            RentalModeId = 2,
            BedCount = null
        };
        var fields = new RoomFieldUpdateModel
        {
            RentalModeCode = MASTER_CODE_RENTAL_MODE_SHARED_BED,
            TotalBeds = 6
        };

        RoomFieldMutationMapper.Apply(room, fields, BuildLookups(rentalModeId: 1));

        room.RentalModeId.Should().Be(1);
        room.BedCount.Should().Be(6);
    }

    [Fact]
    public void RoomPackageUpdate_Should_MergeSubmittedFields_AndPreserveOmittedFields()
    {
        var package = new UnitPackage
        {
            PackageCode = "PKG_654321",
            PackageName = "Tên cũ",
            PriceAdjustment = 800_000,
            PackageTypeId = 10,
            StatusId = 20
        };
        var request = new UpdateRoomPackageRequestDto
        {
            Id = Guid.NewGuid(),
            PriceAdjustment = 900_000
        };

        request.Adapt(package);

        package.PackageCode.Should().Be("PKG_654321");
        package.PackageName.Should().Be("Tên cũ");
        package.PriceAdjustment.Should().Be(900_000);
        package.PackageTypeId.Should().Be(10);
        package.StatusId.Should().Be(20);
    }

    private static RoomFieldLookupModel BuildLookups(long rentalModeId)
    {
        return new RoomFieldLookupModel
        {
            RentalModeId = rentalModeId
        };
    }
}
