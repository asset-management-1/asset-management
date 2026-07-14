using Haven.Application.Commands.UpdateProperty;
using Haven.Application.Dtos.Properties.Detail;
using Haven.Application.Dtos.Properties.Update;
using Haven.Application.Interfaces.Services;
using Haven.Application.Mappings.Properties;
using Haven.Application.Mappings.Rooms;
using Haven.Application.Mappings.Tenants;
using Haven.Application.Models.Parties;
using Haven.Application.Models.Properties.Update;
using Mapster;
using static Haven.Application.Constants.ApplicationConstants;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Commands.UpdateProperty;

public sealed class UpdatePropertyCommandHandlerTests
{
    static UpdatePropertyCommandHandlerTests()
    {
    }

    [Fact]
    public async Task Handle_Should_ResolveLandlordAndPassUpdateRequestToService_When_CommandIsValid()
    {
        var currentParty = new CurrentPartyContextModel
        {
            PartyId = 123,
            PartyPublicId = Guid.NewGuid(),
            PartyTypeCode = MASTER_CODE_PARTY_TYPE_LANDLORD,
            StatusCode = MASTER_CODE_ACTIVE
        };
        var response = new PropertyDetailResponseDto
        {
            BasicInfo = new PropertyDetailBasicInfoResponseDto
            {
                Id = Guid.NewGuid(),
                Name = "Atlas Plaza"
            }
        };
        var propertyService = new Mock<IPropertyService>();
        var partyService = new Mock<IPartyService>();
        var sut = new UpdatePropertyCommandHandler(
            propertyService.Object,
            partyService.Object,
            Mock.Of<ILogger<UpdatePropertyCommandHandler>>());
        var command = BuildCommand();

        partyService
            .Setup(x => x.GetCurrentLandlordAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentParty);
        propertyService
            .Setup(x => x.UpdatePropertyAsync(
                It.Is<PropertyUpdateRequestModel>(request =>
                    request.CurrentParty == currentParty
                    && request.PropertyPublicId == command.Id
                    && request.Name == command.Name
                    && request.Structure.Floors[0].Rooms[0].Name == "Phòng 101"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await sut.Handle(command, CancellationToken.None);

        result.Data.Should().BeSameAs(response);
        partyService.Verify(x => x.GetCurrentLandlordAsync(It.IsAny<CancellationToken>()), Times.Once);
        propertyService.Verify(
            x => x.UpdatePropertyAsync(It.IsAny<PropertyUpdateRequestModel>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static UpdatePropertyCommand BuildCommand()
    {
        return new UpdatePropertyCommand
        {
            Id = Guid.NewGuid(),
            Name = "Atlas Plaza",
            PropertyTypeCode = "MINI_APARTMENT",
            Structure = new UpdatePropertyStructureRequestDto
            {
                Floors =
                [
                    new UpdatePropertyFloorRequestDto
                    {
                        FloorNumber = 1,
                        Rooms =
                        [
                            new UpdatePropertyRoomRequestDto
                            {
                                Name = "Phòng 101",
                                TypeCode = MASTER_CODE_UNIT_TYPE_ROOM,
                                RentalModeCode = MASTER_CODE_RENTAL_MODE_WHOLE_UNIT
                            }
                        ]
                    }
                ]
            }
        };
    }
}
