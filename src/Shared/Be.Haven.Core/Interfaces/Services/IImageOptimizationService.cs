namespace Be.Haven.Core.Interfaces.Services;

/// <summary>
/// Validates actual image content and optionally optimizes it into a canonical storage representation.
/// </summary>
public interface IImageOptimizationService
{
    /// <summary>
    /// Decodes one upload and verifies that its actual content is a supported image.
    /// </summary>
    /// <param name="file">The image uploaded through multipart form data.</param>
    /// <param name="cancellationToken">The token used to cancel stream reads.</param>
    /// <returns>A task that completes after the image content is validated.</returns>
    Task ValidateAsync(IFormFile file, CancellationToken cancellationToken = default);

    /// <summary>
    /// Decodes, normalizes, and re-encodes one image for object storage.
    /// </summary>
    /// <param name="file">The original image uploaded through multipart form data.</param>
    /// <param name="cancellationToken">The token used to cancel stream reads.</param>
    /// <returns>The optimized JPEG content and storage metadata.</returns>
    Task<OptimizedImageModel> OptimizeAsync(IFormFile file, CancellationToken cancellationToken = default);
}
