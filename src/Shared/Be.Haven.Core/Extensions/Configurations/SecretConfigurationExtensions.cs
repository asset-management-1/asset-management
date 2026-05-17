namespace Be.Haven.Core.Extensions.Configurations;

/// <summary>
/// Provides startup helpers for applying Secret Manager values before options binding.
/// </summary>
public static class SecretConfigurationExtensions
{
    /// <summary>
    /// Applies configured secret values into an in-memory configuration overlay.
    /// </summary>
    /// <param name="configuration">The base application configuration.</param>
    /// <param name="cancellationToken">The token used to cancel Secret Manager reads.</param>
    /// <returns>The original configuration when Secret Manager is disabled; otherwise a configuration with loaded secret values overlaid.</returns>
    public static async Task<IConfiguration> ApplySecretsAsync(
        this IConfiguration configuration,
        CancellationToken cancellationToken = default) =>
        await ApplySecretsAsync(
            configuration,
            SecretManagerResourceHelper.BuildClient,
            cancellationToken);

    /// <summary>
    /// Applies configured secret values by using the provided Secret Manager client factory.
    /// </summary>
    /// <param name="configuration">The base application configuration.</param>
    /// <param name="buildClient">The factory that creates a Secret Manager client for the configured location.</param>
    /// <param name="cancellationToken">The token used to cancel Secret Manager reads.</param>
    /// <returns>The original configuration when Secret Manager is disabled; otherwise a configuration with loaded secret values overlaid.</returns>
    private static async Task<IConfiguration> ApplySecretsAsync(
        IConfiguration configuration,
        Func<string, SecretManagerServiceClient> buildClient,
        CancellationToken cancellationToken)
    {
        var gcpOptions = configuration.GetSection(GCP_SETTINGS).Get<GcpOptions>() ?? new GcpOptions();
        if (!gcpOptions.SecretManagerSettings.IsUseSecret)
        {
            return configuration;
        }

        // Only configured secret-id values are resolved; empty values stay unset.
        var secretReferences = GetSecretIds(configuration).ToList();
        if (secretReferences.Count == 0)
        {
            return configuration;
        }

        // Startup resolution happens once before DI options bind, so no sync-over-async is needed later.
        var client = buildClient(gcpOptions.SecretManagerSettings.Location);
        var resourcePrefix = SecretManagerResourceHelper.BuildResourcePrefix(
            gcpOptions.ProjectNumber,
            gcpOptions.SecretManagerSettings.Location);
        var resolvedSecrets = await LoadValuesAsync(
            client,
            resourcePrefix,
            secretReferences,
            cancellationToken);

        return new ConfigurationBuilder()
            .AddConfiguration(configuration)
            .AddInMemoryCollection(resolvedSecrets)
            .Build();
    }

    /// <summary>
    /// Gets configured secret references that should be resolved before options binding.
    /// </summary>
    /// <param name="configuration">The base application configuration.</param>
    /// <returns>The configured paths and secret ids.</returns>
    private static IEnumerable<KeyValuePair<string, string>> GetSecretIds(IConfiguration configuration)
    {
        foreach (var configPath in GetSecretPaths())
        {
            var secretId = configuration[configPath];
            if (!string.IsNullOrWhiteSpace(secretId))
            {
                yield return new KeyValuePair<string, string>(configPath, secretId);
            }
        }
    }

    /// <summary>
    /// Gets the configuration paths that represent secret-backed option values.
    /// </summary>
    /// <returns>The configuration paths to inspect.</returns>
    private static IEnumerable<string> GetSecretPaths()
    {
        yield return $"{AUTH_SETTINGS}:{nameof(AuthenticationTokenValidationOptions.SecretKey)}";
        yield return $"{EMAIL_SETTINGS}:{nameof(EmailOptions.ApiKey)}";
        yield return $"{EMAIL_SETTINGS}:{nameof(EmailOptions.Password)}";
        yield return $"{EMAIL_SETTINGS}:{nameof(EmailOptions.PasswordFrom)}";
        yield return $"{R2_STORAGE_SETTINGS}:{nameof(R2StorageOptions.AccessKeyId)}";
        yield return $"{R2_STORAGE_SETTINGS}:{nameof(R2StorageOptions.SecretAccessKey)}";
        yield return $"{UPLOAD_GCP_SETTINGS}:{nameof(UploadGcpOptions.ApiKey)}";
        yield return $"{RedisConstants.CACHE_SETTING}:{nameof(CacheOptions.RedisSettings)}:{nameof(RedisOptions.Password)}";
    }

    /// <summary>
    /// Loads each configured secret value from Secret Manager.
    /// </summary>
    /// <param name="client">The Secret Manager client.</param>
    /// <param name="resourcePrefix">The Secret Manager resource prefix.</param>
    /// <param name="secretReferences">The config paths and secret ids to resolve.</param>
    /// <param name="cancellationToken">The token used to cancel Secret Manager reads.</param>
    /// <returns>The loaded configuration overlay values.</returns>
    private static async Task<Dictionary<string, string>> LoadValuesAsync(
        SecretManagerServiceClient client,
        string resourcePrefix,
        IReadOnlyCollection<KeyValuePair<string, string>> secretReferences,
        CancellationToken cancellationToken)
    {
        // Load independent secrets in parallel to keep startup latency bounded.
        var resolvedValues = await Task.WhenAll(secretReferences.Select(secretReference =>
            LoadValueAsync(client, resourcePrefix, secretReference, cancellationToken)));

        return resolvedValues.ToDictionary(
            resolvedValue => resolvedValue.Key,
            resolvedValue => resolvedValue.Value,
            StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Loads one Secret Manager value into one configuration key/value pair.
    /// </summary>
    /// <param name="client">The Secret Manager client.</param>
    /// <param name="resourcePrefix">The Secret Manager resource prefix.</param>
    /// <param name="secretReference">The config path and secret id to resolve.</param>
    /// <param name="cancellationToken">The token used to cancel the Secret Manager read.</param>
    /// <returns>The loaded configuration key/value pair.</returns>
    private static async Task<KeyValuePair<string, string>> LoadValueAsync(
        SecretManagerServiceClient client,
        string resourcePrefix,
        KeyValuePair<string, string> secretReference,
        CancellationToken cancellationToken)
    {
        try
        {
            // Secret version is intentionally fixed to latest so appsettings only carries the secret id.
            var secretVersionName = $"{resourcePrefix}/{secretReference.Value}/{VERSIONS_SEGMENT}/{LATEST}";
            var response = await client.AccessSecretVersionAsync(secretVersionName, cancellationToken);

            return new KeyValuePair<string, string>(
                secretReference.Key,
                response.Payload.Data.ToStringUtf8());
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Keep the original GCP exception while adding the exact config path and secret id that failed.
            throw new InvalidOperationException(
                string.Format(
                    SECRET_MANAGER_VALUE_LOAD_FAILED,
                    secretReference.Key,
                    secretReference.Value,
                    resourcePrefix),
                ex);
        }
    }
}
