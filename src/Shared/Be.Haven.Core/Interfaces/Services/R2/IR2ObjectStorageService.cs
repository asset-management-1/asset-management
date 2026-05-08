namespace Be.Haven.Core.Interfaces.Services.R2;

/// <summary>
/// Defines private object upload operations for Cloudflare R2.
/// </summary>
public interface IR2ObjectStorageService
{
    /// <summary>
    /// Uploads one private object to the configured Cloudflare R2 bucket.
    /// </summary>
    /// <param name="request">The object upload request.</param>
    /// <param name="cancellationToken">The token used to cancel the upload.</param>
    /// <returns>The private object metadata safe to persist.</returns>
    Task<R2ObjectUploadResponse> UploadAsync(
        R2ObjectUploadRequest request,
        CancellationToken cancellationToken = default);
}
