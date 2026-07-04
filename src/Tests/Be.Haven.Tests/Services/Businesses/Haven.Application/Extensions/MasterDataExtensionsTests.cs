using Haven.Application.Extensions;
using Haven.Application.Models.Locations;
using Haven.Application.Models.MasterData;
using Haven.Application.Models.Parties;
using Haven.Application.Models.Properties.Create;
using Haven.Application.Models.Properties.Detail;
using Haven.Application.Models.Properties.List;
using Haven.Application.Models.Properties.QueryParameters;
using Haven.Application.Models.Properties.Rows;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Extensions;

public sealed class MasterDataExtensionsTests
{
    [Fact]
    public void AddKey_Should_AddRequiredMasterDataKey()
    {
        var keys = new HashSet<MasterDataKeyModel>();

        keys.AddKey("PROPERTY_TYPE", "BUILDING");

        keys.Should().ContainSingle();
        keys.Should().Contain(new MasterDataKeyModel("PROPERTY_TYPE", "BUILDING"));
    }

    [Fact]
    public void AddOptionalKey_Should_SkipBlankCodeAndTrimSuppliedCode()
    {
        var keys = new HashSet<MasterDataKeyModel>();

        keys.AddOptionalKey("VEHICLE_TYPE", "  MOTORBIKE  ");
        keys.AddOptionalKey("VEHICLE_TYPE", "   ");

        keys.Should().ContainSingle();
        keys.Should().Contain(new MasterDataKeyModel("VEHICLE_TYPE", "MOTORBIKE"));
    }

    [Fact]
    public void AddKey_Should_DedupeDuplicateKeys_When_TargetIsHashSet()
    {
        var keys = new HashSet<MasterDataKeyModel>();

        keys.AddKey("UNIT_STATUS", "AVAILABLE");
        keys.AddKey("UNIT_STATUS", "AVAILABLE");

        keys.Should().ContainSingle();
    }

    [Fact]
    public void RequireValue_Should_ReturnResolvedValue_When_KeyExists()
    {
        var key = new MasterDataKeyModel("PROPERTY_TYPE", "BUILDING");
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

        var result = values.RequireValue(key);

        result.Should().BeSameAs(expected);
    }

    [Fact]
    public void RequireValue_Should_ThrowBadRequest_When_KeyIsMissing()
    {
        var values = new Dictionary<MasterDataKeyModel, MasterDataValueModel>();
        var key = new MasterDataKeyModel("PROPERTY_TYPE", "BUILDING");

        var act = () => values.RequireValue(key);

        act.Should()
            .Throw<ApiException>()
            .Where(exception => exception.StatusCode == StatusCodes.Status400BadRequest);
    }
}

