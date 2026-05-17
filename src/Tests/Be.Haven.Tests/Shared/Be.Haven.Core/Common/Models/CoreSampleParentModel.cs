namespace Be.Haven.Tests.Shared.Be.Haven.Core.Common.Models;

internal sealed class CoreSampleParentModel
{
    [Required]
    public string Name { get; set; }

    public CoreSampleChildModel Child { get; set; }

    public List<CoreSampleChildModel> Children { get; set; } = [];
}
