namespace Be.Haven.Tests.Shared.Be.Haven.ApiCommon.Routing;

public sealed class SlugifyTransformerTests
{
    [Theory]
    [InlineData("UserInfo", "user-info")]
    [InlineData("APIKey", "api-key")]
    [InlineData("userID", "user-id")]
    [InlineData("OAuthCallbackURL", "o-auth-callback-url")]
    public void TransformOutbound_Should_ReturnKebabCase_When_ValueHasWordBoundaries(
        string value,
        string expected)
    {
        // Arrange
        var sut = new SlugifyTransformer();

        // Act
        var result = sut.TransformOutbound(value);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void TransformOutbound_Should_ReturnNull_When_ValueIsNull()
    {
        // Arrange
        var sut = new SlugifyTransformer();

        // Act
        var result = sut.TransformOutbound(null);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void TransformOutbound_Should_ReturnEmptyString_When_ValueIsEmpty()
    {
        // Arrange
        var sut = new SlugifyTransformer();

        // Act
        var result = sut.TransformOutbound(string.Empty);

        // Assert
        result.Should().BeEmpty();
    }
}
