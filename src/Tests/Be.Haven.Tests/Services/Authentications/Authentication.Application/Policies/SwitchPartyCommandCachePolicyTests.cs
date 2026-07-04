using Authentication.Application.Commands.SwitchParty;
using Authentication.Application.Policies;
using static Authentication.Application.Constants.ApplicationConstants;

namespace Be.Haven.Tests.Services.Authentications.Authentication.Application.Policies;

public sealed class SwitchPartyCommandCachePolicyTests
{
    [Fact]
    public void GetTargets_Should_ReturnCurrentUserTargets_When_UserIsAuthenticated()
    {
        var userPublicId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var authService = new Mock<IAuthService>();
        authService.Setup(x => x.UserId()).Returns(userPublicId);
        var sut = new SwitchPartyCommandCachePolicy(authService.Object);

        var targets = sut.GetTargets(new SwitchPartyCommand());

        var cacheScope = userPublicId.ToUserCacheScope();
        targets.Should().BeEquivalentTo(
        [
            new CacheInvalidationTargetModel(USER_INFO_CACHE_KEY, cacheScope, failOnError: true)
        ]);
    }

    [Fact]
    public void GetTargets_Should_ReturnEmptyTargets_When_UserIsMissing()
    {
        var authService = new Mock<IAuthService>();
        authService.Setup(x => x.UserId()).Returns((Guid?)null);
        var sut = new SwitchPartyCommandCachePolicy(authService.Object);

        var targets = sut.GetTargets(new SwitchPartyCommand());

        targets.Should().BeEmpty();
    }
}
