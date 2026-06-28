using Authentication.Application.Commands.RegisterUserVehicle;
using Authentication.Domain.Enums;

namespace Be.Haven.Tests.Services.Authentications.Authentication.Application.Commands.RegisterUserVehicle;

public sealed class RegisterUserVehicleCommandValidatorTests
{
    [Fact]
    public void Validate_Should_Pass_When_LicensePlateIsMissing()
    {
        // Arrange
        var sut = new RegisterUserVehicleCommandValidator();
        var command = CreateValidCommand();
        command.LicensePlate = null;

        // Act
        var result = sut.Validate(command);

        // Assert
        result.Errors.Should().NotContain(x => x.PropertyName == nameof(RegisterUserVehicleCommand.LicensePlate));
    }

    [Fact]
    public void Validate_Should_Fail_When_LicensePlateExceedsMaxLength()
    {
        // Arrange
        var sut = new RegisterUserVehicleCommandValidator();
        var command = CreateValidCommand();
        command.LicensePlate = new string('A', 51);

        // Act
        var result = sut.Validate(command);

        // Assert
        result.Errors.Should().Contain(x => x.PropertyName == nameof(RegisterUserVehicleCommand.LicensePlate));
    }

    private static RegisterUserVehicleCommand CreateValidCommand()
    {
        return new RegisterUserVehicleCommand
        {
            VehicleType = VehicleTypeEnum.Motorbike,
            VehicleName = "Honda SH 150i",
            LicensePlate = "59-S2 123.45"
        };
    }
}
