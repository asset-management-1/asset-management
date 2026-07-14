using Haven.Application.Commands.RestorePropertyDelete;
using Haven.Application.Interfaces.Services;
using Haven.Application.Models.Parties;
using Haven.Application.Models.Properties.Delete;
using static Haven.Application.Constants.ApplicationConstants;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Commands.RestorePropertyDelete;

public sealed class RestorePropertyDeleteCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_ResolveLandlordAndPassRestoreRequestToService_When_CommandIsValid()
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
            Message = global::Haven.Application.Constants.ApplicationMessageConstants.PropertyMessages.PROPERTY_RESTORE_DELETE_SUCCESS_MESSAGE
        };
        var propertyService = new Mock<IPropertyService>();
        var partyService = new Mock<IPartyService>();
        var sut = new RestorePropertyDeleteCommandHandler(
            propertyService.Object,
            partyService.Object,
            Mock.Of<ILogger<RestorePropertyDeleteCommandHandler>>());
        var command = new RestorePropertyDeleteCommand(Guid.NewGuid());

        partyService
            .Setup(x => x.GetCurrentLandlordAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentParty);
        propertyService
            .Setup(x => x.RestorePropertyDeleteAsync(
                It.Is<PropertyDeleteRequestModel>(request =>
                    request.CurrentParty == currentParty
                    && request.PropertyPublicId == command.PropertyPublicId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await sut.Handle(command, CancellationToken.None);

        result.Data.Should().BeSameAs(response);
        partyService.Verify(x => x.GetCurrentLandlordAsync(It.IsAny<CancellationToken>()), Times.Once);
        propertyService.Verify(
            x => x.RestorePropertyDeleteAsync(It.IsAny<PropertyDeleteRequestModel>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
