namespace Be.Haven.Core.Files;

public sealed class ResourceFileProvider : IResourceFileProvider
{
    private readonly Assembly _assembly;
    private readonly IReadOnlyDictionary<string, string> _map;
    private readonly ILogger<ResourceFileProvider> _logger;

    /// <summary>
    /// Provides functionality for accessing resource files embedded in an assembly.
    /// </summary>
    public ResourceFileProvider(
        Assembly assembly,
        IReadOnlyDictionary<string, string> map,
        ILogger<ResourceFileProvider> logger)
    {
        _assembly = assembly;
        _map = map;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves the resource file associated with the provided key as a byte array asynchronously.
    /// </summary>
    /// <param name="key">The key associated with the resource file to retrieve.</param>
    /// <param name="ct">An optional CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>A Task representing the asynchronous operation, containing the resource file as a byte array.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the provided key is not configured for any resource file.
    /// </exception>
    public async Task<byte[]> GetAsync(string key, CancellationToken ct = default)
    {
        _logger.LogInformation(
            EmbeddedResourceExcelSourceProviderLogs.LOG_GET_EXCEL_STARTED,
            key);

        if (!_map.TryGetValue(key, out var resourceName))
        {
            _logger.LogWarning(
                EmbeddedResourceExcelSourceProviderLogs.LOG_EXCEL_KEY_NOT_CONFIGURED,
                key);

            throw new InvalidOperationException(
                string.Format(
                    ResourceFileProviderErrors.ERR_EXCEL_KEY_NOT_CONFIGURED,
                    key));
        }

        await using var stream = _assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            var availableResources = string.Join(ONE_LINE_SEPARATOR, _assembly.GetManifestResourceNames());

            _logger.LogError(
                EmbeddedResourceExcelSourceProviderLogs.LOG_RESOURCE_STREAM_NOT_FOUND,
                resourceName,
                availableResources);

            throw new InvalidOperationException(
                string.Format(
                    ResourceFileProviderErrors.ERR_EMBEDDED_RESOURCE_NOT_FOUND,
                    resourceName,
                    availableResources));
        }

        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, ct);

        var result = ms.ToArray();

        _logger.LogInformation(
            EmbeddedResourceExcelSourceProviderLogs.LOG_EXCEL_STREAM_COPY_COMPLETED,
            resourceName,
            result.Length);

        return result;
    }
}