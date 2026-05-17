namespace Be.Haven.Tests.Shared.Be.Haven.Cache.Models;

public sealed class CacheInvalidationTargetModelTests
{
    [Fact]
    public void Equals_Should_ReturnTrue_When_TargetValuesMatch()
    {
        // Arrange
        var sut = new CacheInvalidationTargetModel("auth:user-info", "user:1", true);
        var model = new CacheInvalidationTargetModel("auth:user-info", "user:1", true);

        // Act
        var result = sut.Equals(model);

        // Assert
        result.Should().BeTrue();
        sut.GetHashCode().Should().Be(model.GetHashCode());
    }

    [Fact]
    public void Equals_Should_ReturnFalse_When_TargetValuesDiffer()
    {
        // Arrange
        var sut = new CacheInvalidationTargetModel("auth:user-info", "user:1", true);
        var model = new CacheInvalidationTargetModel("auth:user-info", "user:2", true);

        // Act
        var result = sut.Equals(model);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_Should_ReturnFalse_When_OtherObjectIsNullOrDifferentType()
    {
        // Arrange
        var sut = new CacheInvalidationTargetModel("auth:user-info");

        // Act
        var nullResult = sut.Equals(null);
        var objectResult = sut.Equals(new object());

        // Assert
        nullResult.Should().BeFalse();
        objectResult.Should().BeFalse();
    }
}
