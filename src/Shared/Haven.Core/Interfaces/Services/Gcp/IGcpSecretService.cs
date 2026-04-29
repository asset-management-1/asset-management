namespace Haven.Core.Interfaces.Services.Gcp;

public interface IGcpSecretService
{
    /// <summary>
    /// Retrieves a secret value as UTF-8 text.
    /// - Uses cache by default (key: "{secretId}/versions/{version}", TTL: 4 hours).
    /// - Falls back to GCP Secret Manager on cache miss.
    /// </summary>
    /// <param name="secretId">The unique identifier of the secret in the Secret Manager.</param>
    /// <param name="version">
    /// Optional version of the secret to be retrieved. If <c>null</c> or not provided, the default implementation
    /// typically uses <c>"latest"</c>.
    /// </param>
    /// <param name="isCached">
    /// Indicates whether to use a cached value if available. If <c>true</c>, the method may return a cached version of the secret.
    /// </param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The value of the secret as a string.</returns>
    /// <exception cref="System.ArgumentException">Thrown if <paramref name="secretId"/> is null, empty, or whitespace.</exception>
    /// <exception cref="Grpc.Core.RpcException">
    /// Thrown for network/authentication/permission issues (e.g., NotFound, PermissionDenied).
    /// </exception>
    Task<string> GetByIdAsync(
        string secretId,
        string version = null,
        bool isCached = true,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves the raw secret payload as bytes.
    /// </summary>
    /// <param name="secretId">Logical secret identifier (the short name created in Secret Manager).</param>
    /// <param name="version">
    /// Optional version to read. If <c>null</c> or empty, the implementation should use the configured default
    /// (typically <c>"latest"</c>).
    /// </param>
    /// <param name="ct">Cancellation token for the underlying network call.</param>
    /// <returns>The secret payload as a byte array.</returns>
    /// <exception cref="System.ArgumentException">Thrown when <paramref name="secretId"/> is null or whitespace.</exception>
    /// <exception cref="Grpc.Core.RpcException">
    /// Thrown for network/auth/permission issues (e.g., NotFound, PermissionDenied).
    /// </exception>
    Task<byte[]> GetBytesAsync(
        string secretId,
        string version = null,
        CancellationToken ct = default);

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
    Task<T> GetJsonAsync<T>(
        string secretId,
        string version = null,
        CancellationToken ct = default);

    /// <summary>
    /// Checks whether a secret exists (metadata-only check; no payload is read).
    /// </summary>
    /// <param name="secretId">Logical secret identifier.</param>
    /// <param name="ct">Cancellation token for the underlying network call.</param>
    /// <returns>
    /// <c>true</c> if the secret exists and is accessible; otherwise <c>false</c>
    /// (including not found or permission denied).
    /// </returns>
    Task<bool> ExistsAsync(
        string secretId,
        CancellationToken ct = default);

    /// <summary>
    /// Checks whether a specific secret <em>version</em> exists (metadata-only check).
    /// </summary>
    /// <param name="secretId">Logical secret identifier.</param>
    /// <param name="version">
    /// Optional version to check. If <c>null</c> or empty, the implementation should use the configured default
    /// (typically <c>"latest"</c>).
    /// </param>
    /// <param name="ct">Cancellation token for the underlying network call.</param>
    /// <returns>
    /// <c>true</c> if the version exists and is accessible; otherwise <c>false</c>
    /// (including not found or permission denied).
    /// </returns>
    Task<bool> VersionExistsAsync(
        string secretId,
        string version = null,
        CancellationToken ct = default);

    /// <summary>
    /// Streams the list of versions for a given secret (metadata only).
    /// </summary>
    /// <param name="secretId">Logical secret identifier.</param>
    /// <param name="pageSize">
    /// Optional page size hint for the server (defaults to 100). Set to 0 to let the server decide.
    /// </param>
    /// <param name="ct">Cancellation token to stop enumeration early.</param>
    /// <returns>
    /// An async sequence of <see cref="SecretVersion"/> items. Each item contains metadata such as
    /// name, state, and create time; not the payload.
    /// </returns>
    IAsyncEnumerable<SecretVersion> ListVersionsAsync(
        string secretId,
        int pageSize = 100,
        CancellationToken ct = default);
}