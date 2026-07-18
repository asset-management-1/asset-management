using Authentication.Application.Commands.VerifyRegisterEmail;
using Authentication.Application.Dtos.Authentications.Otp;
using Authentication.Application.Dtos.Authentications.Register;
using AuthenticationService = Authentication.Application.Interfaces.Services.IAuthenticationService;

namespace Be.Haven.Tests.Services.Authentications.Authentication.Application.Commands.VerifyRegisterEmail;

/// <summary>
/// Verifies concurrent registration OTP consumption behavior.
/// </summary>
public sealed class VerifyRegisterEmailCommandHandlerTests
{
    /// <summary>
    /// Ensures only one concurrent request can create an account from the same valid registration OTP.
    /// </summary>
    [Fact]
    public async Task Handle_Should_AllowOnlyOneConcurrentValidOtpConsumer()
    {
        // Arrange
        var session = new RegisterSessionCacheRequestDto
        {
            PendingRegister = new PendingRegisterCacheRequestDto(),
            Otp = new OtpCacheResponseDto
            {
                Code = "1234",
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5)
            }
        };
        var cachingService = new Mock<ICachingService>();
        cachingService
            .Setup(service => service.GetAsync<RegisterSessionCacheRequestDto>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        cachingService
            .Setup(service => service.RemoveAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var authenticationService = new Mock<AuthenticationService>();
        authenticationService
            .Setup(service => service.CompleteRegistrationAsync(
                session.PendingRegister,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OperationStatusResponseDto());

        var handler = new VerifyRegisterEmailCommandHandler(
            authenticationService.Object,
            cachingService.Object,
            new InMemoryAtomicCacheService(Mock.Of<ILogger<InMemoryAtomicCacheService>>()),
            Mock.Of<ILogger<VerifyRegisterEmailCommandHandler>>());
        var command = new VerifyRegisterEmailCommand
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
        authenticationService.Verify(
            service => service.CompleteRegistrationAsync(
                session.PendingRegister,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Executes one registration verification and converts an expected business rejection into a result flag.
    /// </summary>
    /// <param name="handler">The handler under test.</param>
    /// <param name="command">The shared registration OTP command.</param>
    /// <returns><c>true</c> when verification completed; otherwise <c>false</c>.</returns>
    private static async Task<bool> ExecuteAsync(
        VerifyRegisterEmailCommandHandler handler,
        VerifyRegisterEmailCommand command)
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
