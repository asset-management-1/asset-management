namespace Be.Haven.Tests.Shared.Be.Haven.Core.Common.Fakes;

internal class FakeDistributedCache : IDistributedCache
{
    public Dictionary<string, byte[]> Values { get; } = [];

    public Dictionary<string, DistributedCacheEntryOptions> OptionsByKey { get; } = [];

    public List<string> RemovedKeys { get; } = [];

    public byte[] Get(string key)
    {
        return Values.TryGetValue(key, out var value) ? value : null;
    }

    public virtual Task<byte[]> GetAsync(
        string key,
        CancellationToken token = default)
    {
        return Task.FromResult(Get(key));
    }

    public void Set(
        string key,
        byte[] value,
        DistributedCacheEntryOptions options)
    {
        Values[key] = value;
        OptionsByKey[key] = options;
    }

    public virtual Task SetAsync(
        string key,
        byte[] value,
        DistributedCacheEntryOptions options,
        CancellationToken token = default)
    {
        Set(key, value, options);

        return Task.CompletedTask;
    }

    public void Refresh(string key)
    {
    }

    public Task RefreshAsync(
        string key,
        CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    public void Remove(string key)
    {
        Values.Remove(key);
        RemovedKeys.Add(key);
    }

    public Task RemoveAsync(
        string key,
        CancellationToken token = default)
    {
        Remove(key);

        return Task.CompletedTask;
    }
}
