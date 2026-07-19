using Authentication.Application.Interfaces.Repositories;
using Authentication.Domain.Entities;
using Authentication.Infrastructure.Dependencies;
using Authentication.Infrastructure.Options.Authentications;
using Authentication.Infrastructure.Services;
using Be.Haven.Core.Interfaces.Repositories;
using Be.Haven.Core.Models.ClientDevices;
using Microsoft.AspNetCore.Identity;
using AuthenticationService = Authentication.Infrastructure.Services.AuthenticationService;

namespace Be.Haven.Tests.Services.Authentications.Authentication.Infrastructure.Services;

public sealed class AuthenticationServiceRefreshTokenTests
{
    [Theory]
    [InlineData("missing")]
    [InlineData("expired")]
    [InlineData("revoked")]
    public async Task RefreshTokenAsync_Should_ReturnUnauthorized_WhenSessionCannotBeUsed(string state)
    {
        var refreshRepository = new Mock<IRefreshTokenRepository>();
        var token = state == "missing" ? null : CreateToken(state);
        refreshRepository.Setup(x => x.GetForRefreshForUpdateAsync(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);
        var sut = CreateSut(refreshRepository.Object);

        Func<Task> act = () => sut.RefreshTokenAsync(
            new global::Authentication.Application.Dtos.Authentications.Sessions.RefreshTokenRequestDto
            {
                RefreshToken = "refresh-token"
            });

        var exception = await act.Should().ThrowAsync<ApiException>();
        exception.Which.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    private static RefreshToken CreateToken(string state) => new()
    {
        SessionPublicId = Guid.NewGuid(),
        TokenHash = "not-relevant",
        ExpiresAt = state == "expired" ? DateTime.UtcNow.AddMinutes(-1) : DateTime.UtcNow.AddDays(1),
        RevokedAt = state == "revoked" ? DateTime.UtcNow : null,
        User = new User
        {
            EmailConfirmed = true,
            Status = new MasterDataValue { Code = "ACTIVE" }
        }
    };

    private static AuthenticationService CreateSut(IRefreshTokenRepository refreshRepository)
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(x => x.ExecuteInTransactionAsync(
                It.IsAny<Func<CancellationToken, Task<global::Authentication.Application.Dtos.Users.Sessions.LoginResponseDto>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task<global::Authentication.Application.Dtos.Users.Sessions.LoginResponseDto>>, CancellationToken>(
                (action, token) => action(token));
        var repositories = new AuthenticationRepositoryDependencies(
            Mock.Of<IUserRepository>(),
            refreshRepository,
            Mock.Of<IMasterDataValueRepository>(),
            Mock.Of<IPartyRepository>(),
            Mock.Of<IUserPartyRepository>(),
            Mock.Of<IExternalLoginRepository>());
        var support = new AuthenticationServiceSupportDependencies(
            unitOfWork.Object,
            Mock.Of<IEmailService>(),
            new PasswordHasher<User>(),
            Mock.Of<ICachingService>(),
            OptionsFactory.Create(new AuthOptions()),
            OptionsFactory.Create(new EmailOptions()));
        var device = new Mock<IClientDeviceContextAccessor>();
        device.Setup(x => x.GetCurrent()).Returns(new ClientDeviceContextModel { DeviceId = "device" });
        return new AuthenticationService(
            repositories,
            support,
            device.Object,
            Mock.Of<ILogger<AuthenticationService>>());
    }
}
