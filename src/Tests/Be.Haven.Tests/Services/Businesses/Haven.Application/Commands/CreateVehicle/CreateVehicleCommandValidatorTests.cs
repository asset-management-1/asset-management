using Haven.Application.Commands.CreateVehicle;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Commands.CreateVehicle;

public sealed class CreateVehicleCommandValidatorTests
{
    private readonly CreateVehicleCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_RequiredVehicleFieldsAreSubmitted()
    {
        var command = new CreateVehicleCommand
        {
            RoomId = Guid.NewGuid(),
            PayerTenantId = Guid.NewGuid(),
            VehicleTypeCode = "MOTORBIKE",
            Name = "Honda SH 150i"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_PayerTenantIsMissing()
    {
        var command = new CreateVehicleCommand
        {
            RoomId = Guid.NewGuid(),
            VehicleTypeCode = "MOTORBIKE",
            Name = "Honda SH 150i"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(CreateVehicleCommand.PayerTenantId));
    }

    [Fact]
    public void Validate_Should_Fail_When_SubmittedVehicleImageExceedsTenMegabytes()
    {
        // Arrange
        var imageLength = (10 * 1024 * 1024) + 1;
        var command = new CreateVehicleCommand
        {
            RoomId = Guid.NewGuid(),
            PayerTenantId = Guid.NewGuid(),
            VehicleTypeCode = "MOTORBIKE",
            Name = "Honda SH 150i",
            VehicleFrontImage = new FormFile(
                new MemoryStream(new byte[imageLength]),
                0,
                imageLength,
                "vehicleFrontImage",
                "vehicle.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(CreateVehicleCommand.VehicleFrontImage));
    }

    [Theory]
    [InlineData("vehicle.exe", "image/jpeg")]
    public void Validate_Should_Fail_When_ImageExtensionIsUnsupported(string fileName, string contentType)
    {
        // Arrange
        var command = new CreateVehicleCommand
        {
            RoomId = Guid.NewGuid(),
            PayerTenantId = Guid.NewGuid(),
            VehicleTypeCode = "MOTORBIKE",
            Name = "Honda SH 150i",
            VehicleFrontImage = new FormFile(
                new MemoryStream([1, 2, 3]),
                0,
                3,
                "vehicleFrontImage",
                fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(CreateVehicleCommand.VehicleFrontImage));
    }
}
