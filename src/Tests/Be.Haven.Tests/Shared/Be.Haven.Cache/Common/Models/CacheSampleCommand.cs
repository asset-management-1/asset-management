namespace Be.Haven.Tests.Shared.Be.Haven.Cache.Common.Models;

public sealed class CacheSampleCommand : Mediator.IMessage
{
    public string Scope { get; set; }
}
