namespace Be.Haven.Tests.Shared.Be.Haven.Core.Helpers;

public sealed class ExcelGeneratorHelperTests
{
    [Fact]
    public void ToFormFile_Should_CreateFormFileWithMetadata_When_ContentIsProvided()
    {
        // Arrange
        var content = Encoding.UTF8.GetBytes("excel-content");

        // Act
        var result = ExcelGeneratorHelper.ToFormFile(content, "report.xlsx");

        // Assert
        result.FileName.Should().Be("report.xlsx");
        result.Name.Should().Be("file");
        result.Length.Should().Be(content.Length);
        result.ContentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }
}
