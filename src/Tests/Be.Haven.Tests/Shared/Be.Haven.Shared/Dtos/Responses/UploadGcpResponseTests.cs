namespace Be.Haven.Tests.Shared.Be.Haven.Shared.Dtos.Responses;

public sealed class UploadGcpResponseTests
{
    [Fact]
    public void Properties_Should_RoundTripValues_When_ResponseIsPopulated()
    {
        // Arrange
        var expected = Guid.NewGuid();

        // Act
        var result = new UploadGcpResponse
        {
            Data = expected,
            Success = false,
            Error = "upload failed"
        };

        // Assert
        result.Data.Should().Be(expected);
        result.Success.Should().BeFalse();
        result.Error.Should().Be("upload failed");
    }
}
