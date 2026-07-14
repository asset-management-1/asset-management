namespace Be.Haven.Core.Services;

/// <summary>
/// Validates supported image content and can produce privacy-safe JPEG objects when optimization is enabled.
/// </summary>
public class ImageOptimizationService : IImageOptimizationService
{
    private readonly ILogger<ImageOptimizationService> _logger;

    /// <summary>
    /// Creates the image validation and optional optimization service.
    /// </summary>
    /// <param name="logger">The structured application logger.</param>
    public ImageOptimizationService(ILogger<ImageOptimizationService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Decodes the supplied file and verifies its actual format before any storage write occurs.
    /// </summary>
    /// <param name="file">The original multipart image file.</param>
    /// <param name="cancellationToken">The token used to cancel source-stream reads.</param>
    /// <returns>A task that completes after the content is decoded successfully.</returns>
    public async Task ValidateAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        // Fully decode the submitted bytes, then dispose the image without transforming or re-encoding it.
        using var image = await DecodeAndValidateAsync(file, cancellationToken);

        _logger.LogInformation(
            ImageOptimizationLogs.IMAGE_CONTENT_VALIDATED,
            image.Format,
            file.Length,
            image.Width,
            image.Height);
    }

    /// <summary>
    /// Decodes the supplied file, keeps web-safe dimensions, removes metadata, and emits JPEG content.
    /// </summary>
    /// <param name="file">The original multipart image file.</param>
    /// <param name="cancellationToken">The token used to cancel source-stream reads.</param>
    /// <returns>The optimized JPEG content ready for storage.</returns>
    public async Task<OptimizedImageModel> OptimizeAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        using var image = await DecodeAndValidateAsync(file, cancellationToken);

        // Capture non-sensitive characteristics for operational metrics before mutating the decoded image.
        var inputFormat = image.Format;
        var originalWidth = image.Width;
        var originalHeight = image.Height;

        // Normalize orientation, remove metadata, and cap dimensions for both mobile and web rendering.
        image.AutoOrient();
        image.Strip();
        image.Resize(new MagickGeometry(OPTIMIZED_IMAGE_MAX_LONG_EDGE_PIXELS)
        {
            Greater = true
        });
        image.Format = MagickFormat.Jpeg;

        // Prefer a compact upload, but retain the final valid JPEG even when it cannot reach the target size.
        var quality = OPTIMIZED_IMAGE_INITIAL_QUALITY;
        var minimumQuality = OPTIMIZED_IMAGE_MINIMUM_QUALITY;
        var qualityStep = OPTIMIZED_IMAGE_QUALITY_STEP;
        var content = EncodeJpeg(image, quality);
        while (content.LongLength > OPTIMIZED_IMAGE_TARGET_BYTES && quality > minimumQuality)
        {
            quality = quality > minimumQuality + qualityStep
                ? quality - qualityStep
                : minimumQuality;
            content = EncodeJpeg(image, quality);
        }

        _logger.LogInformation(
            ImageOptimizationLogs.IMAGE_OPTIMIZATION_COMPLETED,
            inputFormat,
            file.Length,
            content.LongLength,
            originalWidth,
            originalHeight,
            image.Width,
            image.Height,
            quality);

        return new OptimizedImageModel
        {
            Content = content,
            ContentType = OPTIMIZED_IMAGE_CONTENT_TYPE,
            FileName = $"image{OPTIMIZED_IMAGE_FILE_EXTENSION}"
        };
    }

    /// <summary>
    /// Opens and fully decodes one supported image while enforcing the shared input limits.
    /// </summary>
    /// <param name="file">The original multipart image file.</param>
    /// <param name="cancellationToken">The token used to cancel source-stream reads.</param>
    /// <returns>The decoded ImageMagick image owned by the caller.</returns>
    private async Task<MagickImage> DecodeAndValidateAsync(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(file);
        if (file.Length <= 0)
        {
            throw new ArgumentException(IMAGE_FILE_EMPTY_MESSAGE, nameof(file));
        }

        if (file.Length > MAX_IMAGE_UPLOAD_BYTES)
        {
            throw new ArgumentOutOfRangeException(nameof(file), IMAGE_FILE_TOO_LARGE_MESSAGE);
        }

        await using var source = file.OpenReadStream();
        MagickImage image;
        try
        {
            image = new MagickImage(source);
        }
        catch (MagickException exception)
        {
            _logger.LogWarning(exception, ImageOptimizationLogs.IMAGE_OPTIMIZATION_REJECTED, file.Length);
            throw new ArgumentException(IMAGE_FILE_INVALID_MESSAGE, nameof(file), exception);
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (IsSupportedInputFormat(image.Format) && IsSubmittedExtensionConsistent(file, image.Format))
            {
                return image;
            }

            _logger.LogWarning(ImageOptimizationLogs.IMAGE_OPTIMIZATION_REJECTED, file.Length);
            throw new ArgumentException(IMAGE_FILE_INVALID_MESSAGE, nameof(file));
        }
        catch
        {
            // The caller owns only successfully returned images; every rejected or cancelled decode is released here.
            image.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Encodes one normalized ImageMagick image with the requested JPEG quality.
    /// </summary>
    /// <param name="image">The normalized source image.</param>
    /// <param name="quality">The JPEG quality used for this attempt.</param>
    /// <returns>The encoded JPEG bytes.</returns>
    private static byte[] EncodeJpeg(MagickImage image, uint quality)
    {
        image.Quality = quality;
        using var destination = new MemoryStream();
        image.Write(destination, MagickFormat.Jpeg);
        return destination.ToArray();
    }

    /// <summary>
    /// Determines whether ImageMagick decoded one of the public image formats accepted by the API.
    /// </summary>
    /// <param name="format">The format detected from decoded file content.</param>
    /// <returns><c>true</c> when the format belongs to the supported upload contract.</returns>
    private static bool IsSupportedInputFormat(MagickFormat format)
    {
        return format is MagickFormat.Jpeg
            or MagickFormat.Png
            or MagickFormat.WebP
            or MagickFormat.Heic
            or MagickFormat.Heif
            or MagickFormat.Avif;
    }

    /// <summary>
    /// Verifies that the submitted extension describes the decoded image format.
    /// </summary>
    /// <param name="file">The original multipart image file.</param>
    /// <param name="format">The format detected from the decoded file content.</param>
    /// <returns><c>true</c> when the submitted extension matches the actual image content.</returns>
    private static bool IsSubmittedExtensionConsistent(IFormFile file, MagickFormat format)
    {
        var extension = Path.GetExtension(file.FileName);
        return format switch
        {
            MagickFormat.Jpeg =>
                (string.Equals(extension, JPG_IMAGE_FILE_EXTENSION, StringComparison.OrdinalIgnoreCase)
                 || string.Equals(extension, JPEG_IMAGE_FILE_EXTENSION, StringComparison.OrdinalIgnoreCase)),
            MagickFormat.Png =>
                string.Equals(extension, PNG_IMAGE_FILE_EXTENSION, StringComparison.OrdinalIgnoreCase),
            MagickFormat.WebP =>
                string.Equals(extension, WEBP_IMAGE_FILE_EXTENSION, StringComparison.OrdinalIgnoreCase),
            MagickFormat.Heic or MagickFormat.Heif =>
                (string.Equals(extension, HEIC_IMAGE_FILE_EXTENSION, StringComparison.OrdinalIgnoreCase)
                 || string.Equals(extension, HEIF_IMAGE_FILE_EXTENSION, StringComparison.OrdinalIgnoreCase)),
            MagickFormat.Avif =>
                string.Equals(extension, AVIF_IMAGE_FILE_EXTENSION, StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }
}
