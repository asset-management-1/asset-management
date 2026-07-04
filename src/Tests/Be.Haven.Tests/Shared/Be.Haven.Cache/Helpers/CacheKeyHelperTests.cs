namespace Be.Haven.Tests.Shared.Be.Haven.Cache.Helpers;

public sealed class CacheKeyHelperTests
{
    [Fact]
    public void EmptyParameterHash_Should_ReturnStableHash_When_Called()
    {
        // Arrange
        var expected = CacheKeyHelper.EmptyParameterHash;

        // Act
        var result = CacheKeyHelper.EmptyParameterHash;

        // Assert
        result.Should().Be(expected);
        result.Should().HaveLength(32);
    }

    [Fact]
    public void BuildParameterHash_Should_IgnoreCacheMetadata_When_RequestHasBusinessParameters()
    {
        // Arrange
        var first = new CacheSampleQuery
        {
            CacheKey = "group",
            CacheScope = "scope-a",
            BypassCache = true,
            AbsoluteExpiration = TimeSpan.FromSeconds(5),
            Search = "abc",
            Page = 2
        };
        var second = new CacheSampleQuery
        {
            CacheKey = "different",
            CacheScope = "scope-b",
            BypassCache = false,
            AbsoluteExpiration = TimeSpan.FromSeconds(30),
            Search = "abc",
            Page = 2
        };

        // Act
        var firstHash = CacheKeyHelper.BuildParameterHash(first);
        var secondHash = CacheKeyHelper.BuildParameterHash(second);

        // Assert
        firstHash.Should().Be(secondHash);
    }

    [Fact]
    public void BuildKey_Should_IncludeScopeEpochVersionAndParameterHash_When_RequestHasScope()
    {
        // Arrange
        var request = new CacheSampleQuery
        {
            CacheKey = "auth:user-info",
            CacheScope = "ignored",
            Search = "abc",
            Page = 1
        };
        var hash = CacheKeyHelper.BuildParameterHash(request);

        // Act
        var result = CacheKeyHelper.BuildKey(request, "user:123", "20260101", 5);

        // Assert
        result.Should().Be($"auth:user-info:user:123:e20260101:v5:{hash}");
    }

    [Fact]
    public void BuildKey_Should_UseEmptyParameterHash_When_TargetHasNoParameters()
    {
        // Arrange
        var target = new CacheInvalidationTargetModel("auth:user-info", "user:123");

        // Act
        var result = CacheKeyHelper.BuildKey(target, "20260101", 2);

        // Assert
        result.Should().Be($"auth:user-info:user:123:e20260101:v2:{CacheKeyHelper.EmptyParameterHash}");
    }

    [Fact]
    public void BuildKey_Should_OmitScopeSegment_When_TargetScopeIsBlank()
    {
        // Arrange
        var target = new CacheInvalidationTargetModel("auth:global", string.Empty);

        // Act
        var result = CacheKeyHelper.BuildKey(target, "20260101", 2);

        // Assert
        result.Should().Be($"auth:global:e20260101:v2:{CacheKeyHelper.EmptyParameterHash}");
    }

}
