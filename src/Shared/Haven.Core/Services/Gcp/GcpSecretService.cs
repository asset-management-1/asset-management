namespace Haven.Core.Services.Gcp;

public sealed class GcpSecretService : IGcpSecretService
{
    private readonly SecretManagerServiceClient _client;
    private readonly ILogger<GcpSecretService> _logger;
    private readonly ICachingService _cachingService;

    // Resource name prefix, e.g. "projects/my-project/secrets"
    private readonly string _resourcePrefix;

    // Default version to use when none is specified (e.g. "latest")
    private readonly string _defaultVersion;

      /// <summary>
    /// Service for interacting with Google Cloud Platform (GCP) secrets.
    /// - Provides functionality to fetch and manage secrets stored in GCP's Secret Manager.
    /// </summary>
    public GcpSecretService(
        SecretManagerServiceClient client,
        ILogger<GcpSecretService> logger,
        ICachingService cachingService,
        string resourcePrefix,
        string defaultVersion)
    {
        _client = client;
        _logger = logger;
        _cachingService = cachingService;
        _resourcePrefix = resourcePrefix;
        _defaultVersion = defaultVersion;
    }


    /// <summary>
    /// Retrieves a secret value as UTF-8 text.
    /// - Uses cache by default (key: "{secretId}/versions/{version}", TTL: 4 hours).
    /// - Falls back to GCP Secret Manager on cache miss.
    /// </summary>
    /// <param name="secretId">The unique identifier of the secret in GCP Secret Manager.</param>
    /// <param name="version">
    /// The specific version of the secret to retrieve. If not provided, the default version is used.
    /// </param>
    /// <param name="isCached">
    /// A flag indicating whether the retrieval should utilize caching. Defaults to <c>true</c>.
    /// </param>
    /// <param name="ct">A cancellation token for the asynchronous operation.</param>
    /// <returns>The value of the secret as a string.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the <paramref name="secretId"/> is null, empty, or consists solely of whitespace.
    /// </exception>
    public async Task<string> GetByIdAsync(
        string secretId,
        string version = null,
        bool isCached = true,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(secretId))
        {
            _logger.LogWarning(SECRET_ID_REQUIRED);
            throw new ArgumentException(SECRET_ID_REQUIRED);
        }

        string value;
        if (isCached)
        {
            var ver = ResolveVersion(version);
            var cacheKey = BuildCacheKey(secretId, ver); // "{secretId}/versions/{ver}"
        
            // 1) Cache-first (unless disabled)
            var cached = await _cachingService.GetAsync<string>(cacheKey, ct);
            if (!string.IsNullOrWhiteSpace(cached))
            {
                _logger.LogDebug(LOG_CACHE_VALUE_RETRIEVED, cacheKey);
                return cached;
            }

            _logger.LogDebug(LOG_CACHE_VALUE_NOT_FOUND, cacheKey);

            // 2) Fetch bytes from GCP (no cache here)
            var bytes = await GetBytesAsync(secretId, version, ct);
            value = UTF8.GetString(bytes);

            // 3) Store cache (absolute 4h)
            await _cachingService.SetAbsoluteAsync(cacheKey, value, TimeSpan.FromHours(4), ct);
        }
        else
        {
            // 1) Fetch bytes from GCP (no cache here)
            var bytes = await GetBytesAsync(secretId, version, ct);
            value = UTF8.GetString(bytes);
        }
        
        return value;
    }

    /// <summary>
    /// Retrieves a secret as JSON, deserializes it into the specified type, and returns the result.
    /// - Fetches the secret's value from GCP's Secret Manager.
    /// - Performs JSON deserialization of the retrieved data into the given type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the JSON content into.</typeparam>
    /// <param name="secretId">The identifier of the secret to retrieve.</param>
    /// <param name="version">The version of the secret to retrieve. Defaults to the configured default version if not specified.</param>
    /// <param name="ct">A CancellationToken to observe the cancellation of the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the deserialized object of type <typeparamref name="T"/>.</returns>
    public async Task<T> GetJsonAsync<T>(
        string secretId,
        string version = null,
        CancellationToken ct = default)
    {
        var json = await GetByIdAsync(secretId, version, true, ct);
        return JsonConvert.DeserializeObject<T>(json);
    }

    /// <summary>
    /// Retrieves the value of a secret stored in Google Cloud Platform (GCP) Secret Manager as a byte array.
    /// - Allows fetching the raw binary data associated with a secret.
    /// </summary>
    /// <param name="secretId">The identifier of the secret to retrieve. This value must not be null or whitespace.</param>
    /// <param name="version">The secret version to fetch. If null or not specified, the default version will be used.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A byte array representing the value of the requested secret.</returns>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="secretId"/> is null, empty, or contains only whitespace.</exception>
    public async Task<byte[]> GetBytesAsync(
        string secretId,
        string version = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(secretId))
        {
            _logger.LogWarning(SECRET_ID_REQUIRED);
            throw new ArgumentException(SECRET_ID_REQUIRED);
        }

        var ver = ResolveVersion(version);
        var name = $"{_resourcePrefix}/{secretId}/{VERSIONS_SEGMENT}/{ver}";

        var resp = await _client.AccessSecretVersionAsync(name, ct);
        return resp.Payload.Data.ToByteArray();
    }

    /// <summary>
    /// Checks whether a secret exists in Google Cloud Platform's Secret Manager.
    /// - Attempts to retrieve the secret by its ID to determine its existence.
    /// </summary>
    /// <param name="secretId">The ID of the secret to check.</param>
    /// <param name="ct">An optional cancellation token to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing:
    /// - <c>true</c> if the secret exists in the Secret Manager;
    /// - <c>false</c> otherwise.
    /// </returns>
    public async Task<bool> ExistsAsync(string secretId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(secretId))
            return false;

        var secretName = $"{_resourcePrefix}/{secretId}";
        try
        {
            _ = await _client.GetSecretAsync(secretName, ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LOG_EXISTS_FAILED, secretName);
            return false;
        }
    }

    /// <summary>
    /// Determines whether a specific version of a secret exists in the Google Cloud Platform Secret Manager.
    /// </summary>
    /// <param name="secretId">The identifier of the secret to check.</param>
    /// <param name="version">The version of the secret to look for. If null, the default version will be used.</param>
    /// <param name="ct">The cancellation token used to propagate notifications that the operation should be canceled.</param>
    /// <returns>Returns <c>true</c> if the specified version of the secret exists; otherwise, returns <c>false</c>.</returns>
    public async Task<bool> VersionExistsAsync(
        string secretId,
        string version = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(secretId))
            return false;

        var ver = ResolveVersion(version);
        var versionName = $"{_resourcePrefix}/{secretId}/{VERSIONS_SEGMENT}/{ver}";
        try
        {
            _ = await _client.GetSecretVersionAsync(versionName, ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LOG_VERSION_EXISTS_FAILED, versionName);
            return false;
        }
    }

    /// <summary>
    /// Asynchronously lists the versions of a specified secret in Google Cloud Platform's Secret Manager.
    /// - Retrieves the versions of the secret based on the provided secret ID and paging configuration.
    /// </summary>
    /// <param name="secretId">The unique identifier of the secret whose versions are to be listed. Cannot be null, empty, or whitespace.</param>
    /// <param name="pageSize">The maximum number of versions to retrieve in a single request. Defaults to the configured default page size.</param>
    /// <param name="ct">The cancellation token to observe while waiting for the asynchronous operation to complete.</param>
    /// <returns>An asynchronous stream of <see cref="SecretVersion"/> objects representing the versions of the specified secret.</returns>
    public async IAsyncEnumerable<SecretVersion> ListVersionsAsync(
        string secretId,
        int pageSize = DEFAULT_PAGE_SIZE,
        [EnumeratorCancellation]
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(secretId))
            yield break;

        var parent = $"{_resourcePrefix}/{secretId}";
        var request = new ListSecretVersionsRequest
        {
            ParentAsSecretName = SecretName.Parse(parent),
            PageSize = pageSize
        };

        var pager = _client.ListSecretVersionsAsync(request);
        await foreach (var ver in pager.WithCancellation(ct))
            yield return ver;
    }

    /// <summary>
    /// Resolves the version of a secret to utilize when interacting with the secret.
    /// If the provided version is null or empty, the default version is returned.
    /// </summary>
    /// <param name="version">The version of the secret to resolve. Can be null or empty.</param>
    /// <returns>The resolved version. Returns the default version if the input is null or empty.</returns>
    private string ResolveVersion(string version) => string.IsNullOrWhiteSpace(version) ? _defaultVersion : version;

    /// <summary>
    /// Constructs a cache key for a secret using the provided secret ID and version.
    /// The cache key follows the format "{secretId}/versions/{version}".
    /// </summary>
    /// <param name="secretId">The identifier of the secret.</param>
    /// <param name="version">The version of the secret.</param>
    /// <returns>A string representing the constructed cache key.</returns>
    private static string BuildCacheKey(string secretId, string version) => $"{secretId}/{VERSIONS_SEGMENT}/{version}";
}