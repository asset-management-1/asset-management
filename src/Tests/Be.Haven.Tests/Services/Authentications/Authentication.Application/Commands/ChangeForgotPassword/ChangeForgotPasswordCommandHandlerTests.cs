using Authentication.Application.Commands.ChangeForgotPassword;
using Authentication.Application.Dtos.Authentications.Passwords;

namespace Be.Haven.Tests.Services.Authentications.Authentication.Application.Commands.ChangeForgotPassword;

public sealed class ChangeForgotPasswordCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_AllowOnlyOneConcurrentResetTokenConsumer()
    {
        var cachingService = new Mock<ICachingService>();
        cachingService.Setup(x => x.GetAsync<ResetSessionCacheResponseDto>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResetSessionCacheResponseDto
            {
                Email = "user@example.com",
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5)
            });
        cachingService.Setup(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var authenticationService = new Mock<global::Authentication.Application.Interfaces.Services.IAuthenticationService>();
        authenticationService.Setup(x => x.ChangeForgotPasswordAsync(
                It.IsAny<ChangeForgotPasswordRequestDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationStatusResponseHelper.Success("Password changed successfully."));
        var sut = new ChangeForgotPasswordCommandHandler(
            authenticationService.Object,
            cachingService.Object,
            new InMemoryAtomicCacheService(Mock.Of<ILogger<InMemoryAtomicCacheService>>()),
            Mock.Of<ILogger<ChangeForgotPasswordCommandHandler>>());
        var command = new ChangeForgotPasswordCommand
        {
            PasswordResetToken = "reset-token",
            NewPassword = "New@123a"
        };

        var results = await Task.WhenAll(
            ExecuteAsync(sut, command),
            ExecuteAsync(sut, command));

        results.Count(x => x).Should().Be(1);
        authenticationService.Verify(x => x.ChangeForgotPasswordAsync(
            It.Is<ChangeForgotPasswordRequestDto>(request => request.Email == "user@example.com"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private static async Task<bool> ExecuteAsync(
        ChangeForgotPasswordCommandHandler sut,
        ChangeForgotPasswordCommand command)
    {
        try
        {
            await sut.Handle(command, CancellationToken.None);
            return true;
        }
        catch (ApiException)
        {
            return false;
        }
    }
}
