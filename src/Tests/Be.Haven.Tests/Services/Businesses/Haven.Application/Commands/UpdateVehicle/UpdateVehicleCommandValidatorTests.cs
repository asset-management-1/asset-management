using Haven.Application.Commands.UpdateVehicle;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Commands.UpdateVehicle;

public sealed class UpdateVehicleCommandValidatorTests
{
    private readonly UpdateVehicleCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_OnlySubmittedPartialFieldIsValid()
    {
        var command = new UpdateVehicleCommand
        {
            RoomId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            Name = "Honda Vision"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
