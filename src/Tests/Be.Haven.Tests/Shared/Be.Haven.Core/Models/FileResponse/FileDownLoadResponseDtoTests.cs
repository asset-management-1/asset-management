using Be.Haven.Core.Models.FileResponse;

namespace Be.Haven.Tests.Shared.Be.Haven.Core.Models.FileResponse;

public sealed class FileDownLoadResponseDtoTests
{
    [Fact]
    public void Constructor_Should_AssignFileResponseValues_When_ValuesAreProvided()
    {
        // Arrange
        var content = Encoding.UTF8.GetBytes("file-content");

        // Act
        var result = new FileDownLoadResponseDto(content, "report.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        // Assert
        result.Content.Should().BeSameAs(content);
        result.FileName.Should().Be("report.xlsx");
        result.ContentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }
}
