namespace Be.Haven.Tests.Shared.Be.Haven.Core.Common.Models;

internal sealed class CoreSerializationAttributeModel
{
    [JsonProperty("external_name")]
    public string InternalName { get; set; }
}

