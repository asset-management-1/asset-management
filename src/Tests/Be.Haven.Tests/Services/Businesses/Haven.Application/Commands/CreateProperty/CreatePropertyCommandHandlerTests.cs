using Haven.Application.Commands.CreateProperty;
using Haven.Application.Dtos.Properties.Create;
using Haven.Application.Interfaces.Services;
using Haven.Application.Models.Parties;
using Haven.Application.Models.Properties.Create;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Commands.CreateProperty;

public sealed class CreatePropertyCommandHandlerTests
{
    static CreatePropertyCommandHandlerTests()
    {
    }

    [Fact]
    public async Task Handle_Should_ResolveLandlordAndPassCreationRequestToService_When_CommandIsValid()
    {
        var currentParty = new CurrentPartyContextModel
        {
            PartyId = 123,
            PartyPublicId = Guid.NewGuid(),
            PartyTypeCode = MASTER_CODE_PARTY_TYPE_LANDLORD,
            StatusCode = MASTER_CODE_ACTIVE
        };
        var response = new CreatedPropertyResponseDto
        {
            Id = Guid.NewGuid(),
            PropertyCode = "PROP-TEST",
            Name = "Tòa nhà A"
        };
        var propertyService = new Mock<IPropertyService>();
        var partyService = new Mock<IPartyService>();
        var sut = new CreatePropertyCommandHandler(
            propertyService.Object,
            partyService.Object,
            Mock.Of<ILogger<CreatePropertyCommandHandler>>());
        var command = BuildCommand();

        partyService
            .Setup(x => x.GetCurrentLandlordAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentParty);
        propertyService
            .Setup(x => x.CreatePropertyAsync(
                It.Is<PropertyCreationRequestModel>(request =>
                    request.CurrentParty == currentParty
                    && request.Name == command.Name
                    && request.StructureSetup.Floors.Count == command.StructureSetup.Floors.Count
                    && request.StructureSetup.Floors[0].Rooms.Count == command.StructureSetup.Floors[0].Rooms.Count),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await sut.Handle(command, CancellationToken.None);

        result.Data.Should().BeSameAs(response);
        partyService.Verify(x => x.GetCurrentLandlordAsync(It.IsAny<CancellationToken>()), Times.Once);
        propertyService.Verify(
            x => x.CreatePropertyAsync(It.IsAny<PropertyCreationRequestModel>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static CreatePropertyCommand BuildCommand()
    {
        return new CreatePropertyCommand
        {
            Name = "Tòa nhà A",
            PropertyTypeCode = "BUILDING",
            StructureSetup = new CreatePropertyStructureRequestDto
            {
                Floors =
                [
                    new CreatePropertyFloorRequestDto
                    {
                        FloorNumber = 1,
                        Rooms =
                        [
                            new CreatePropertyRoomRequestDto
                            {
                                Name = "Phòng 101",
                                TypeCode = MASTER_CODE_UNIT_TYPE_ROOM,
                                RentalModeCode = MASTER_CODE_RENTAL_MODE_WHOLE_UNIT,
                                BaseRentAmount = 5_500_000
                            }
                        ]
                    }
                ]
            }
        };
    }
}

