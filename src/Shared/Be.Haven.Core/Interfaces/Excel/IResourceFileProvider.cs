namespace Be.Haven.Core.Interfaces.Excel;

public interface IResourceFileProvider
{
    /// <summary>
    /// Retrieves the resource file as a byte array asynchronously using the specified key.
    /// </summary>
    /// <param name="key">The unique key identifying the resource file to retrieve.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the byte array of the resource file.</returns>
    Task<byte[]> GetAsync(string key, CancellationToken ct = default);
}