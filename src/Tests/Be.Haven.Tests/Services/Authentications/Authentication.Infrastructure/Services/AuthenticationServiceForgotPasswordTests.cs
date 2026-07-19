using Authentication.Application.Interfaces.Repositories;
using Authentication.Domain.Entities;
using Authentication.Infrastructure.Dependencies;
using Authentication.Infrastructure.Options.Authentications;
using Authentication.Infrastructure.Services;
using Be.Haven.Core.Models.ClientDevices;
using Microsoft.AspNetCore.Identity;
using Be.Haven.Core.Interfaces.Repositories;
using AuthenticationService = Authentication.Infrastructure.Services.AuthenticationService;

namespace Be.Haven.Tests.Services.Authentications.Authentication.Infrastructure.Services;

public sealed class AuthenticationServiceForgotPasswordTests
{
    [Fact]
    public async Task ChangeForgotPasswordAsync_Should_RunDatabaseMutationInsideTransaction()
    {
        var user = new User { Id = 10, PublicId = Guid.NewGuid(), Email = "user@example.com" };
        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.GetPasswordIdentityByEmailForUpdateAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        var refreshRepository = new Mock<IRefreshTokenRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(x => x.ExecuteInTransactionAsync(
                It.IsAny<Func<CancellationToken, Task>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task>, CancellationToken>((action, token) => action(token));
        var cache = new Mock<ICachingService>();
        cache.Setup(x => x.SetAbsoluteAsync(
                It.IsAny<string>(), It.IsAny<long>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var repositories = new AuthenticationRepositoryDependencies(
            userRepository.Object,
            refreshRepository.Object,
            Mock.Of<IMasterDataValueRepository>(),
            Mock.Of<IPartyRepository>(),
            Mock.Of<IUserPartyRepository>(),
            Mock.Of<IExternalLoginRepository>());
        var support = new AuthenticationServiceSupportDependencies(
            unitOfWork.Object,
            Mock.Of<IEmailService>(),
            new PasswordHasher<User>(),
            cache.Object,
            OptionsFactory.Create(new AuthOptions { RefreshTokenDays = 30 }),
            OptionsFactory.Create(new EmailOptions()));
        var sut = new AuthenticationService(
            repositories,
            support,
            Mock.Of<IClientDeviceContextAccessor>(),
            Mock.Of<ILogger<AuthenticationService>>());

        await sut.ChangeForgotPasswordAsync(
            new global::Authentication.Application.Dtos.Authentications.Passwords.ChangeForgotPasswordRequestDto
            {
                Email = user.Email,
                NewPassword = "New@123a"
            });

        unitOfWork.Verify(x => x.ExecuteInTransactionAsync(
            It.IsAny<Func<CancellationToken, Task>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
