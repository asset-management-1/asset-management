using System.Collections.Concurrent;
using System.Reflection;

namespace Be.Haven.Tests.Shared.Be.Haven.Cache.Services;

public sealed class InMemoryCacheBypassServiceTests
{
    [Fact]
    public async Task ShouldBypassAsync_Should_ReturnFalse_When_GroupIsMissing()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var result = await sut.ShouldBypassAsync(string.Empty, "scope", CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldBypassAsync_Should_ReturnTrue_When_MarkerWasSet()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        await sut.MarkBypassAsync("group", "user:1", CancellationToken.None);
        var result = await sut.ShouldBypassAsync("group", "user:1", CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldBypassAsync_Should_ReturnFalse_When_MarkerDoesNotExist()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var result = await sut.ShouldBypassAsync("group", "user:1", CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldBypassAsync_Should_ReturnTrue_When_UnscopedMarkerWasSet()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        await sut.MarkBypassAsync("group", string.Empty, CancellationToken.None);
        var result = await sut.ShouldBypassAsync("group", string.Empty, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task MarkBypassAsync_Should_IgnoreBlankGroup_When_Called()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        await sut.MarkBypassAsync(" ", "user:1", CancellationToken.None);
        var result = await sut.ShouldBypassAsync(" ", "user:1", CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldBypassAsync_Should_ReturnFalseAndRemoveMarker_When_MarkerExpired()
    {
        // Arrange
        var sut = CreateSut();
        var registry = GetBypassRegistry(sut);
        registry["group:user:1"] = DateTimeOffset.UtcNow.AddSeconds(-1);

        // Act
        var result = await sut.ShouldBypassAsync("group", "user:1", CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        registry.Should().NotContainKey("group:user:1");
    }

    private static ConcurrentDictionary<string, DateTimeOffset> GetBypassRegistry(InMemoryCacheBypassService sut)
    {
        var field = typeof(InMemoryCacheBypassService).GetField(
            "_bypassUntilByKey",
            BindingFlags.Instance | BindingFlags.NonPublic);

        return (ConcurrentDictionary<string, DateTimeOffset>)field.GetValue(sut);
    }

    private static InMemoryCacheBypassService CreateSut()
    {
        return new InMemoryCacheBypassService(Mock.Of<ILogger<InMemoryCacheBypassService>>());
    }
}
