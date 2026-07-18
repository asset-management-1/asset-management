namespace Be.Haven.Tests.Shared.Be.Haven.Cache.Services;

public sealed class InMemoryAtomicCacheServiceTests
{
    [Fact]
    public async Task TrySetIfAbsentAsync_Should_AllowOnlyOneConcurrentWriter()
    {
        // Arrange
        var sut = CreateSut();
        var tasks = Enumerable.Range(0, 20)
            .Select(_ => sut.TrySetIfAbsentAsync("otp:cooldown", "1", TimeSpan.FromMinutes(1)));

        // Act
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Count(result => result).Should().Be(1);
    }

    [Fact]
    public async Task IncrementAsync_Should_NotLoseConcurrentIncrements()
    {
        // Arrange
        var sut = CreateSut();
        var tasks = Enumerable.Range(0, 50)
            .Select(_ => sut.IncrementAsync("otp:attempt", TimeSpan.FromMinutes(1)));

        // Act
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().OnlyHaveUniqueItems();
        results.Max().Should().Be(50);
    }

    [Fact]
    public async Task IncrementAsync_Should_StartNewBucketAfterExpiration()
    {
        // Arrange
        var sut = CreateSut();
        await sut.IncrementAsync("otp:attempt", TimeSpan.FromMilliseconds(20));
        await Task.Delay(40);

        // Act
        var result = await sut.IncrementAsync("otp:attempt", TimeSpan.FromMinutes(1));

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task RemoveAsync_Should_ClearAtomicState()
    {
        // Arrange
        var sut = CreateSut();
        await sut.IncrementAsync("otp:attempt", TimeSpan.FromMinutes(1));

        // Act
        await sut.RemoveAsync("otp:attempt");
        var result = await sut.IncrementAsync("otp:attempt", TimeSpan.FromMinutes(1));

        // Assert
        result.Should().Be(1);
    }

    private static InMemoryAtomicCacheService CreateSut()
    {
        return new InMemoryAtomicCacheService(Mock.Of<ILogger<InMemoryAtomicCacheService>>());
    }
}
