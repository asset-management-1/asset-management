namespace Be.Haven.Tests.Shared.Be.Haven.Core.Helpers.Attributes;

public sealed class NotEmptyCollectionAttributeTests
{
    [Theory]
    [MemberData(nameof(InvalidCollections))]
    public void IsValid_Should_ReturnFalse_When_ValueIsNotACollectionWithItems(object value)
    {
        // Arrange
        var sut = new NotEmptyCollectionAttribute();

        // Act
        var result = sut.IsValid(value);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_Should_ReturnTrue_When_CollectionContainsAtLeastOneItem()
    {
        // Arrange
        var sut = new NotEmptyCollectionAttribute();

        // Act
        var result = sut.IsValid(new[] { "item" });

        // Assert
        result.Should().BeTrue();
    }

    public static IEnumerable<object[]> InvalidCollections()
    {
        yield return [null];
        yield return [Array.Empty<string>()];
        yield return [123];
    }
}
