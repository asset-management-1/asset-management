using Haven.Application.Dtos.Properties.Create;
using Haven.Application.Dtos.Properties.Detail;
using Haven.Application.Dtos.Properties.List;
using Haven.Application.Interfaces.Services;
using Haven.Application.Mappings.Properties;
using Haven.Application.Mappings.Rooms;
using Haven.Application.Mappings.Tenants;
using Haven.Application.Models.Locations;
using Haven.Application.Models.MasterData;
using Haven.Application.Models.Parties;
using Haven.Application.Models.Properties.Create;
using Haven.Application.Models.Properties.Detail;
using Haven.Application.Models.Properties.List;
using Haven.Application.Models.Properties.QueryParameters;
using Haven.Application.Models.Properties.Rows;
using Haven.Application.Queries.GetProperties;
using Mapster;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Queries.GetProperties;

public sealed class GetPropertiesQueryHandlerTests
{
    static GetPropertiesQueryHandlerTests()
    {
    }

    [Fact]
    public async Task Handle_Should_ResolvePartyAndPassListRequestToService_When_QueryIsValid()
    {
        var currentParty = new CurrentPartyContextModel
        {
            PartyId = 123,
            PartyPublicId = Guid.NewGuid()
        };
        var response = new PaginationResponse<IReadOnlyList<PropertyListItemResponseDto>>(
            [],
            1,
            20,
            0);
        var propertyService = new Mock<IPropertyService>();
        var partyService = new Mock<IPartyService>();
        var sut = new GetPropertiesQueryHandler(
            propertyService.Object,
            partyService.Object,
            Mock.Of<ILogger<GetPropertiesQueryHandler>>());
        var query = new GetPropertiesQuery
        {
            Search = " toa ",
            PageNumber = 1,
            PageSize = 20
        };

        partyService
            .Setup(x => x.GetCurrentPartyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentParty);
        propertyService
            .Setup(x => x.GetPropertiesAsync(
                It.Is<PropertyListRequestModel>(request =>
                    request.CurrentParty == currentParty
                    && request.Search == query.Search),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await sut.Handle(query, CancellationToken.None);

        result.Data.Should().BeSameAs(response);
        partyService.Verify(x => x.GetCurrentPartyAsync(It.IsAny<CancellationToken>()), Times.Once);
        propertyService.Verify(
            x => x.GetPropertiesAsync(It.IsAny<PropertyListRequestModel>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

