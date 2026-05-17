namespace Be.Haven.Tests.Shared.Be.Haven.Shared.Dtos.Responses;

public sealed class PaginationResponseTests
{
    [Fact]
    public void TotalPages_Should_CalculatePages_When_PageSizeIsPositive()
    {
        // Arrange
        var sut = new PaginationResponse<List<string>>(new List<string> { "a" }, 2, 10, 21);

        // Act
        var result = sut.TotalPages;

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public void TotalPages_Should_UseOneAsSafeDivisor_When_PageSizeIsZero()
    {
        // Arrange
        var sut = new PaginationResponse<List<string>>(new List<string> { "a" }, 1, 0, 3);

        // Act
        var result = sut.TotalPages;

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public void Constructor_Should_SetItemsAndTotal_When_DataAndTotalAreProvided()
    {
        // Arrange
        var items = new List<string> { "a", "b" };

        // Act
        var result = new PaginationResponse<List<string>>(items, 2);

        // Assert
        result.Items.Should().BeSameAs(items);
        result.Total.Should().Be(2);
    }

    [Fact]
    public void Constructor_Should_KeepDefaultValues_When_NoArgumentsAreProvided()
    {
        // Act
        var result = new PaginationResponse<List<string>>();

        // Assert
        result.Items.Should().BeNull();
        result.Total.Should().Be(0);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public void Constructor_Should_SetPageMetadata_When_PageArgumentsAreProvided()
    {
        // Arrange
        var items = new List<string> { "a" };

        // Act
        var result = new PaginationResponse<List<string>>(items, 4, 25);

        // Assert
        result.Items.Should().BeSameAs(items);
        result.PageNumber.Should().Be(4);
        result.PageSize.Should().Be(25);
    }

    [Fact]
    public void Constructor_Should_SetPageMetadataAndTotal_When_FullPaginationArgumentsAreProvided()
    {
        // Arrange
        var items = new List<string> { "a" };

        // Act
        var result = new PaginationResponse<List<string>>(items, 2, 10, 15);

        // Assert
        result.Items.Should().BeSameAs(items);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.Total.Should().Be(15);
    }

    [Fact]
    public void Constructor_Should_SetPageMetadataOnly_When_PageArgumentsAreProvided()
    {
        // Act
        var result = new PaginationResponse<List<string>>(3, 20);

        // Assert
        result.PageNumber.Should().Be(3);
        result.PageSize.Should().Be(20);
        result.Items.Should().BeNull();
    }
}
