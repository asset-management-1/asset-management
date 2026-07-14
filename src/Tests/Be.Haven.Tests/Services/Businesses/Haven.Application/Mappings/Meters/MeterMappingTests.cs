using Haven.Application.Commands.CreateMeter;
using Haven.Application.Dtos.Meters.Common;
using Haven.Application.Mappings.Meters;
using Haven.Application.Models.Meters.Mutation;
using Haven.Application.Models.Meters.Rows;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Mappings.Meters;

public sealed class MeterMappingTests
{
    [Fact]
    public void Adapt_Should_MapValidatedFlatMutationFields()
    {
        var config = new TypeAdapterConfig();
        new MeterMapping().Register(config);
        var billingDate = new DateTime(2026, 7, 5);
        var roomId = Guid.NewGuid();
        var command = new CreateMeterCommand
        {
            RoomId = roomId,
            BillingDate = billingDate,
            ElectricPrevious = 100,
            ElectricCurrent = 125,
            ElectricPrice = 3_500,
            WaterPrevious = 40,
            WaterCurrent = 47,
            WaterPrice = 25_000
        };

        var request = command.Adapt<MeterMutationRequestModel>(config);

        request.RoomId.Should().Be(roomId);
        request.BillingDate.Should().Be(billingDate);
        request.ElectricPrevious.Should().Be(100);
        request.ElectricCurrent.Should().Be(125);
        request.ElectricPrice.Should().Be(3_500);
        request.WaterPrevious.Should().Be(40);
        request.WaterCurrent.Should().Be(47);
        request.WaterPrice.Should().Be(25_000);
    }

    [Theory]
    [InlineData(false, UTILITY_PREVIOUS_SOURCE_MANUAL)]
    [InlineData(true, UTILITY_PREVIOUS_SOURCE_PRIOR_CONFIRMED)]
    public void Adapt_Should_MapPreviousSourceFromConfirmedHistory(
        bool hasPriorConfirmedReading,
        string expectedSourceCode)
    {
        var config = new TypeAdapterConfig();
        new MeterMapping().Register(config);
        var row = new MeterRowModel
        {
            HasPriorConfirmedReading = hasPriorConfirmedReading
        };

        var response = row.Adapt<MeterValueResponseDto>(config);

        response.PreviousSourceCode.Should().Be(expectedSourceCode);
    }
}
