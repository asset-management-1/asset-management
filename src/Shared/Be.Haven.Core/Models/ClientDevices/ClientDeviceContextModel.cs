namespace Be.Haven.Core.Models.ClientDevices;

/// <summary>
/// Represents normalized device metadata captured from the current HTTP request.
/// </summary>
public sealed class ClientDeviceContextModel
{
    /// <summary>
    /// Gets or sets the normalized client-supplied client instance identifier.
    /// </summary>
    public string DeviceId { get; set; }

    /// <summary>
    /// Gets or sets the normalized client-supplied device display name.
    /// </summary>
    public string DeviceName { get; set; }

    /// <summary>
    /// Gets or sets the normalized client-supplied device type.
    /// </summary>
    public string DeviceType { get; set; }

    /// <summary>
    /// Gets or sets the normalized request user-agent value.
    /// </summary>
    public string UserAgent { get; set; }

    /// <summary>
    /// Gets or sets the normalized remote IP address observed by ASP.NET Core.
    /// </summary>
    public string IpAddress { get; set; }
}
