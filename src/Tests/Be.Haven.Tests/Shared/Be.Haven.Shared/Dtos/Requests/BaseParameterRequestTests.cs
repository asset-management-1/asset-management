namespace Be.Haven.Tests.Shared.Be.Haven.Shared.Dtos.Requests;

public sealed class BaseParameterRequestTests
{
    [Fact]
    public void Constructor_Should_SetDefaultPagination_When_NoArgumentsAreProvided()
    {
        // Act
        var result = new BaseParameterRequest();

        // Assert
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public void Constructor_Should_SetPagination_When_ArgumentsAreProvided()
    {
        // Act
        var result = new BaseParameterRequest(3, 25);

        // Assert
        result.PageNumber.Should().Be(3);
        result.PageSize.Should().Be(25);
    }

    [Fact]
    public void SearchProps_Should_RoundTripValues_When_SetThroughMethods()
    {
        // Arrange
        var sut = new BaseParameterRequest();
        var searchProps = new List<SearchFieldConfiguration>
        {
            new("Name", SearchFieldType.String)
        };

        // Act
        sut.SetSearchProps(searchProps);
        var result = sut.GetSearchProps();

        // Assert
        result.Should().BeSameAs(searchProps);
    }
}
