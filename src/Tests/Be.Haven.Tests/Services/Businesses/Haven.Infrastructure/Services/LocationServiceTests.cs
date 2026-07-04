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

public sealed class LocationServiceTests
{
    [Fact]
    public async Task GetLocationsAsync_Should_ReturnHierarchy_When_CodesAreValid()
    {
        var repository = new Mock<ILocationRepository>();
        repository
            .Setup(x => x.GetProvinceByCodeAsync("HCM", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LocationLookupModel { Id = 10, Code = "HCM", Name = "TP.HCM" });
        repository
            .Setup(x => x.GetDistrictByCodeAsync("D1", 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LocationLookupModel { Id = 20, Code = "D1", Name = "Quận 1" });
        repository
            .Setup(x => x.GetWardByCodeAsync("W1", 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LocationLookupModel { Id = 30, Code = "W1", Name = "Phường 1" });
        var service = new LocationService(repository.Object, Mock.Of<ILogger<LocationService>>());

        var result = await service.GetLocationsAsync(new LocationKeyModel
        {
            ProvinceCode = "HCM",
            DistrictCode = "D1",
            WardCode = "W1"
        });

        result.Province.Id.Should().Be(10);
        result.District.Id.Should().Be(20);
        result.Ward.Id.Should().Be(30);
    }

    [Fact]
    public async Task GetLocationsAsync_Should_ReturnNullLocations_When_CodesAreBlank()
    {
        var repository = new Mock<ILocationRepository>();
        var service = new LocationService(repository.Object, Mock.Of<ILogger<LocationService>>());

        var result = await service.GetLocationsAsync(new LocationKeyModel());

        result.Province.Should().BeNull();
        result.District.Should().BeNull();
        result.Ward.Should().BeNull();
        repository.Verify(
            x => x.GetProvinceByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetLocationsAsync_Should_ThrowBadRequest_When_SuppliedCodeIsInvalid()
    {
        var repository = new Mock<ILocationRepository>();
        repository
            .Setup(x => x.GetProvinceByCodeAsync("INVALID", It.IsAny<CancellationToken>()))
            .ReturnsAsync((LocationLookupModel)null);
        var service = new LocationService(repository.Object, Mock.Of<ILogger<LocationService>>());

        var act = () => service.GetLocationsAsync(new LocationKeyModel { ProvinceCode = "INVALID" });

        await act.Should()
            .ThrowAsync<ApiException>()
            .Where(exception => exception.StatusCode == StatusCodes.Status400BadRequest);
    }
}

