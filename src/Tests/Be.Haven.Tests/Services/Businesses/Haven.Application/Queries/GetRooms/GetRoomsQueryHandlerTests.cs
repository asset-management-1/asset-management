using Haven.Application.Dtos.Rooms.List;
using Haven.Application.Interfaces.Services;
using Haven.Application.Models.Parties;
using Haven.Application.Models.Rooms.List;
using Haven.Application.Queries.GetRooms;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Queries.GetRooms;

public sealed class GetRoomsQueryHandlerTests
{
    static GetRoomsQueryHandlerTests()
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
        var response = new PaginationResponse<IReadOnlyList<RoomListPropertyResponseDto>>(
            [],
            1,
            20,
            0);
        var roomService = new Mock<IRoomService>();
        var partyService = new Mock<IPartyService>();
        var sut = new GetRoomsQueryHandler(
            roomService.Object,
            partyService.Object,
            Mock.Of<ILogger<GetRoomsQueryHandler>>());
        var query = new GetRoomsQuery
        {
            PropertyId = Guid.NewGuid(),
            PropertySearch = "Atlas",
            Search = "101",
            PageNumber = 1,
            PageSize = 20
        };

        partyService
            .Setup(x => x.GetCurrentPartyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentParty);
        roomService
            .Setup(x => x.GetRoomsAsync(
                It.Is<RoomListRequestModel>(request =>
                    request.CurrentParty == currentParty
                    && request.PropertyId == query.PropertyId
                    && request.PropertySearch == query.PropertySearch
                    && request.Search == query.Search),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await sut.Handle(query, CancellationToken.None);

        result.Data.Should().BeSameAs(response);
        partyService.Verify(x => x.GetCurrentPartyAsync(It.IsAny<CancellationToken>()), Times.Once);
        roomService.Verify(
            x => x.GetRoomsAsync(It.IsAny<RoomListRequestModel>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
