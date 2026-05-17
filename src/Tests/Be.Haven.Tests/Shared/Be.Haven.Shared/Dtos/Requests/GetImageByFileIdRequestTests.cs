namespace Be.Haven.Tests.Shared.Be.Haven.Shared.Dtos.Requests;

public sealed class GetImageByFileIdRequestTests
{
    [Fact]
    public void Properties_Should_RoundTripValues_When_RequestIsPopulated()
    {
        // Act
        var result = new GetImageByFileIdRequest
        {
            MediaId = "media-123"
        };

        // Assert
        result.MediaId.Should().Be("media-123");
    }
}
