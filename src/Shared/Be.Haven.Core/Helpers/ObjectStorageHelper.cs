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
        var missingRequiredFile = (request.Files ?? []).Any(uploadFile => uploadFile.IsRequired && uploadFile.File is null);
        if (missingRequiredFile)
        {
            // Preserve the established raw-document batch contract for callers that handle storage failures uniformly.
            throw new InvalidOperationException(
                OBJECT_STORAGE_BATCH_UPLOAD_FAILED_MESSAGE,
                new ArgumentException(OBJECT_STORAGE_REQUIRED_FILE_MISSING_MESSAGE, nameof(request)));
        }

        // Raw documents retain their submitted content type; allowed image extensions receive a canonical browser-safe media type.
        var contents = (request.Files ?? [])
            .Where(uploadFile => uploadFile.File is not null)
            .Select(uploadFile => new ObjectStorageUploadContentModel
            {
                SlotName = uploadFile.SlotName,
                ObjectTag = uploadFile.ObjectTag,
                FileName = uploadFile.File.FileName,
                ContentType = ResolveStorageContentType(uploadFile.File.FileName, uploadFile.File.ContentType),
                OpenReadStream = uploadFile.File.OpenReadStream
            })
            .ToList();

        return await UploadOwnerScopedContentsAsync(objectStorageService, request, contents, cancellationToken);
    }

    /// <summary>
    /// Decodes every supplied image before uploading the original content with its original format.
    /// </summary>
    /// <param name="objectStorageService">The object-storage service used to persist image content.</param>
    /// <param name="imageOptimizationService">The image processor used only to verify actual image content.</param>
    /// <param name="request">The owner-scoped image batch request.</param>
    /// <param name="cancellationToken">The token used to cancel validation and upload work.</param>
    /// <returns>The uploaded object metadata keyed by logical slot.</returns>
    public static async Task<ObjectStorageUploadBatchResultModel> UploadOwnerScopedValidatedImageFormFilesAsync(
        IObjectStorageService objectStorageService,
        IImageOptimizationService imageOptimizationService,
        ObjectStorageUploadBatchRequestModel request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(imageOptimizationService);
        var files = request.Files ?? [];
        if (files.Any(uploadFile => uploadFile.IsRequired && uploadFile.File is null))
        {
            throw new ArgumentException(OBJECT_STORAGE_REQUIRED_FILE_MISSING_MESSAGE, nameof(request));
        }

        // Complete all content decodes before the first storage write, so a malformed later slot cannot leave objects behind.
        foreach (var file in files.Where(uploadFile => uploadFile.File is not null))
        {
            await imageOptimizationService.ValidateAsync(file.File, cancellationToken);
        }

        // Preserve the submitted bytes, content type, and extension after the decoder has accepted every image.
        return await UploadOwnerScopedFormFilesAsync(objectStorageService, request, cancellationToken);
    }

    /// <summary>
    /// Uploads prepared owner-scoped content and cleans up successful uploads if a later slot fails.
    /// </summary>
    /// <param name="objectStorageService">The object-storage service used to persist and clean up content.</param>
    /// <param name="request">The owner-scoped object-key request data.</param>
    /// <param name="contents">The content slots ready to upload.</param>
    /// <param name="cancellationToken">The token used to cancel upload calls.</param>
    /// <returns>The uploaded object metadata keyed by logical slot.</returns>
    private static async Task<ObjectStorageUploadBatchResultModel> UploadOwnerScopedContentsAsync(
        IObjectStorageService objectStorageService,
        ObjectStorageUploadBatchRequestModel request,
        IReadOnlyCollection<ObjectStorageUploadContentModel> contents,
        CancellationToken cancellationToken)
    {
        var result = new ObjectStorageUploadBatchResultModel();
        try
        {
            foreach (var content in contents)
            {
                var objectKey = BuildOwnerScopedObjectKey(
                    new ObjectStorageKeyRequestModel
                    {
                        ConfiguredPrefix = request.ConfiguredPrefix,
                        DefaultPrefix = request.DefaultPrefix,
                        OwnerPublicId = request.OwnerPublicId,
                        Tag = content.ObjectTag,
                        FileName = content.FileName
                    });
                await using var stream = content.OpenReadStream();
                result.UploadsBySlot[content.SlotName] = await objectStorageService.UploadAsync(
                    new ObjectUploadRequestModel
                    {
                        Content = stream,
                        ContentType = content.ContentType,
                        ObjectKey = objectKey
                    },
                    cancellationToken);
            }

            return result;
        }
        catch (Exception ex)
        {
            ObjectStorageCleanupResultModel cleanup = null;
            Exception cleanupException = null;

            // Compensate with an independent bounded token because the request token may already be cancelled.
            using var cleanupCts = new CancellationTokenSource(OBJECT_STORAGE_CLEANUP_TIMEOUT);

            try
            {
                cleanup = await CleanupUploadedObjectsAsync(
                    objectStorageService,
                    result.UploadsBySlot.Values,
                    cleanupCts.Token);
            }
            catch (Exception cleanupError)
            {
                cleanupException = cleanupError;
            }

            // Cancellation remains the authoritative caller outcome even when best-effort compensation is incomplete.
            if (ex is OperationCanceledException)
            {
                throw;
            }

            if (cleanupException is not null || cleanup?.HasFailures == true)
            {
                throw new InvalidOperationException(OBJECT_STORAGE_ROLLBACK_INCOMPLETE_MESSAGE, ex);
            }

            throw new InvalidOperationException(OBJECT_STORAGE_BATCH_UPLOAD_FAILED_MESSAGE, ex);
        }
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
        return await CleanupObjectKeysAsync(
            objectStorageService,
            (uploads ?? [])
                .Where(upload => upload is not null)
                .Select(upload => upload.ObjectKey),
            cancellationToken);
    }

    /// <summary>
    /// Selects a canonical content type for an allowed image extension while preserving document metadata for other files.
    /// </summary>
    /// <param name="fileName">The submitted file name used to identify the approved image extension.</param>
    /// <param name="submittedContentType">The multipart content type supplied by the client.</param>
    /// <returns>A browser-safe image media type or the submitted type for non-image files.</returns>
    private static string ResolveStorageContentType(
        string fileName,
        string submittedContentType)
    {
        // Image byte validation happens at the Application boundary; this only prevents generic mobile MIME metadata from reaching storage.
        return Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            JPG_IMAGE_FILE_EXTENSION or JPEG_IMAGE_FILE_EXTENSION => JPEG_IMAGE_CONTENT_TYPE,
            PNG_IMAGE_FILE_EXTENSION => PNG_IMAGE_CONTENT_TYPE,
            WEBP_IMAGE_FILE_EXTENSION => WEBP_IMAGE_CONTENT_TYPE,
            HEIC_IMAGE_FILE_EXTENSION => HEIC_IMAGE_CONTENT_TYPE,
            HEIF_IMAGE_FILE_EXTENSION => HEIF_IMAGE_CONTENT_TYPE,
            AVIF_IMAGE_FILE_EXTENSION => AVIF_IMAGE_CONTENT_TYPE,
            _ => submittedContentType
        };
    }

    /// <summary>
    /// Deletes object-storage entries by their durable keys.
    /// </summary>
    /// <param name="objectStorageService">The object-storage service used to delete objects.</param>
    /// <param name="objectKeys">The object keys to delete.</param>
    /// <param name="cancellationToken">The token used to cancel delete calls.</param>
    /// <returns>The best-effort cleanup result.</returns>
    public static async Task<ObjectStorageCleanupResultModel> CleanupObjectKeysAsync(
        IObjectStorageService objectStorageService,
        IEnumerable<string> objectKeys,
        CancellationToken cancellationToken = default)
    {
        // Filter blank and duplicate keys before launching independent best-effort cleanup calls.
        var uniqueObjectKeys = (objectKeys ?? [])
            .Where(objectKey => !string.IsNullOrWhiteSpace(objectKey))
            .Distinct(StringComparer.Ordinal)
            .ToList();
        if (uniqueObjectKeys.Count == 0)
        {
            return new ObjectStorageCleanupResultModel();
        }

        // Delete objects in parallel because cleanup is best-effort and independent per object.
        var cleanupResults = await Task.WhenAll(
            uniqueObjectKeys.Select(objectKey => objectStorageService.DeleteAsync(
                objectKey,
                cancellationToken)));

        return new ObjectStorageCleanupResultModel
        {
            AttemptedCount = uniqueObjectKeys.Count,
            FailedCount = cleanupResults.Count(x => !x)
        };
    }

    /// <summary>
    /// Resolves the configured storage prefix or falls back to the provider default.
    /// </summary>
    /// <param name="configuredPrefix">The configured prefix from application settings.</param>
    /// <param name="defaultPrefix">The default prefix used when settings are empty.</param>
    /// <returns>The normalized object prefix without leading or trailing slashes.</returns>
    private static string ResolvePrefix(string configuredPrefix, string defaultPrefix)
    {
        // Storage prefixes are persisted as path segments without leading or trailing separators.
        return string.IsNullOrWhiteSpace(configuredPrefix)
            ? defaultPrefix
            : configuredPrefix.Trim(OBJECT_KEY_SEPARATOR);
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
        var timestamp = DateTime.UtcNow.ToString(OBJECT_KEY_TIMESTAMP_FORMAT, CultureInfo.InvariantCulture);
        var shortId = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture)[..OBJECT_KEY_RANDOM_SUFFIX_LENGTH];

        return string.Format(
            CultureInfo.InvariantCulture,
            OWNER_SCOPED_OBJECT_KEY_FORMAT,
            prefix,
            request.OwnerPublicId,
            request.Tag,
            timestamp,
            shortId,
            extension);
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
            : $"{publicBaseUrl.TrimEnd(OBJECT_KEY_SEPARATOR)}{OBJECT_KEY_SEPARATOR}{objectKey}";
    }

    /// <summary>
    /// Extracts the durable object key from a stored public URL or private object key.
    /// </summary>
    /// <param name="publicBaseUrl">The configured public base URL, when objects are publicly served.</param>
    /// <param name="objectUrlOrKey">The persisted public URL or private object key.</param>
    /// <returns>The durable object key, or <c>null</c> when the value is blank or outside the configured base URL.</returns>
    public static string GetObjectKey(string publicBaseUrl, string objectUrlOrKey)
    {
        if (string.IsNullOrWhiteSpace(objectUrlOrKey))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(publicBaseUrl))
        {
            return objectUrlOrKey;
        }

        var normalizedBaseUrl = $"{publicBaseUrl.TrimEnd(OBJECT_KEY_SEPARATOR)}{OBJECT_KEY_SEPARATOR}";
        return objectUrlOrKey.StartsWith(normalizedBaseUrl, StringComparison.Ordinal)
            ? objectUrlOrKey[normalizedBaseUrl.Length..]
            : null;
    }
}
