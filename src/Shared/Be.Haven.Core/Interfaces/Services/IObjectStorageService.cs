namespace Be.Haven.Core.Interfaces.Services;

/// <summary>
/// Defines shared object-storage operations used by backend-mediated uploads.
/// </summary>
public interface IObjectStorageService
{
    /// <summary>
    /// Uploads one object using the configured storage provider.
    /// </summary>
    /// <param name="request">The object upload request.</param>
    /// <param name="cancellationToken">The token used to cancel the upload.</param>
    /// <returns>The uploaded object metadata safe to persist.</returns>
    Task<ObjectUploadResponseModel> UploadAsync(
        ObjectUploadRequestModel request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes one object from the configured storage provider.
    /// </summary>
    /// <param name="objectKey">The provider object key to delete.</param>
    /// <param name="cancellationToken">The token used to cancel the delete call.</param>
    /// <returns><c>true</c> when the object was deleted or already absent; otherwise <c>false</c>.</returns>
    Task<bool> DeleteAsync(
        string objectKey,
        CancellationToken cancellationToken = default);
}
