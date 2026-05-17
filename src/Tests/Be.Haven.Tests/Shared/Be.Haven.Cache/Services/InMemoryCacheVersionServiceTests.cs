using System.Reflection;

namespace Be.Haven.Tests.Shared.Be.Haven.Cache.Services;

public sealed class InMemoryCacheVersionServiceTests
{
    [Fact]
    public async Task GetAsync_Should_ReturnDefaultVersion_When_KeyIsMissing()
    {
        // Arrange
        var sut = new InMemoryCacheVersionService();

        // Act
        var result = await sut.GetAsync("group", "user:1", "20260101");

        // Assert
        result.Should().Be(DEFAULT_VERSION);
    }

    [Fact]
    public async Task InvalidateAsync_Should_AdvanceVersion_When_KeyExists()
    {
        // Arrange
        var sut = new InMemoryCacheVersionService();
        var cacheGroup = $"group-{Guid.NewGuid():N}";

        // Act
        var first = await sut.InvalidateAsync(cacheGroup, "user:1", "20260101");
        var second = await sut.InvalidateAsync(cacheGroup, "user:1", "20260101");
        var current = await sut.GetAsync(cacheGroup, "user:1", "20260101");

        // Assert
        first.Should().Be(DEFAULT_VERSION + 1);
        second.Should().Be(DEFAULT_VERSION + 2);
        current.Should().Be(second);
    }

    [Fact]
    public async Task InvalidateAsync_Should_ThrowArgumentException_When_GroupIsMissing()
    {
        // Arrange
        var sut = new InMemoryCacheVersionService();

        // Act
        var action = () => sut.InvalidateAsync(string.Empty, "scope", "20260101");

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData("", "scope", "20260101")]
    [InlineData("group", "scope", "")]
    public async Task GetAsync_Should_ThrowArgumentException_When_RequiredArgumentsAreBlank(
        string cacheGroup,
        string cacheScope,
        string epoch)
    {
        // Arrange
        var sut = new InMemoryCacheVersionService();

        // Act
        var action = () => sut.GetAsync(cacheGroup, cacheScope, epoch);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task InvalidateAsync_Should_UseUnscopedVersionKey_When_ScopeIsBlank()
    {
        // Arrange
        var sut = new InMemoryCacheVersionService();
        var cacheGroup = $"unscoped-{Guid.NewGuid():N}";

        // Act
        var result = await sut.InvalidateAsync(cacheGroup, string.Empty, "20260101");
        var current = await sut.GetAsync(cacheGroup, string.Empty, "20260101");

        // Assert
        result.Should().Be(DEFAULT_VERSION + 1);
        current.Should().Be(result);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnDefaultVersionAndRemoveState_When_VersionStateExpired()
    {
        // Arrange
        var sut = new InMemoryCacheVersionService();
        var cacheGroup = $"expired-{Guid.NewGuid():N}";
        var key = $"ver:{cacheGroup}:user:1:20260101";
        var versions = GetVersionRegistry();
        AddVersionState(versions, key, DEFAULT_VERSION + 5, DateTimeOffset.UtcNow.AddSeconds(-1)).Should().BeTrue();

        // Act
        var result = await sut.GetAsync(cacheGroup, "user:1", "20260101");

        // Assert
        result.Should().Be(DEFAULT_VERSION);
        ContainsVersionState(versions, key).Should().BeFalse();
    }

    [Fact]
    public async Task GetAsync_Should_ThrowInvalidOperationException_When_VersionStateIsUnsafe()
    {
        // Arrange
        var sut = new InMemoryCacheVersionService();
        var cacheGroup = $"unsafe-{Guid.NewGuid():N}";
        var versions = GetVersionRegistry();
        AddVersionState(
            versions,
            $"ver:{cacheGroup}:user:1:20260101",
            0,
            DateTimeOffset.UtcNow.AddMinutes(5)).Should().BeTrue();

        // Act
        var action = () => sut.GetAsync(cacheGroup, "user:1", "20260101");

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task InvalidateAsync_Should_RestartFromDefaultVersion_When_CurrentVersionStateExpired()
    {
        // Arrange
        var sut = new InMemoryCacheVersionService();
        var cacheGroup = $"restart-{Guid.NewGuid():N}";
        var versions = GetVersionRegistry();
        AddVersionState(
            versions,
            $"ver:{cacheGroup}:user:1:20260101",
            DEFAULT_VERSION + 10,
            DateTimeOffset.UtcNow.AddSeconds(-1)).Should().BeTrue();

        // Act
        var result = await sut.InvalidateAsync(cacheGroup, "user:1", "20260101");

        // Assert
        result.Should().Be(DEFAULT_VERSION + 1);
    }

    [Fact]
    public void GetEpoch_Should_ReturnUtcDateEpoch_When_Called()
    {
        // Arrange
        var sut = new InMemoryCacheVersionService();

        // Act
        var result = sut.GetEpoch();

        // Assert
        result.Should().HaveLength(8);
        result.Should().MatchRegex("^\\d{8}$");
    }

    private static object GetVersionRegistry()
    {
        var field = typeof(InMemoryCacheVersionService).GetField(
            "VersionsByKey",
            BindingFlags.Static | BindingFlags.NonPublic);

        return field.GetValue(null);
    }

    private static bool AddVersionState(
        object registry,
        string key,
        long version,
        DateTimeOffset expiresAtUtc)
    {
        var method = registry.GetType().GetMethod("TryAdd");

        return (bool)method.Invoke(registry, [key, CreateVersionState(version, expiresAtUtc)]);
    }

    private static bool ContainsVersionState(object registry, string key)
    {
        var method = registry.GetType().GetMethod("ContainsKey");

        return (bool)method.Invoke(registry, [key]);
    }

    private static object CreateVersionState(long version, DateTimeOffset expiresAtUtc)
    {
        var modelType = typeof(InMemoryCacheVersionService).Assembly.GetType("Be.Haven.Cache.Models.CacheVersionStateModel");

        return Activator.CreateInstance(
            modelType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            args: [version, expiresAtUtc],
            culture: CultureInfo.InvariantCulture);
    }
}
