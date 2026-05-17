namespace Be.Haven.Tests.Shared.Be.Haven.Cache.Common.Models;

public sealed class CacheSampleQuery : ICacheableMediatorQueryService
{
    public bool BypassCache { get; set; }

    public string CacheKey { get; set; } = "cache:sample";

    public string CacheScope { get; set; }

    public TimeSpan? AbsoluteExpiration { get; set; }

    public string Search { get; set; }

    public int Page { get; set; }
}
