namespace Be.Haven.Core.Helpers;

/// <summary>
/// Provides shared helper methods for backend-mediated object storage uploads.
/// </summary>
public static class ObjectStorageHelper
{
    /// <summary>
    /// Uploads an owner-scoped batch of form files and cleans up successful uploads if a later upload fails.
    /// </summary>
    /// <param name="objectStorageService">The object-storage service used to persist and clean up file content.</param>
    /// <param name="request">The batch upload request.</param>
    /// <param name="cancellationToken">The token used to cancel upload calls.</param>
    /// <returns>The uploaded object metadata keyed by logical slot.</returns>
    public static async Task<ObjectStorageUploadBatchResultModel> UploadOwnerScopedFormFilesAsync(
        IObjectStorageService objectStorageService,
        ObjectStorageUploadBatchRequestModel request,
        CancellationToken cancellationToken = default)
    {
        // Track successful uploads so a later slot failure can remove orphaned objects.
        var result = new ObjectStorageUploadBatchResultModel();

        try
        {
            foreach (var uploadFile in request.Files ?? [])
            {
                if (uploadFile.File is null)
                {
                    if (uploadFile.IsRequired)
                    {
                        throw new ArgumentException(OBJECT_STORAGE_REQUIRED_FILE_MISSING_MESSAGE);
                    }

                    continue;
                }

                // Build one owner-scoped object key per slot before streaming the file to storage.
                var objectKey = BuildOwnerScopedObjectKey(
                    new ObjectStorageKeyRequestModel
                    {
                        ConfiguredPrefix = request.ConfiguredPrefix,
                        DefaultPrefix = request.DefaultPrefix,
                        OwnerPublicId = request.OwnerPublicId,
                        Tag = uploadFile.ObjectTag,
                        FileName = uploadFile.File.FileName
                    });
                result.UploadsBySlot[uploadFile.SlotName] = await UploadFormFileAsync(
                    objectStorageService,
                    uploadFile.File,
                    objectKey,
                    cancellationToken);
            }

            return result;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Batch upload cleanup is best-effort; the caller owns business logging and error conversion.
            await CleanupUploadedObjectsAsync(
                objectStorageService,
                result.UploadsBySlot.Values,
                cancellationToken);

            throw;
        }
    }

    /// <summary>
    /// Uploads an owner-scoped batch of form files and maps the uploaded metadata to a caller-owned result type.
    /// </summary>
    /// <typeparam name="TResult">The caller-owned result type.</typeparam>
    /// <param name="objectStorageService">The object-storage service used to persist and clean up file content.</param>
    /// <param name="request">The batch upload request.</param>
    /// <param name="resultFactory">The mapper that converts uploaded slots into the caller-owned result.</param>
    /// <param name="cancellationToken">The token used to cancel upload calls.</param>
    /// <returns>The mapped caller-owned upload result.</returns>
    public static async Task<TResult> UploadOwnerScopedFormFilesAsync<TResult>(
        IObjectStorageService objectStorageService,
        ObjectStorageUploadBatchRequestModel request,
        Func<ObjectStorageUploadBatchResultModel, TResult> resultFactory,
        CancellationToken cancellationToken = default)
    {
        // Reuse the non-generic upload flow so cleanup behavior stays identical for every result shape.
        var uploadResult = await UploadOwnerScopedFormFilesAsync(
            objectStorageService,
            request,
            cancellationToken);

        return resultFactory(uploadResult);
    }

    /// <summary>
    /// Uploads one form file through the configured object-storage service.
    /// </summary>
    /// <param name="objectStorageService">The object-storage service used to persist the file content.</param>
    /// <param name="file">The uploaded form file.</param>
    /// <param name="objectKey">The destination object key.</param>
    /// <param name="cancellationToken">The token used to cancel the upload.</param>
    /// <returns>The uploaded object metadata safe to persist.</returns>
    public static async Task<ObjectUploadResponseModel> UploadFormFileAsync(
        IObjectStorageService objectStorageService,
        IFormFile file,
        string objectKey,
        CancellationToken cancellationToken = default)
    {
        // Keep the form-file stream lifetime scoped to the upload operation.
        await using var stream = file.OpenReadStream();

        return await objectStorageService.UploadAsync(
            new ObjectUploadRequestModel
            {
                Content = stream,
                ContentType = file.ContentType,
                ObjectKey = objectKey
            },
            cancellationToken);
    }

    /// <summary>
    /// Deletes uploaded objects by unique object key.
    /// </summary>
    /// <param name="objectStorageService">The object-storage service used to delete objects.</param>
    /// <param name="uploads">The uploaded object metadata to clean up.</param>
    /// <param name="cancellationToken">The token used to cancel delete calls.</param>
    /// <returns>The best-effort cleanup result.</returns>
    public static async Task<ObjectStorageCleanupResultModel> CleanupUploadedObjectsAsync(
        IObjectStorageService objectStorageService,
        IEnumerable<ObjectUploadResponseModel> uploads,
        CancellationToken cancellationToken = default)
    {
        // Delegate to the generic cleanup overload using the standard object-upload key selector.
        return await CleanupUploadedObjectsAsync(
            objectStorageService,
            uploads,
            upload => upload.ObjectKey,
            cancellationToken);
    }

    /// <summary>
    /// Deletes uploaded objects by unique object key selected from caller-owned upload metadata.
    /// </summary>
    /// <typeparam name="TUpload">The caller-owned upload metadata type.</typeparam>
    /// <param name="objectStorageService">The object-storage service used to delete objects.</param>
    /// <param name="uploads">The uploaded metadata to clean up.</param>
    /// <param name="objectKeySelector">The selector used to read a durable object key from each upload item.</param>
    /// <param name="cancellationToken">The token used to cancel delete calls.</param>
    /// <returns>The best-effort cleanup result.</returns>
    public static async Task<ObjectStorageCleanupResultModel> CleanupUploadedObjectsAsync<TUpload>(
        IObjectStorageService objectStorageService,
        IEnumerable<TUpload> uploads,
        Func<TUpload, string> objectKeySelector,
        CancellationToken cancellationToken = default)
    {
        // Filter null and duplicate object keys before launching independent delete calls.
        var objectKeys = (uploads ?? [])
            .Where(x => x is not null)
            .Select(objectKeySelector)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .ToList();
        if (objectKeys.Count == 0)
        {
            return new ObjectStorageCleanupResultModel();
        }

        // Delete objects in parallel because cleanup is best-effort and independent per object.
        var cleanupResults = await Task.WhenAll(
            objectKeys.Select(objectKey => objectStorageService.DeleteAsync(
                objectKey,
                cancellationToken)));

        return new ObjectStorageCleanupResultModel
        {
            AttemptedCount = objectKeys.Count,
            FailedCount = cleanupResults.Count(x => !x)
        };
    }

    /// <summary>
    /// Resolves the configured storage prefix or falls back to the provider default.
    /// </summary>
    /// <param name="configuredPrefix">The configured prefix from application settings.</param>
    /// <param name="defaultPrefix">The default prefix used when settings are empty.</param>
    /// <returns>The normalized object prefix without leading or trailing slashes.</returns>
    public static string ResolvePrefix(string configuredPrefix, string defaultPrefix)
    {
        // Storage prefixes are persisted as path segments without leading or trailing separators.
        return string.IsNullOrWhiteSpace(configuredPrefix)
            ? defaultPrefix
            : configuredPrefix.Trim('/');
    }

    /// <summary>
    /// Builds a unique object key under a normalized prefix and owner identifier.
    /// </summary>
    /// <param name="request">The owner-scoped object-key request.</param>
    /// <returns>The generated object key.</returns>
    public static string BuildOwnerScopedObjectKey(ObjectStorageKeyRequestModel request)
    {
        // Reuse the common owner-scoped path shape for every backend-mediated upload.
        var prefix = ResolvePrefix(request.ConfiguredPrefix, request.DefaultPrefix);
        var extension = Path.GetExtension(request.FileName);
        var objectName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}-{request.Tag}-{Guid.NewGuid():N}";

        return string.Format(OWNER_SCOPED_OBJECT_KEY_FORMAT, prefix, request.OwnerPublicId, objectName, extension);
    }

    /// <summary>
    /// Builds an externally displayable object URL when a public base URL is configured.
    /// </summary>
    /// <param name="publicBaseUrl">The configured public base URL.</param>
    /// <param name="objectKey">The uploaded object key.</param>
    /// <returns>The public URL when configured; otherwise the durable object key.</returns>
    public static string BuildObjectUrl(string publicBaseUrl, string objectKey)
    {
        // Optional uploads may not produce an object key, so preserve the caller's null or blank value.
        if (string.IsNullOrWhiteSpace(objectKey))
        {
            return objectKey;
        }

        // Keep private-object callers stable by returning the object key when no public base URL exists.
        return string.IsNullOrWhiteSpace(publicBaseUrl)
            ? objectKey
            : $"{publicBaseUrl.TrimEnd('/')}/{objectKey}";
    }
}
