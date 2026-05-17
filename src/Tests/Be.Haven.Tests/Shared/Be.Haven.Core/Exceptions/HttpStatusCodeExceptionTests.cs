namespace Be.Haven.Tests.Shared.Be.Haven.Core.Exceptions;

public sealed class HttpStatusCodeExceptionTests
{
    [Fact]
    public void Constructor_Should_SetStatusCode_When_ErrorCodeIsNotProvided()
    {
        // Arrange
        var statusCode = StatusCodes.Status404NotFound;

        // Act
        var result = new HttpStatusCodeException("Missing", statusCode);

        // Assert
        result.Message.Should().Be("Missing");
        result.StatusCode.Should().Be(statusCode);
        result.ErrorCode.Should().BeNull();
    }

    [Fact]
    public void Constructor_Should_SetErrorCodeAndStatusCode_When_AllValuesAreProvided()
    {
        // Arrange
        var statusCode = StatusCodes.Status503ServiceUnavailable;

        // Act
        var result = new HttpStatusCodeException("Unavailable", "error_unavailable", statusCode);

        // Assert
        result.Message.Should().Be("Unavailable");
        result.ErrorCode.Should().Be("error_unavailable");
        result.StatusCode.Should().Be(statusCode);
    }
}
