using Haven.Application.Extensions;
using Haven.Application.Models.MasterData;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Extensions;

public sealed class MasterDataExtensionsTests
{
    [Fact]
    public void GetValue_Should_ReturnResolvedValue_When_KeyExists()
    {
        var key = new MasterDataKeyModel(MasterDataTypeEnum.PropertyType, "BUILDING");
        var expected = new MasterDataValueModel
        {
            Id = 1,
            Type = key.Type,
            Code = key.Code,
            Name = "Building"
        };
        var values = new Dictionary<MasterDataKeyModel, MasterDataValueModel>
        {
            [key] = expected
        };

        var result = values.GetValue(MasterDataTypeEnum.PropertyType, key.Code);

        result.Should().BeSameAs(expected);
    }

    [Fact]
    public void GetValue_Should_ThrowBadRequest_When_KeyIsMissing()
    {
        var values = new Dictionary<MasterDataKeyModel, MasterDataValueModel>();
        var key = new MasterDataKeyModel(MasterDataTypeEnum.PropertyType, "BUILDING");

        var act = () => values.GetValue(MasterDataTypeEnum.PropertyType, key.Code);

        act.Should()
            .Throw<ApiException>()
            .Where(exception => exception.StatusCode == StatusCodes.Status400BadRequest);
    }
}

