namespace Be.Haven.Core.Helpers;

/// <summary>
/// Builds Secret Manager endpoint and resource values for global and regional secret stores.
/// </summary>
public static class SecretManagerResourceHelper
{
    /// <summary>
    /// Builds a Secret Manager client for the configured location.
    /// </summary>
    /// <param name="location">The configured Secret Manager location.</param>
    /// <returns>A Secret Manager client that targets either the global endpoint or the regional endpoint.</returns>
    public static SecretManagerServiceClient BuildClient(string location)
    {
        if (IsGlobalLocation(location))
        {
            return new SecretManagerServiceClientBuilder().Build();
        }

        // Regional secrets require the regional replicated endpoint.
        return new SecretManagerServiceClientBuilder
        {
            Endpoint = string.Format(SECRET_MANAGER_ENDPOINT_FORMAT, location),
        }.Build();
    }

    /// <summary>
    /// Builds the Secret Manager resource prefix for the configured location.
    /// </summary>
    /// <param name="projectIdOrNumber">The GCP project id or project number.</param>
    /// <param name="location">The configured Secret Manager location.</param>
    /// <returns>The resource prefix used to access secrets.</returns>
    public static string BuildResourcePrefix(string projectIdOrNumber, string location)
    {
        if (IsGlobalLocation(location))
        {
            return string.Format(SECRET_RESOURCE_GLOBAL_PREFIX_FORMAT, projectIdOrNumber);
        }

        return string.Format(
            SECRET_RESOURCE_PREFIX_FORMAT,
            projectIdOrNumber,
            location);
    }

    /// <summary>
    /// Determines whether the configured location should use global Secret Manager.
    /// </summary>
    /// <param name="location">The configured Secret Manager location.</param>
    /// <returns><c>true</c> when global Secret Manager should be used; otherwise <c>false</c>.</returns>
    private static bool IsGlobalLocation(string location)
    {
        return string.IsNullOrWhiteSpace(location)
               || string.Equals(
                   location,
                   SECRET_MANAGER_GLOBAL_LOCATION,
                   StringComparison.OrdinalIgnoreCase);
    }
}
