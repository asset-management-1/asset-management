namespace Be.Haven.Tests.Shared.Be.Haven.Core.Services;

public sealed class ImageOptimizationServiceTests
{
    [Fact]
    public async Task ValidateAsync_Should_Accept_When_ActualContentIsSupportedImage()
    {
        // Arrange
        using var sourceImage = new ImageMagick.MagickImage(ImageMagick.MagickColors.CornflowerBlue, 64, 64);
        sourceImage.Format = ImageMagick.MagickFormat.Png;
        using var sourceStream = new MemoryStream();
        sourceImage.Write(sourceStream);
        var imageFile = CreateFormFile(sourceStream.ToArray(), "image.png", "image/png");
        var sut = CreateService();

        // Act
        var action = () => sut.ValidateAsync(imageFile);

        // Assert
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ValidateAsync_Should_Reject_When_ExecutableBytesUseImageMetadata()
    {
        // Arrange
        var executableHeader = new byte[] { 0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00, 0x00, 0x00 };
        var imageFile = CreateFormFile(executableHeader, "malware.jpg", "image/jpeg");
        var sut = CreateService();

        // Act
        var action = () => sut.ValidateAsync(imageFile);

        // Assert
        var result = await action.Should().ThrowAsync<ArgumentException>();
        result.Which.Message.Should().Contain(IMAGE_FILE_INVALID_MESSAGE);
    }

    [Fact]
    public async Task ValidateAsync_Should_Accept_When_DecodedImageUsesGenericMobileContentType()
    {
        // Arrange
        using var sourceImage = new ImageMagick.MagickImage(ImageMagick.MagickColors.CornflowerBlue, 64, 64);
        sourceImage.Format = ImageMagick.MagickFormat.Png;
        using var sourceStream = new MemoryStream();
        sourceImage.Write(sourceStream);
        var imageFile = CreateFormFile(sourceStream.ToArray(), "image.png", "application/octet-stream");
        var sut = CreateService();

        // Act
        var action = () => sut.ValidateAsync(imageFile);

        // Assert
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task OptimizeAsync_Should_KeepSmallImageDimensionsAndEmitJpeg_When_PngIsSubmitted()
    {
        // Arrange
        using var sourceImage = new ImageMagick.MagickImage(ImageMagick.MagickColors.CornflowerBlue, 320, 180);
        sourceImage.Format = ImageMagick.MagickFormat.Png;
        using var sourceStream = new MemoryStream();
        sourceImage.Write(sourceStream);
        var imageFile = CreateFormFile(sourceStream.ToArray(), "image.png", "image/png");
        var sut = CreateService();

        // Act
        var result = await sut.OptimizeAsync(imageFile);

        // Assert
        result.ContentType.Should().Be(OPTIMIZED_IMAGE_CONTENT_TYPE);
        result.FileName.Should().EndWith(OPTIMIZED_IMAGE_FILE_EXTENSION);
        using var optimizedImage = new ImageMagick.MagickImage(result.Content);
        optimizedImage.Format.Should().Be(ImageMagick.MagickFormat.Jpeg);
        optimizedImage.Width.Should().Be(320);
        optimizedImage.Height.Should().Be(180);
    }

    [Fact]
    public async Task OptimizeAsync_Should_Reject_When_FileContentIsNotAnImage()
    {
        // Arrange
        var imageFile = CreateFormFile(Encoding.UTF8.GetBytes("not an image"), "document.txt", "text/plain");
        var sut = CreateService();

        // Act
        var action = () => sut.OptimizeAsync(imageFile);

        // Assert
        var result = await action.Should().ThrowAsync<ArgumentException>();
        result.Which.Message.Should().Contain(IMAGE_FILE_INVALID_MESSAGE);
    }

    private static IFormFile CreateFormFile(byte[] content, string fileName, string contentType)
    {
        return new FormFile(new MemoryStream(content), 0, content.LongLength, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    private static ImageOptimizationService CreateService()
    {
        return new ImageOptimizationService(Mock.Of<ILogger<ImageOptimizationService>>());
    }
}
