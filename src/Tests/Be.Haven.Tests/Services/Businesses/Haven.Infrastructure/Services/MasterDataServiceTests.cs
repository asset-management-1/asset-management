using Haven.Application.Interfaces.Repositories;
using Haven.Application.Models.Locations;
using Haven.Application.Models.MasterData;
using Haven.Application.Models.Parties;
using Haven.Application.Models.Properties.Create;
using Haven.Application.Models.Properties.Detail;
using Haven.Application.Models.Properties.List;
using Haven.Application.Models.Properties.QueryParameters;
using Haven.Application.Models.Properties.Rows;
using Haven.Infrastructure.Services;

namespace Be.Haven.Tests.Services.Businesses.Haven.Infrastructure.Services;

public sealed class MasterDataServiceTests
{
    [Fact]
    public async Task GetValuesAsync_Should_ReturnRepositoryValues_When_AllKeysExist()
    {
        var keys = new[]
        {
            new MasterDataKeyModel("PROPERTY_TYPE", "BUILDING"),
            new MasterDataKeyModel("UNIT_STATUS", "AVAILABLE")
        };
        var resolved = new Dictionary<MasterDataKeyModel, MasterDataValueModel>
        {
            [keys[0]] = new()
            {
                Id = 1,
                Type = keys[0].Type,
                Code = keys[0].Code,
                Name = "Building"
            },
            [keys[1]] = new()
            {
                Id = 2,
                Type = keys[1].Type,
                Code = keys[1].Code,
                Name = "Available"
            }
        };
        var repository = new Mock<IMasterDataRepository>();
        repository
            .Setup(x => x.GetByTypeAndCodesAsync(keys, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resolved);
        var service = new MasterDataService(repository.Object, Mock.Of<ILogger<MasterDataService>>());

        var result = await service.GetValuesAsync(keys);

        result.Should().BeSameAs(resolved);
        repository.Verify(
            x => x.GetByTypeAndCodesAsync(keys, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetValuesAsync_Should_ThrowBadRequest_When_KeyIsMissing()
    {
        var keys = new[] { new MasterDataKeyModel("PROPERTY_TYPE", "BUILDING") };
        var repository = new Mock<IMasterDataRepository>();
        repository
            .Setup(x => x.GetByTypeAndCodesAsync(keys, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<MasterDataKeyModel, MasterDataValueModel>());
        var service = new MasterDataService(repository.Object, Mock.Of<ILogger<MasterDataService>>());

        var act = () => service.GetValuesAsync(keys);

        await act.Should()
            .ThrowAsync<ApiException>()
            .Where(exception => exception.StatusCode == StatusCodes.Status400BadRequest);
    }
}

