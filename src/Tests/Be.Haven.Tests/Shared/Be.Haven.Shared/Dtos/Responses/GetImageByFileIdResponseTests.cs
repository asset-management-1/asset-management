namespace Be.Haven.Tests.Shared.Be.Haven.Shared.Dtos.Responses;

public sealed class GetImageByFileIdResponseTests
{
    [Fact]
    public void Properties_Should_RoundTripValues_When_ResponseIsPopulated()
    {
        // Arrange
        var expireDate = new DateTime(2026, 5, 17, 10, 30, 0, DateTimeKind.Utc);
        var detail = new DetailObject
        {
            Url = "https://cdn.haven.test/image.png",
            Thumbnail = "https://cdn.haven.test/thumb.png",
            ExpireDate = expireDate
        };

        // Act
        var result = new GetImageByFileIdResponse
        {
            Success = true,
            Data = detail,
            Error = "none"
        };

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().BeSameAs(detail);
        result.Data.Url.Should().Be("https://cdn.haven.test/image.png");
        result.Data.Thumbnail.Should().Be("https://cdn.haven.test/thumb.png");
        result.Data.ExpireDate.Should().Be(expireDate);
        result.Error.Should().Be("none");
    }
}
