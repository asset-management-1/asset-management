using Authentication.Application.Interfaces.Repositories;
using Authentication.Application.Interfaces.Services;
using Authentication.Application.Dtos.Authentications.ExternalProviders;
using Authentication.Domain.Entities;
using Authentication.Infrastructure.Dependencies;
using Authentication.Infrastructure.Options.Authentications;
using Authentication.Infrastructure.Options.ExternalProviders;
using Authentication.Infrastructure.Services;
using Be.Haven.Core.Interfaces.Repositories;
using Be.Haven.Core.Models.ClientDevices;

namespace Be.Haven.Tests.Services.Authentications.Authentication.Infrastructure.Services;

public sealed class ExternalAuthenticationServiceTests
{
    [Fact]
    public async Task LoginAsync_Should_ReturnPrefillWithoutPersistence_WhenIdentityIsFirstTime()
    {
        var identityProviderService = new Mock<IExternalIdentityProviderService>();
        identityProviderService.Setup(x => x.GetValidatedIdentityAsync("google", "token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExternalIdentityProfileResponseDto
            {
                ProviderUserId = "provider-user",
                Email = "new@example.com",
                FullName = "New User",
                EmailVerified = true
            });
        var externalLoginRepository = new Mock<IExternalLoginRepository>();
        var userRepository = new Mock<IUserRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var repositories = new AuthenticationRepositoryDependencies(
            userRepository.Object,
            Mock.Of<IRefreshTokenRepository>(),
            Mock.Of<IMasterDataValueRepository>(),
            Mock.Of<IPartyRepository>(),
            Mock.Of<IUserPartyRepository>(),
            externalLoginRepository.Object);
        var sut = CreateSut(
            repositories,
            unitOfWork.Object,
            identityProviderService.Object);

        var result = await sut.LoginAsync(new ExternalLoginRequestDto
        {
            Provider = "google",
            ExternalToken = "token"
        });

        result.IsNewRegistration.Should().BeTrue();
        result.Registration.Email.Should().Be("new@example.com");
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(x => x.ExecuteInTransactionAsync(
            It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CompleteRegistrationAsync_Should_RejectExistingInactiveMapping()
    {
        var identityProviderService = new Mock<IExternalIdentityProviderService>();
        identityProviderService.Setup(x => x.GetValidatedIdentityAsync("google", "token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExternalIdentityProfileResponseDto
            {
                ProviderUserId = "provider-user",
                Email = "user@example.com",
                EmailVerified = true
            });
        var externalLoginRepository = new Mock<IExternalLoginRepository>();
        externalLoginRepository.Setup(x => x.GetByProviderAsync(
                "google", "provider-user", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExternalLogin
            {
                User = new User
                {
                    IsDeleted = true,
                    EmailConfirmed = true,
                    Status = new MasterDataValue { Code = "ACTIVE" }
                }
            });
        var repositories = new AuthenticationRepositoryDependencies(
            Mock.Of<IUserRepository>(),
            Mock.Of<IRefreshTokenRepository>(),
            Mock.Of<IMasterDataValueRepository>(),
            Mock.Of<IPartyRepository>(),
            Mock.Of<IUserPartyRepository>(),
            externalLoginRepository.Object);
        var sut = CreateSut(
            repositories,
            Mock.Of<IUnitOfWork>(),
            identityProviderService.Object);

        Func<Task> act = () => sut.CompleteRegistrationAsync(new CompleteExternalRegistrationRequestDto
        {
            Provider = "google",
            ExternalToken = "token"
        });

        var exception = await act.Should().ThrowAsync<ApiException>();
        exception.Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    private static ExternalAuthenticationService CreateSut(
        AuthenticationRepositoryDependencies repositories,
        IUnitOfWork unitOfWork,
        IExternalIdentityProviderService identityProviderService)
    {
        return new ExternalAuthenticationService(
            repositories,
            unitOfWork,
            OptionsFactory.Create(new ExternalAuthenticationOptions
            {
                Providers = [new ExternalProviderOptions { Name = "google" }]
            }),
            OptionsFactory.Create(new AuthOptions()),
            Mock.Of<ILogger<ExternalAuthenticationService>>(),
            Mock.Of<IClientDeviceContextAccessor>(),
            identityProviderService);
    }
}
