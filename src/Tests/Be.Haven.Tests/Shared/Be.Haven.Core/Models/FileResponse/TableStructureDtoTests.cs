using Be.Haven.Core.Models.FileResponse;

namespace Be.Haven.Tests.Shared.Be.Haven.Core.Models.FileResponse;

public sealed class TableStructureDtoTests
{
    [Fact]
    public void Properties_Should_RoundTripValues_When_TableStructureIsPopulated()
    {
        // Arrange
        var headers = new[] { "Name", "Amount" };
        var rows = new List<string[]>
        {
            new[] { "Rent", "1000" }
        };

        // Act
        var result = new TableStructureDto
        {
            Headers = headers,
            Rows = rows,
            Title = "Payments"
        };

        // Assert
        result.Headers.Should().BeSameAs(headers);
        result.Rows.Should().BeSameAs(rows);
        result.Title.Should().Be("Payments");
    }
}
