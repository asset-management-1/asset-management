using Haven.Application.Commands.DeleteProperty;
using Haven.Application.Interfaces.Services;
using Haven.Application.Models.Parties;
using Haven.Application.Models.Properties.Delete;
using static Haven.Application.Constants.ApplicationConstants;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Commands.DeleteProperty;

public sealed class DeletePropertyCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_ResolveLandlordAndPassDeleteRequestToService_When_CommandIsValid()
    {
        var currentParty = new CurrentPartyContextModel
        {
            PartyId = 123,
            PartyPublicId = Guid.NewGuid(),
            PartyTypeCode = MASTER_CODE_PARTY_TYPE_LANDLORD,
            StatusCode = MASTER_CODE_ACTIVE
        };
        var response = new OperationStatusResponseDto
        {
            IsSuccess = true,
            Message = global::Haven.Application.Constants.ApplicationMessageConstants.PropertyMessages.PROPERTY_DELETE_SUCCESS_MESSAGE
        };
        var propertyService = new Mock<IPropertyService>();
        var partyService = new Mock<IPartyService>();
        var sut = new DeletePropertyCommandHandler(
            propertyService.Object,
            partyService.Object,
            Mock.Of<ILogger<DeletePropertyCommandHandler>>());
        var command = new DeletePropertyCommand(Guid.NewGuid());

        partyService
            .Setup(x => x.GetCurrentLandlordAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentParty);
        propertyService
            .Setup(x => x.DeletePropertyAsync(
                It.Is<PropertyDeleteRequestModel>(request =>
                    request.CurrentParty == currentParty
                    && request.PropertyPublicId == command.PropertyPublicId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await sut.Handle(command, CancellationToken.None);

        result.Data.Should().BeSameAs(response);
        partyService.Verify(x => x.GetCurrentLandlordAsync(It.IsAny<CancellationToken>()), Times.Once);
        propertyService.Verify(
            x => x.DeletePropertyAsync(It.IsAny<PropertyDeleteRequestModel>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
