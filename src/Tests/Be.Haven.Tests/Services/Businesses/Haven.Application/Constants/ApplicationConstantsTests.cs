namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Constants;

public sealed class ApplicationConstantsTests
{
    [Fact]
    public void TenantJoinTokenExpiryMinutes_Should_BeFiveMinutes()
    {
        // Act
        var result = TENANT_JOIN_TOKEN_EXPIRY_MINUTES;

        // Assert
        result.Should().Be(5);
    }
}
