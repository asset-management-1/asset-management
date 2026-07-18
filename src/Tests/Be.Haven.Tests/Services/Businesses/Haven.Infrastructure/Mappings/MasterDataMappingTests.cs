using Haven.Application.Models.MasterData;
using Haven.Infrastructure.Mappings.MasterData;
using Haven.Infrastructure.Models.MasterData;

namespace Be.Haven.Tests.Services.Businesses.Haven.Infrastructure.Mappings;

public sealed class MasterDataMappingTests
{
    [Fact]
    public void MasterDataRow_Should_MapToMasterDataValue()
    {
        var config = new TypeAdapterConfig();
        new MasterDataMapping().Register(config);
        var row = new MasterDataRowModel
        {
            KeyType = "PropertyType",
            KeyCode = "BUILDING",
            Id = 100,
            Type = "PropertyType",
            Code = "BUILDING",
            Name = "Tòa nhà"
        };

        var value = row.Adapt<MasterDataValueModel>(config);

        value.Id.Should().Be(100);
        value.Type.Should().Be("PropertyType");
        value.Code.Should().Be("BUILDING");
        value.Name.Should().Be("Tòa nhà");
    }
}

