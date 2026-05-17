namespace Be.Haven.Core.Services;

/// <summary>
/// Reads client device metadata from request headers and connection information.
/// </summary>
public sealed class ClientDeviceContextAccessor : IClientDeviceContextAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Creates the client-device accessor for the current HTTP request context.
    /// </summary>
    /// <param name="httpContextAccessor">Accessor for the ambient HTTP context.</param>
    public ClientDeviceContextAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Gets normalized device metadata from the current request.
    /// </summary>
    /// <returns>The normalized client device context.</returns>
    public ClientDeviceContextModel GetCurrent()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var headers = httpContext?.Request.Headers;
        var rawDeviceId = ReadHeader(headers, ClientDeviceHeaders.DEVICE_ID);
        var rawDeviceName = ReadHeader(headers, ClientDeviceHeaders.DEVICE_NAME);
        var rawDeviceType = ReadHeader(headers, ClientDeviceHeaders.DEVICE_TYPE);
        var rawUserAgent = ReadHeader(headers, ClientDeviceHeaders.USER_AGENT);

        // Device metadata is normalized for storage; token endpoint presence is enforced by the API filter.
        return new ClientDeviceContextModel
        {
            DeviceId = Normalize(
                rawDeviceId,
                ClientDeviceFallbacks.DEVICE_ID,
                ClientDeviceMetadataLimits.DEVICE_ID_MAX_LENGTH),
            DeviceName = Normalize(
                rawDeviceName,
                ClientDeviceFallbacks.DEVICE_NAME,
                ClientDeviceMetadataLimits.DEVICE_NAME_MAX_LENGTH),
            DeviceType = Normalize(
                rawDeviceType,
                ClientDeviceFallbacks.DEVICE_TYPE,
                ClientDeviceMetadataLimits.DEVICE_TYPE_MAX_LENGTH),
            UserAgent = Normalize(
                rawUserAgent,
                ClientDeviceFallbacks.USER_AGENT,
                ClientDeviceMetadataLimits.USER_AGENT_MAX_LENGTH),
            IpAddress = Normalize(
                httpContext?.Connection.RemoteIpAddress?.ToString(),
                ClientDeviceFallbacks.IP_ADDRESS,
                ClientDeviceMetadataLimits.IP_ADDRESS_MAX_LENGTH)
        };
    }

    /// <summary>
    /// Reads a request header value without trusting it as an authorization source.
    /// </summary>
    /// <param name="headers">The current request headers.</param>
    /// <param name="name">The header name to read.</param>
    /// <returns>The raw header value, or <c>null</c> when missing.</returns>
    private static string ReadHeader(IHeaderDictionary headers, string name)
    {
        // Missing HTTP context is allowed for non-request execution paths.
        return headers is not null && headers.TryGetValue(name, out var value)
            ? value.ToString()
            : null;
    }

    /// <summary>
    /// Normalizes optional metadata and constrains it to the database-backed maximum length.
    /// </summary>
    /// <param name="value">The raw metadata value.</param>
    /// <param name="fallback">The fallback value used when the raw value is blank.</param>
    /// <param name="maxLength">The maximum returned string length.</param>
    /// <returns>The normalized metadata value.</returns>
    private static string Normalize(string value, string fallback, int maxLength)
    {
        // Trimming and truncation keeps stored metadata bounded without rejecting login.
        var normalized = string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }
}
