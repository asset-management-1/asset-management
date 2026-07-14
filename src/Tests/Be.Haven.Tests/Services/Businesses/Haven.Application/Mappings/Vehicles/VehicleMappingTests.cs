using Haven.Application.Commands.UpdateVehicle;
using Haven.Application.Mappings.Vehicles;
using Haven.Application.Models.Vehicles.Update;
using Haven.Domain.Entities;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Mappings.Vehicles;

public sealed class VehicleMappingTests
{
    [Fact]
    public void Adapt_Should_KeepRelationshipAndImageFields_When_PartialScalarUpdateIsSubmitted()
    {
        var config = new TypeAdapterConfig();
        new VehicleMapping().Register(config);
        var vehicle = new PartyVehicle
        {
            PartyId = 10,
            UnitId = 20,
            VehicleTypeId = 30,
            VehicleName = "Old name",
            LicensePlate = "59-X1 123.45",
            VehicleFrontImageUrl = "old-image"
        };
        var request = new UpdateVehicleCommand
        {
            RoomId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            Name = "New name"
        }
            .Adapt<VehicleUpdateRequestModel>(config);

        request.Adapt(vehicle, config);

        vehicle.VehicleName.Should().Be("New name");
        vehicle.PartyId.Should().Be(10);
        vehicle.UnitId.Should().Be(20);
        vehicle.VehicleTypeId.Should().Be(30);
        vehicle.LicensePlate.Should().Be("59-X1 123.45");
        vehicle.VehicleFrontImageUrl.Should().Be("old-image");
    }
}
