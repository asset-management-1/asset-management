using Authentication.Application.Commands.VerifyForgotPasswordOtp;
using Authentication.Application.Dtos.Authentications.Otp;
using Authentication.Application.Dtos.Authentications.Passwords;

namespace Be.Haven.Tests.Services.Authentications.Authentication.Application.Commands.VerifyForgotPasswordOtp;

/// <summary>
/// Verifies concurrent forgot-password OTP consumption behavior.
/// </summary>
public sealed class VerifyForgotPasswordOtpCommandHandlerTests
{
    /// <summary>
    /// Ensures only one concurrent request can complete with the same valid OTP.
    /// </summary>
    [Fact]
    public async Task Handle_Should_AllowOnlyOneConcurrentValidOtpConsumer()
    {
        // Arrange
        var cachingService = new Mock<ICachingService>();
        cachingService
            .Setup(service => service.GetAsync<OtpCacheResponseDto>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OtpCacheResponseDto
            {
                Code = "1234",
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5)
            });
        cachingService
            .Setup(service => service.RemoveAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        cachingService
            .Setup(service => service.SetAbsoluteAsync(
                It.IsAny<string>(),
                It.IsAny<ResetSessionCacheResponseDto>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new VerifyForgotPasswordOtpCommandHandler(
            cachingService.Object,
            new InMemoryAtomicCacheService(Mock.Of<ILogger<InMemoryAtomicCacheService>>()),
            Mock.Of<ILogger<VerifyForgotPasswordOtpCommandHandler>>());
        var command = new VerifyForgotPasswordOtpCommand
        {
            Email = "user@example.com",
            Otp = "1234"
        };

        // Act
        var results = await Task.WhenAll(
            ExecuteAsync(handler, command),
            ExecuteAsync(handler, command));

        // Assert
        results.Count(succeeded => succeeded).Should().Be(1);
        cachingService.Verify(
            service => service.SetAbsoluteAsync(
                It.IsAny<string>(),
                It.IsAny<ResetSessionCacheResponseDto>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Ensures a reset-session write failure releases the OTP claim for a safe retry.
    /// </summary>
    [Fact]
    public async Task Handle_Should_AllowRetry_WhenResetSessionCreationFails()
    {
        // Arrange
        var cachingService = new Mock<ICachingService>();
        cachingService
            .Setup(service => service.GetAsync<OtpCacheResponseDto>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OtpCacheResponseDto
            {
                Code = "1234",
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5)
            });
        cachingService
            .Setup(service => service.RemoveAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        cachingService
            .SetupSequence(service => service.SetAbsoluteAsync(
                It.IsAny<string>(),
                It.IsAny<ResetSessionCacheResponseDto>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Transient cache failure."))
            .Returns(Task.CompletedTask);

        var handler = new VerifyForgotPasswordOtpCommandHandler(
            cachingService.Object,
            new InMemoryAtomicCacheService(Mock.Of<ILogger<InMemoryAtomicCacheService>>()),
            Mock.Of<ILogger<VerifyForgotPasswordOtpCommandHandler>>());
        var command = new VerifyForgotPasswordOtpCommand
        {
            Email = "user@example.com",
            Otp = "1234"
        };

        // Act
        var firstAttempt = () => handler.Handle(command, CancellationToken.None).AsTask();
        await firstAttempt.Should().ThrowAsync<InvalidOperationException>();
        var secondAttempt = await handler.Handle(command, CancellationToken.None);

        // Assert
        secondAttempt.Should().NotBeNull();
        cachingService.Verify(
            service => service.SetAbsoluteAsync(
                It.IsAny<string>(),
                It.IsAny<ResetSessionCacheResponseDto>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    /// <summary>
    /// Executes one verification request and converts the expected business rejection into a result flag.
    /// </summary>
    /// <param name="handler">The handler under test.</param>
    /// <param name="command">The shared OTP verification command.</param>
    /// <returns><c>true</c> when verification completed; otherwise <c>false</c>.</returns>
    private static async Task<bool> ExecuteAsync(
        VerifyForgotPasswordOtpCommandHandler handler,
        VerifyForgotPasswordOtpCommand command)
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
