namespace Be.Haven.Core.Interfaces.Services;

/// <summary>
/// Provides normalized client device metadata for the current request.
/// </summary>
public interface IClientDeviceContextAccessor
{
    /// <summary>
    /// Gets normalized device metadata from the current request.
    /// </summary>
    /// <returns>The normalized client device context.</returns>
    ClientDeviceContextModel GetCurrent();
}
