namespace Be.Haven.Tests.Shared.Be.Haven.Shared.Dtos.Responses;

public sealed class ODataPaginationResponseTests
{
    [Fact]
    public void Constructor_Should_KeepDefaultValues_When_NoArgumentsAreProvided()
    {
        // Act
        var result = new ODataPaginationResponse<List<string>>();

        // Assert
        result.Items.Should().BeNull();
        result.Total.Should().Be(0);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.PreviousLinkId.Should().BeNull();
        result.NextLinkId.Should().BeNull();
    }

    [Fact]
    public void Constructor_Should_SetItemsAndTotal_When_DataAndTotalAreProvided()
    {
        // Arrange
        var items = new List<string> { "a", "b" };

        // Act
        var result = new ODataPaginationResponse<List<string>>(items, 2);

        // Assert
        result.Items.Should().BeSameAs(items);
        result.Total.Should().Be(2);
    }

    [Fact]
    public void Constructor_Should_SetPageMetadataAndTotal_When_FullPaginationArgumentsAreProvided()
    {
        // Arrange
        var items = new List<string> { "a" };

        // Act
        var result = new ODataPaginationResponse<List<string>>(items, 3, 25, 80);

        // Assert
        result.Items.Should().BeSameAs(items);
        result.PageNumber.Should().Be(3);
        result.PageSize.Should().Be(25);
        result.Total.Should().Be(80);
    }

    [Fact]
    public void Constructor_Should_SetItemsAndPageMetadata_When_DataAndPageArgumentsAreProvided()
    {
        // Arrange
        var items = new List<string> { "a", "b" };

        // Act
        var result = new ODataPaginationResponse<List<string>>(items, 4, 50);

        // Assert
        result.Items.Should().BeSameAs(items);
        result.PageNumber.Should().Be(4);
        result.PageSize.Should().Be(50);
    }

    [Fact]
    public void Constructor_Should_SetPageMetadataOnly_When_PageArgumentsAreProvided()
    {
        // Act
        var result = new ODataPaginationResponse<List<string>>(5, 15);

        // Assert
        result.PageNumber.Should().Be(5);
        result.PageSize.Should().Be(15);
        result.Items.Should().BeNull();
    }

    [Fact]
    public void Constructor_Should_SetPageLinks_When_LinkArgumentsAreProvided()
    {
        // Arrange
        var items = new List<string> { "a" };

        // Act
        var result = new ODataPaginationResponse<List<string>>(
            items,
            2,
            10,
            "previous-id",
            "next-id");

        // Assert
        result.Items.Should().BeSameAs(items);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.PreviousLinkId.Should().Be("previous-id");
        result.NextLinkId.Should().Be("next-id");
    }
}
