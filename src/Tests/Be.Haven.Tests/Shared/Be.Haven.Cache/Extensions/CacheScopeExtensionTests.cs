namespace Be.Haven.Tests.Shared.Be.Haven.Cache.Extensions;

public sealed class CacheScopeExtensionTests
{
    [Fact]
    public void ToUserCacheScope_Should_ReturnUserScope_When_GuidIsProvided()
    {
        // Arrange
        var userPublicId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var result = userPublicId.ToUserCacheScope();

        // Assert
        result.Should().Be("user:11111111-1111-1111-1111-111111111111");
    }
}
