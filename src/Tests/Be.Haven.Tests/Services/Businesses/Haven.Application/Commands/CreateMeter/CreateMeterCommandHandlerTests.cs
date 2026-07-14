using Haven.Application.Commands.CreateMeter;
using Haven.Application.Dtos.Meters.Detail;
using Haven.Application.Interfaces.Services;
using Haven.Application.Models.Meters.Mutation;
using Haven.Application.Models.Parties;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Commands.CreateMeter;

public sealed class CreateMeterCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_CallSharedMutationWorkflowWithCreateMode()
    {
        var currentParty = new CurrentPartyContextModel
        {
            PartyId = 10,
            PartyPublicId = Guid.NewGuid()
        };
        var response = new MeterPeriodDetailResponseDto();
        var meterService = new Mock<IRoomMeterService>();
        var partyService = new Mock<IPartyService>();
        var command = new CreateMeterCommand
        {
            RoomId = Guid.NewGuid(),
            BillingDate = new DateTime(2026, 7, 5)
        };
        var sut = new CreateMeterCommandHandler(
            meterService.Object,
            partyService.Object,
            Mock.Of<ILogger<CreateMeterCommandHandler>>());

        partyService
            .Setup(service => service.GetCurrentLandlordAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentParty);
        meterService
            .Setup(service => service.SaveConfirmedPeriodAsync(
                It.Is<MeterMutationRequestModel>(request =>
                    request.RoomId == command.RoomId
                    && request.BillingDate == command.BillingDate
                    && request.CurrentParty == currentParty),
                MeterMutationModeEnum.Create,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await sut.Handle(command, CancellationToken.None);

        result.Data.Should().BeSameAs(response);
        meterService.Verify(service => service.SaveConfirmedPeriodAsync(
            It.IsAny<MeterMutationRequestModel>(),
            MeterMutationModeEnum.Create,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
