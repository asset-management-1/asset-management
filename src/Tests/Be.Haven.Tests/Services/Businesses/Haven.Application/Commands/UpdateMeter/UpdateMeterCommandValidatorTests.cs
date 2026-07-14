using Haven.Application.Commands.UpdateMeter;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Commands.UpdateMeter;

public sealed class UpdateMeterCommandValidatorTests
{
    [Fact]
    public async Task Validate_Should_RejectEmptyMutation()
    {
        var imageValidationService = new Mock<IImageOptimizationService>();
        var validator = new UpdateMeterCommandValidator(imageValidationService.Object);
        var command = new UpdateMeterCommand
        {
            RoomId = Guid.NewGuid(),
            BillingDate = new DateTime(2026, 7, 5)
        };

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_Should_RejectMoreThanFiveWaterImages()
    {
        var imageValidationService = new Mock<IImageOptimizationService>();
        imageValidationService
            .Setup(service => service.ValidateAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var validator = new UpdateMeterCommandValidator(imageValidationService.Object);
        var image = new FormFile(Stream.Null, 0, 1, "file", "water-meter.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };
        var command = new UpdateMeterCommand
        {
            RoomId = Guid.NewGuid(),
            BillingDate = new DateTime(2026, 7, 5),
            WaterCurrent = 125,
            WaterImages = [image, image, image, image, image, image]
        };

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_Should_RejectMoreThanFiveSelectedElectricImageDeletions()
    {
        var validator = new UpdateMeterCommandValidator(new Mock<IImageOptimizationService>().Object);
        var command = new UpdateMeterCommand
        {
            RoomId = Guid.NewGuid(),
            BillingDate = new DateTime(2026, 7, 5),
            ElectricCurrent = 125,
            DeletedElectricImageIds =
            [
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()
            ]
        };

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
    }
}
