using Haven.Application.Dtos.Rooms.Update;
using Haven.Application.Mappings.Rooms;
using Haven.Application.Models.RentalChargePolicies;
using Haven.Domain.Entities;
using Haven.Infrastructure.Mappings.RentalChargePolicies;
using Mapster;
using static Haven.Application.Constants.ApplicationConstants;

namespace Be.Haven.Tests.Services.Businesses.Haven.Infrastructure.Mappings;

public sealed class RentalChargePolicyFieldMapperTests
{
    public RentalChargePolicyFieldMapperTests()
    {
        // Production scans Application mapping profiles during startup; register the touched profile for this isolated mapper test.
    }

    [Fact]
    public void Apply_Should_MapNonParkingPolicy_And_ClearVehicleType()
    {
        var policy = new RentalChargePolicy { VehicleTypeId = 99 };
        var input = new UpdateRoomChargePolicyRequestDto
        {
            Code = "ELECTRIC",
            Name = "Tiền điện",
            Amount = 3500,
            CalculationMethodCode = CALCULATION_METHOD_METER_READING
        };

        RentalChargePolicyFieldMapper.Apply(policy, input, BuildLookup(lineTypeId: 10, vehicleTypeId: null));

        policy.LineTypeId.Should().Be(10);
        policy.VehicleTypeId.Should().BeNull();
        policy.ChargeName.Should().Be("Tiền điện");
        policy.IsUsageBased.Should().BeTrue();
        policy.Amount.Should().Be(3500);
        policy.StatusId.Should().Be(30);
    }

    [Fact]
    public void Apply_Should_MapParkingPolicy_WithVehicleType()
    {
        var policy = new RentalChargePolicy();
        var input = new UpdateRoomChargePolicyRequestDto
        {
            Code = "PARKING",
            Name = "Phí giữ xe máy",
            Amount = 100000,
            CalculationMethodCode = CALCULATION_METHOD_FIXED,
            VehicleTypeCode = "MOTORBIKE"
        };

        RentalChargePolicyFieldMapper.Apply(policy, input, BuildLookup(lineTypeId: 11, vehicleTypeId: 20));

        policy.LineTypeId.Should().Be(11);
        policy.VehicleTypeId.Should().Be(20);
        policy.ChargeName.Should().Be("Phí giữ xe máy");
        policy.IsUsageBased.Should().BeFalse();
        policy.Amount.Should().Be(100000);
        policy.StatusId.Should().Be(30);
    }

    [Fact]
    public void Apply_Should_PreserveOmittedFields_When_ExistingPolicyIsPartiallyUpdated()
    {
        var policy = new RentalChargePolicy
        {
            LineTypeId = 10,
            VehicleTypeId = 20,
            ChargeName = "Tên cũ",
            IsUsageBased = true,
            Amount = 3_500,
            StatusId = 30
        };
        var input = new UpdateRoomChargePolicyRequestDto
        {
            Name = "Tên mới"
        };

        RentalChargePolicyFieldMapper.Apply(policy, input, new RentalChargePolicyLookupModel());

        policy.LineTypeId.Should().Be(10);
        policy.VehicleTypeId.Should().Be(20);
        policy.ChargeName.Should().Be("Tên mới");
        policy.IsUsageBased.Should().BeTrue();
        policy.Amount.Should().Be(3_500);
        policy.StatusId.Should().Be(30);
    }

    private static RentalChargePolicyLookupModel BuildLookup(long lineTypeId, long? vehicleTypeId)
    {
        return new RentalChargePolicyLookupModel
        {
            LineTypeId = lineTypeId,
            VehicleTypeId = vehicleTypeId,
            StatusId = 30
        };
    }
}
