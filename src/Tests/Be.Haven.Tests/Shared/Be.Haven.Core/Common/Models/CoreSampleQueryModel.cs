namespace Be.Haven.Tests.Shared.Be.Haven.Core.Common.Models;

internal sealed class CoreSampleQueryModel
{
    public int Id { get; set; }

    public string Name { get; set; }

    public int Count { get; set; }

    public CoreSampleChildModel Child { get; set; }
}
