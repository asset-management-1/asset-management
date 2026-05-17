namespace Be.Haven.Tests.Shared.Be.Haven.Core.Extensions.Objects;

public sealed class QueryableExtensionTests
{
    [Fact]
    public void Pagination_Should_SkipAndTake_When_PageSizeIsPositive()
    {
        // Arrange
        var query = Enumerable.Range(1, 5).AsQueryable();

        // Act
        var result = query.Pagination(2, 2).ToList();

        // Assert
        result.Should().Equal(3, 4);
    }

    [Fact]
    public void Pagination_Should_ReturnOriginalQuery_When_PageSizeIsZero()
    {
        // Arrange
        var query = Enumerable.Range(1, 3).AsQueryable();

        // Act
        var result = query.Pagination(0, 2).ToList();

        // Assert
        result.Should().Equal(1, 2, 3);
    }

    [Fact]
    public void OrderBy_Should_ApplyMultipleSorts_When_OrderByContainsTwoFields()
    {
        // Arrange
        var query = CreateQuerySamples().AsQueryable();

        // Act
        var result = query.OrderBy("Name ASC, Count DESC").Select(x => x.Id).ToList();

        // Assert
        result.Should().Equal(2, 3, 1);
    }

    [Fact]
    public void OrderBy_Should_ThrowArgumentException_When_OrderByItemHasTooManySegments()
    {
        // Arrange
        var query = CreateQuerySamples().AsQueryable();

        // Act
        var act = () => query.OrderBy("Name ASC DESC").ToList();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Invalid OrderBy string 'Name ASC DESC'.*");
    }

    [Fact]
    public void OrderBy_Should_ReturnOriginalQuery_When_OrderByIsEmpty()
    {
        // Arrange
        var query = CreateQuerySamples().AsQueryable();

        // Act
        var result = query.OrderBy(string.Empty).Select(x => x.Id).ToList();

        // Assert
        result.Should().Equal(1, 2, 3);
    }

    [Fact]
    public void OrderBy_Should_ThrowArgumentException_When_PropertyNameIsEmpty()
    {
        // Arrange
        var query = CreateQuerySamples().AsQueryable();

        // Act
        var action = () => query.OrderBy(" ").ToList();

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage(ERROR_INVALID_PROPERTY);
    }

    [Fact]
    public void Filter_Should_ApplyStringContainsAndExactValueFilters_When_RequestHasValues()
    {
        // Arrange
        var query = CreateQuerySamples().AsQueryable();
        var request = new CoreSampleFilterModel
        {
            Name = "Ann",
            Count = 2
        };

        // Act
        var result = query.Filter(request).ToList();

        // Assert
        result.Should().ContainSingle();
        result[0].Id.Should().Be(2);
    }

    [Fact]
    public void Filter_Should_ReturnOriginalQuery_When_RequestHasNoMatchingValues()
    {
        // Arrange
        var query = CreateQuerySamples().AsQueryable();
        var request = new CoreSampleFilterModel();

        // Act
        var result = query.Filter(request).ToList();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public void SearchTerm_Should_SearchAllStringProperties_When_SearchPropsAreEmpty()
    {
        // Arrange
        var query = CreateQuerySamples().AsQueryable();

        // Act
        var result = query.SearchTerm("ann", []).Select(x => x.Id).ToList();

        // Assert
        result.Should().Equal(2, 3);
    }

    [Fact]
    public void SearchTerm_Should_SearchOnlyConfiguredProperties_When_SearchPropsAreProvided()
    {
        // Arrange
        var query = CreateQuerySamples().AsQueryable();
        var searchProps = new List<SearchFieldConfiguration>
        {
            new(nameof(CoreSampleQueryModel.Name))
        };

        // Act
        var result = query.SearchTerm("bob", searchProps).Select(x => x.Id).ToList();

        // Assert
        result.Should().Equal(1);
    }

    [Fact]
    public void SearchTerm_Should_SearchUnicodeAndNonUnicodeVariants_When_SearchTermHasAccents()
    {
        // Arrange
        var query = new List<CoreSampleQueryModel>
        {
            new()
            {
                Id = 1,
                Name = "Dang Van B",
                Count = 1
            }
        }.AsQueryable();

        // Act
        var result = query.SearchTerm("\u0111\u1eb7ng", []).Select(x => x.Id).ToList();

        // Assert
        result.Should().Equal(1);
    }

    private static List<CoreSampleQueryModel> CreateQuerySamples()
    {
        return
        [
            new()
            {
                Id = 1,
                Name = "Bob",
                Count = 1,
                Child = new CoreSampleChildModel
                {
                    Name = "North"
                }
            },
            new()
            {
                Id = 2,
                Name = "Ann",
                Count = 2,
                Child = new CoreSampleChildModel
                {
                    Name = "South"
                }
            },
            new()
            {
                Id = 3,
                Name = "Annabelle",
                Count = 1,
                Child = new CoreSampleChildModel
                {
                    Name = "East"
                }
            }
        ];
    }
}
