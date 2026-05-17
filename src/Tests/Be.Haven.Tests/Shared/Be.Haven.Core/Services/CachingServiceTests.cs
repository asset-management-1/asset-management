namespace Be.Haven.Tests.Shared.Be.Haven.Core.Services;

public sealed class CachingServiceTests
{
    [Fact]
    public async Task GetAsync_Should_ReturnDefault_When_KeyIsMissing()
    {
        // Arrange
        var cache = new FakeDistributedCache();
        var serializer = new Mock<IJsonSerializerService>();
        var sut = CreateSut(cache, serializer);

        // Act
        var result = await sut.GetAsync<CoreSerializationModel>("missing-key");

        // Assert
        result.Should().BeNull();
        serializer.Verify(x => x.Deserialize<CoreSerializationModel>(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetAsync_Should_DeserializeCachedJson_When_KeyExists()
    {
        // Arrange
        var cache = new FakeDistributedCache();
        cache.Values["profile"] = Encoding.UTF8.GetBytes("""{"displayName":"Haven"}""");
        var expected = new CoreSerializationModel
        {
            DisplayName = "Haven"
        };
        var serializer = new Mock<IJsonSerializerService>();
        serializer
            .Setup(x => x.Deserialize<CoreSerializationModel>("""
                {"displayName":"Haven"}
                """.ReplaceLineEndings(string.Empty).Trim()))
            .Returns(expected);
        var sut = CreateSut(cache, serializer);

        // Act
        var result = await sut.GetAsync<CoreSerializationModel>("profile");

        // Assert
        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task SetAsync_Should_WriteSerializedDataWithSlidingExpiration_When_Called()
    {
        // Arrange
        var cache = new FakeDistributedCache();
        var model = new CoreSerializationModel
        {
            DisplayName = "Haven"
        };
        var serializer = new Mock<IJsonSerializerService>();
        serializer.Setup(x => x.Serialize(model)).Returns("""{"displayName":"Haven"}""");
        var sut = CreateSut(cache, serializer);

        // Act
        await sut.SetAsync("profile", model, TimeSpan.FromSeconds(30));

        // Assert
        Encoding.UTF8.GetString(cache.Values["profile"]).Should().Be("""{"displayName":"Haven"}""");
        cache.OptionsByKey["profile"].SlidingExpiration.Should().Be(TimeSpan.FromSeconds(30));
        cache.OptionsByKey["profile"].AbsoluteExpirationRelativeToNow.Should().BeNull();
    }

    [Fact]
    public async Task SetAbsoluteAsync_Should_WriteSerializedDataWithAbsoluteExpiration_When_Called()
    {
        // Arrange
        var cache = new FakeDistributedCache();
        var model = new CoreSerializationModel
        {
            DisplayName = "Haven"
        };
        var serializer = new Mock<IJsonSerializerService>();
        serializer.Setup(x => x.Serialize(model)).Returns("""{"displayName":"Haven"}""");
        var sut = CreateSut(cache, serializer);

        // Act
        await sut.SetAbsoluteAsync("profile", model, TimeSpan.FromMinutes(1));

        // Assert
        Encoding.UTF8.GetString(cache.Values["profile"]).Should().Be("""{"displayName":"Haven"}""");
        cache.OptionsByKey["profile"].AbsoluteExpirationRelativeToNow.Should().Be(TimeSpan.FromMinutes(1));
        cache.OptionsByKey["profile"].SlidingExpiration.Should().BeNull();
    }

    [Fact]
    public async Task RemoveAsync_Should_RemoveKeyFromDistributedCache_When_KeyExists()
    {
        // Arrange
        var cache = new FakeDistributedCache();
        cache.Values["profile"] = Encoding.UTF8.GetBytes("cached");
        var serializer = new Mock<IJsonSerializerService>();
        var sut = CreateSut(cache, serializer);

        // Act
        await sut.RemoveAsync("profile");

        // Assert
        cache.Values.Should().NotContainKey("profile");
        cache.RemovedKeys.Should().ContainSingle().Which.Should().Be("profile");
    }

    private static CachingService CreateSut(
        IDistributedCache cache,
        Mock<IJsonSerializerService> serializer)
    {
        return new CachingService(
            cache,
            Mock.Of<ILogger<CachingService>>(),
            serializer.Object);
    }
}
