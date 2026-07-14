using PdfSharp.Pdf.IO;

namespace Be.Haven.Core.Helpers;

/// <summary>
/// Provides provider-neutral content checks for multipart files accepted by public APIs.
/// </summary>
public static class FileContentValidationHelper
{
    /// <summary>
    /// Verifies that an optional multipart file can be fully decoded as one supported image type.
    /// </summary>
    /// <param name="file">The optional multipart image file to inspect.</param>
    /// <param name="imageValidationService">The shared decoder used for supported image formats.</param>
    /// <param name="cancellationToken">The token used to cancel content reads.</param>
    /// <returns><c>true</c> when the file is absent or its bytes decode as a supported image.</returns>
    public static async Task<bool> IsSupportedImageAsync(
        IFormFile file,
        IImageOptimizationService imageValidationService,
        CancellationToken cancellationToken = default)
    {
        if (file is null)
        {
            return true;
        }

        try
        {
            // The image service validates decoded bytes and extension without relying on mobile-provided MIME metadata.
            await imageValidationService.ValidateAsync(file, cancellationToken);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    /// <summary>
    /// Verifies that an optional multipart file contains either a decodable supported image or a parseable PDF document.
    /// </summary>
    /// <param name="file">The optional multipart file to inspect.</param>
    /// <param name="imageValidationService">The shared decoder used for supported image formats.</param>
    /// <param name="cancellationToken">The token used to cancel content reads.</param>
    /// <returns><c>true</c> when the file is absent or its content matches the supported contract.</returns>
    public static async Task<bool> IsSupportedImageOrPdfAsync(
        IFormFile file,
        IImageOptimizationService imageValidationService,
        CancellationToken cancellationToken = default)
    {
        if (file is null)
        {
            return true;
        }

        var hasPdfExtension = string.Equals(
            Path.GetExtension(file.FileName),
            PDF_FILE_EXTENSION,
            StringComparison.OrdinalIgnoreCase);
        var hasPdfContentType = string.Equals(
            file.ContentType,
            PDF_CONTENT_TYPE,
            StringComparison.OrdinalIgnoreCase);

        // Extension and MIME type must agree before either decoder receives the file.
        if (hasPdfExtension != hasPdfContentType)
        {
            return false;
        }

        // PDF evidence must be parsed by the installed document library instead of trusting a spoofable header.
        if (hasPdfExtension)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await using var stream = file.OpenReadStream();
                using var document = PdfReader.Open(stream, PdfDocumentOpenMode.Import);

                return document.PageCount > 0;
            }
            catch (PdfReaderException)
            {
                return false;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        try
        {
            // Image evidence must survive a full decoder pass before Application accepts the command.
            await imageValidationService.ValidateAsync(file, cancellationToken);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
