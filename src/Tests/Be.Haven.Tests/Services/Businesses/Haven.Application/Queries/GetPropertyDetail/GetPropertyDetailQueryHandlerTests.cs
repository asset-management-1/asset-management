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
using Haven.Application.Queries.GetPropertyDetail;
using Mapster;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Queries.GetPropertyDetail;

public sealed class GetPropertyDetailQueryHandlerTests
{
    static GetPropertyDetailQueryHandlerTests()
    {
    }

    [Fact]
    public async Task Handle_Should_ResolvePartyAndPassDetailRequestToService_When_QueryIsValid()
    {
        var propertyPublicId = Guid.NewGuid();
        var currentParty = new CurrentPartyContextModel
        {
            PartyId = 123,
            PartyPublicId = Guid.NewGuid()
        };
        var response = new PropertyDetailResponseDto
        {
            BasicInfo = new PropertyDetailBasicInfoResponseDto
            {
                Id = propertyPublicId,
                Name = "Tòa nhà A"
            }
        };
        var propertyService = new Mock<IPropertyService>();
        var partyService = new Mock<IPartyService>();
        var sut = new GetPropertyDetailQueryHandler(
            propertyService.Object,
            partyService.Object,
            Mock.Of<ILogger<GetPropertyDetailQueryHandler>>());
        var query = new GetPropertyDetailQuery(propertyPublicId);

        partyService
            .Setup(x => x.GetCurrentPartyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentParty);
        propertyService
            .Setup(x => x.GetPropertyDetailAsync(
                It.Is<PropertyDetailRequestModel>(request =>
                    request.CurrentParty == currentParty
                    && request.PropertyPublicId == propertyPublicId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await sut.Handle(query, CancellationToken.None);

        result.Data.Should().BeSameAs(response);
        partyService.Verify(x => x.GetCurrentPartyAsync(It.IsAny<CancellationToken>()), Times.Once);
        propertyService.Verify(
            x => x.GetPropertyDetailAsync(It.IsAny<PropertyDetailRequestModel>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

