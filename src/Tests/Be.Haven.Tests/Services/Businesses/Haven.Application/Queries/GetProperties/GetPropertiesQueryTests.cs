using Haven.Application.Queries.GetProperties;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Queries.GetProperties;

public sealed class GetPropertiesQueryTests
{
    [Fact]
    public void SearchAndCodeFilters_Should_PreserveRawInput_When_Set()
    {
        // Arrange
        var query = new GetPropertiesQuery
        {
            Search = "  toa nha  ",
            PropertyTypeCode = " BUILDING ",
            StatusCode = " DRAFT ",
            RoomStatusCode = " AVAILABLE ",
            PaymentStatusCode = " UNPAID "
        };

        // Assert
        query.Search.Should().Be("  toa nha  ");
        query.PropertyTypeCode.Should().Be(" BUILDING ");
        query.StatusCode.Should().Be(" DRAFT ");
        query.RoomStatusCode.Should().Be(" AVAILABLE ");
        query.PaymentStatusCode.Should().Be(" UNPAID ");
    }

}
