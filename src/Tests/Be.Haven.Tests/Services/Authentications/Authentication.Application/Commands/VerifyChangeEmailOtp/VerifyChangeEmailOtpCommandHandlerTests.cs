using Authentication.Application.Commands.VerifyChangeEmailOtp;
using Authentication.Application.Dtos.Authentications.Otp;
using Authentication.Application.Dtos.Users.Profiles;
using Authentication.Application.Interfaces.Services;

namespace Be.Haven.Tests.Services.Authentications.Authentication.Application.Commands.VerifyChangeEmailOtp;

/// <summary>
/// Verifies concurrent change-email OTP consumption behavior.
/// </summary>
public sealed class VerifyChangeEmailOtpCommandHandlerTests
{
    /// <summary>
    /// Ensures only one concurrent request can update an account from the same valid email OTP.
    /// </summary>
    [Fact]
    public async Task Handle_Should_AllowOnlyOneConcurrentValidOtpConsumer()
    {
        // Arrange
        var userPublicId = Guid.NewGuid();
        var session = new ChangeEmailSessionCacheResponseDto
        {
            NewEmail = "new@example.com",
            Otp = new OtpCacheResponseDto
            {
                Code = "1234",
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5)
            }
        };
        var cachingService = new Mock<ICachingService>();
        cachingService
            .Setup(service => service.GetAsync<ChangeEmailSessionCacheResponseDto>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        cachingService
            .Setup(service => service.RemoveAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var userService = new Mock<IUserService>();
        userService
            .Setup(service => service.VerifyChangeEmailAsync(
                It.IsAny<VerifyChangeEmailOtpRequestDto>(),
                userPublicId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OperationStatusResponseDto());
        var authService = new Mock<IAuthService>();
        authService.Setup(service => service.UserId()).Returns(userPublicId);

        var handler = new VerifyChangeEmailOtpCommandHandler(
            userService.Object,
            cachingService.Object,
            new InMemoryAtomicCacheService(Mock.Of<ILogger<InMemoryAtomicCacheService>>()),
            authService.Object,
            Mock.Of<ILogger<VerifyChangeEmailOtpCommandHandler>>());
        var command = new VerifyChangeEmailOtpCommand
        {
            NewEmail = session.NewEmail,
            Otp = "1234"
        };

        // Act
        var results = await Task.WhenAll(
            ExecuteAsync(handler, command),
            ExecuteAsync(handler, command));

        // Assert
        results.Count(succeeded => succeeded).Should().Be(1);
        userService.Verify(
            service => service.VerifyChangeEmailAsync(
                It.IsAny<VerifyChangeEmailOtpRequestDto>(),
                userPublicId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Executes one email verification and converts an expected business rejection into a result flag.
    /// </summary>
    /// <param name="handler">The handler under test.</param>
    /// <param name="command">The shared change-email OTP command.</param>
    /// <returns><c>true</c> when verification completed; otherwise <c>false</c>.</returns>
    private static async Task<bool> ExecuteAsync(
        VerifyChangeEmailOtpCommandHandler handler,
        VerifyChangeEmailOtpCommand command)
    {
        try
        {
            await handler.Handle(command, CancellationToken.None);
            return true;
        }
        catch (ApiException)
        {
            return false;
        }
    }
}
