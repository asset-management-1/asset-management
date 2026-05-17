namespace Be.Haven.Tests.Shared.Be.Haven.Cache.Common.Fakes;

internal sealed class CacheSampleInvalidationPolicy : ICacheInvalidationPolicy<CacheSampleCommand>
{
    private readonly IReadOnlyCollection<CacheInvalidationTargetModel> _targets;

    public CacheSampleInvalidationPolicy(params CacheInvalidationTargetModel[] targets)
    {
        _targets = targets;
    }

    public IReadOnlyCollection<CacheInvalidationTargetModel> GetTargets(CacheSampleCommand request) =>
        _targets;
}
