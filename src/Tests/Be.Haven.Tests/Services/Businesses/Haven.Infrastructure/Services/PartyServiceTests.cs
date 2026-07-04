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
using static Haven.Application.Constants.ApplicationConstants;

namespace Be.Haven.Tests.Services.Businesses.Haven.Infrastructure.Services;

public sealed class PartyServiceTests
{
    [Fact]
    public async Task GetCurrentPartyAsync_Should_ReturnCurrentPartyFromRepository_When_UserHasSelectedParty()
    {
        var userPublicId = Guid.NewGuid();
        var currentParty = CreateCurrentParty();
        var context = CreateSut(userPublicId, currentParty);

        var result = await context.Sut.GetCurrentPartyAsync();

        result.Should().BeSameAs(currentParty);
        context.PartyRepository.Verify(
            x => x.GetCurrentPartyByUserPublicIdAsync(userPublicId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCurrentPartyAsync_Should_ReadRepositoryEachTime_When_CalledTwiceForSameUser()
    {
        var userPublicId = Guid.NewGuid();
        var currentParty = CreateCurrentParty();
        var context = CreateSut(userPublicId, currentParty);

        var first = await context.Sut.GetCurrentPartyAsync();
        var second = await context.Sut.GetCurrentPartyAsync();

        second.Should().BeSameAs(first);
        context.PartyRepository.Verify(
            x => x.GetCurrentPartyByUserPublicIdAsync(userPublicId, It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task GetCurrentPartyAsync_Should_ThrowUnauthorized_When_UserIsMissing()
    {
        var context = CreateSut(userPublicId: null, repositoryParty: null);

        var action = () => context.Sut.GetCurrentPartyAsync();

        await action.Should()
            .ThrowAsync<HttpStatusCodeException>()
            .Where(exception => exception.StatusCode == StatusCodes.Status401Unauthorized);
        context.PartyRepository.Verify(
            x => x.GetCurrentPartyByUserPublicIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetCurrentPartyAsync_Should_ThrowForbidden_When_SelectedPartyIsMissing()
    {
        var userPublicId = Guid.NewGuid();
        var context = CreateSut(userPublicId, repositoryParty: null);

        var action = () => context.Sut.GetCurrentPartyAsync();

        await action.Should()
            .ThrowAsync<ApiException>()
            .Where(exception => exception.StatusCode == StatusCodes.Status403Forbidden);
        context.PartyRepository.Verify(
            x => x.GetCurrentPartyByUserPublicIdAsync(userPublicId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCurrentLandlordAsync_Should_ReturnCurrentParty_When_PartyIsActiveLandlord()
    {
        var userPublicId = Guid.NewGuid();
        var currentParty = CreateCurrentParty();
        var context = CreateSut(userPublicId, currentParty);

        var result = await context.Sut.GetCurrentLandlordAsync();

        result.Should().BeSameAs(currentParty);
        context.PartyRepository.Verify(
            x => x.GetCurrentPartyByUserPublicIdAsync(userPublicId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCurrentLandlordAsync_Should_ThrowForbidden_When_PartyIsNotActiveLandlord()
    {
        var currentParty = CreateCurrentParty(partyTypeCode: "TENANT");
        var context = CreateSut(Guid.NewGuid(), currentParty);

        var action = () => context.Sut.GetCurrentLandlordAsync();

        await action.Should()
            .ThrowAsync<ApiException>()
            .Where(exception => exception.StatusCode == StatusCodes.Status403Forbidden);
    }

    private static PartyServiceTestContext CreateSut(
        Guid? userPublicId,
        CurrentPartyContextModel repositoryParty)
    {
        var authService = new Mock<IAuthService>();
        authService.Setup(x => x.UserId()).Returns(userPublicId);
        var partyRepository = new Mock<IPartyRepository>();
        if (userPublicId.HasValue)
        {
            partyRepository
                .Setup(x => x.GetCurrentPartyByUserPublicIdAsync(userPublicId.Value, It.IsAny<CancellationToken>()))
                .ReturnsAsync(repositoryParty);
        }

        return new PartyServiceTestContext
        {
            PartyRepository = partyRepository,
            Sut = new PartyService(authService.Object, partyRepository.Object, Mock.Of<ILogger<PartyService>>())
        };
    }

    private static CurrentPartyContextModel CreateCurrentParty(
        long partyId = 10,
        string partyTypeCode = MASTER_CODE_PARTY_TYPE_LANDLORD,
        string statusCode = MASTER_CODE_ACTIVE)
    {
        return new CurrentPartyContextModel
        {
            PartyId = partyId,
            PartyPublicId = Guid.NewGuid(),
            DisplayName = "Nguyen Van A",
            PartyTypeCode = partyTypeCode,
            StatusCode = statusCode
        };
    }

    private sealed class PartyServiceTestContext
    {
        public Mock<IPartyRepository> PartyRepository { get; set; }

        public PartyService Sut { get; set; }
    }
}

