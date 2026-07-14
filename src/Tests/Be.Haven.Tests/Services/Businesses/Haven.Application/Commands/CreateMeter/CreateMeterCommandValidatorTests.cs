using Haven.Application.Commands.CreateMeter;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Commands.CreateMeter;

public sealed class CreateMeterCommandValidatorTests
{
    [Fact]
    public void Command_Should_ExposeShortMeterFieldNames()
    {
        var propertyNames = typeof(CreateMeterCommand)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();

        propertyNames.Should().Contain([
            nameof(CreateMeterCommand.ElectricPrevious),
            nameof(CreateMeterCommand.ElectricCurrent),
            nameof(CreateMeterCommand.WaterPrevious),
            nameof(CreateMeterCommand.WaterCurrent)
        ]);
        propertyNames.Should().NotContain(name => name.EndsWith("Reading", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Validate_Should_AcceptFlatElectricFields()
    {
        var validator = CreateValidator();
        var command = new CreateMeterCommand
        {
            RoomId = Guid.NewGuid(),
            BillingDate = new DateTime(2026, 7, 5),
            ElectricPrevious = 100,
            ElectricCurrent = 125,
            ElectricPrice = 3500
        };

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_Should_RejectCurrentBelowManualPrevious()
    {
        var validator = CreateValidator();
        var command = new CreateMeterCommand
        {
            RoomId = Guid.NewGuid(),
            BillingDate = new DateTime(2026, 7, 5),
            WaterPrevious = 100,
            WaterCurrent = 99,
            WaterPrice = 25000
        };

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_Should_RequireBillingDate()
    {
        var validator = CreateValidator();
        var command = new CreateMeterCommand
        {
            RoomId = Guid.NewGuid(),
            ElectricCurrent = 125
        };

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_Should_RejectEvidenceImageOverUploadLimit()
    {
        var validator = CreateValidator();
        var command = new CreateMeterCommand
        {
            RoomId = Guid.NewGuid(),
            BillingDate = new DateTime(2026, 7, 5),
            ElectricCurrent = 125,
            ElectricImages =
            [
                new FormFile(
                Stream.Null,
                baseStreamOffset: 0,
                length: MAX_IMAGE_UPLOAD_BYTES + 1,
                name: nameof(CreateMeterCommand.ElectricImages),
                fileName: "electric-meter.jpg")
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "image/jpeg"
                }
            ]
        };

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_Should_RejectPdfEvidenceContent()
    {
        var validator = CreateValidator();
        using var document = new PdfSharp.Pdf.PdfDocument();
        document.AddPage();
        using var stream = new MemoryStream();
        document.Save(stream, false);
        var content = stream.ToArray();
        var command = new CreateMeterCommand
        {
            RoomId = Guid.NewGuid(),
            BillingDate = new DateTime(2026, 7, 5),
            ElectricCurrent = 125,
            ElectricImages =
            [
                new FormFile(
                new MemoryStream(content),
                baseStreamOffset: 0,
                length: content.Length,
                name: nameof(CreateMeterCommand.ElectricImages),
                fileName: "electric-meter.pdf")
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "application/pdf"
                }
            ]
        };

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_Should_RejectMoreThanFiveElectricImages()
    {
        var validator = CreateValidator();
        var image = new FormFile(Stream.Null, 0, 1, "file", "electric-meter.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };
        var command = new CreateMeterCommand
        {
            RoomId = Guid.NewGuid(),
            BillingDate = new DateTime(2026, 7, 5),
            ElectricCurrent = 125,
            ElectricImages = [image, image, image, image, image, image]
        };

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
    }

    private static CreateMeterCommandValidator CreateValidator()
    {
        var imageValidationService = new Mock<IImageOptimizationService>();
        imageValidationService
            .Setup(service => service.ValidateAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return new CreateMeterCommandValidator(imageValidationService.Object);
    }
}
