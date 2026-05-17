namespace Be.Haven.Tests.Shared.Be.Haven.Shared.Dtos.Options.Gcp;

public sealed class GcpStorageOptionsTests
{
    [Fact]
    public void Properties_Should_RoundTripValues_When_OptionsArePopulated()
    {
        // Act
        var result = new GcpStorageOptions
        {
            BasePublicUrl = "https://cdn.haven.test",
            BucketName = "haven-assets"
        };

        // Assert
        result.BasePublicUrl.Should().Be("https://cdn.haven.test");
        result.BucketName.Should().Be("haven-assets");
    }
}
