namespace Be.Haven.Tests.Shared.Be.Haven.Shared.Dtos.Options.Caching;

public sealed class CacheOptionsTests
{
    [Fact]
    public void AbsoluteExpiration_Should_DefaultToTenSeconds_When_NotConfigured()
    {
        // Act
        var result = new CacheOptions();

        // Assert
        result.AbsoluteExpiration.Should().Be(10);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AbsoluteExpiration_Should_ResetToDefault_When_ValueIsNotPositive(int value)
    {
        // Arrange
        var sut = new CacheOptions();

        // Act
        sut.AbsoluteExpiration = value;

        // Assert
        sut.AbsoluteExpiration.Should().Be(10);
    }

    [Fact]
    public void AbsoluteExpiration_Should_UseConfiguredValue_When_ValueIsPositive()
    {
        // Arrange
        var sut = new CacheOptions();

        // Act
        sut.AbsoluteExpiration = 60;

        // Assert
        sut.AbsoluteExpiration.Should().Be(60);
    }
}
