using Be.Haven.Core.Files;
using Be.Haven.Core.Interfaces.Excel;

namespace Be.Haven.Tests.Shared.Be.Haven.Core.Files;

public sealed class ExcelReportGeneratorBaseTests
{
    [Fact]
    public async Task GenerateAsync_Should_OpenConfigureFillBuildAndDisposeDocument_When_ReportIsGenerated()
    {
        // Arrange
        var expected = Encoding.UTF8.GetBytes("excel-bytes");
        var document = new Mock<IExcelDocument>();
        document.Setup(x => x.Build()).Returns(expected);

        var factory = new Mock<IExcelDocumentFactory>();
        factory.Setup(x => x.OpenAsync("sample-report", It.IsAny<CancellationToken>()))
               .ReturnsAsync(document.Object);

        var sut = new SampleExcelReportGenerator(factory.Object);

        // Act
        var result = await sut.GenerateAsync("model-value", CancellationToken.None);

        // Assert
        result.Should().BeSameAs(expected);
        factory.Verify(x => x.OpenAsync("sample-report", CancellationToken.None), Times.Once);
        document.Verify(x => x.Set("configured", "model-value", null), Times.Once);
        document.Verify(x => x.Set("filled", "model-value", null), Times.Once);
        document.Verify(x => x.Build(), Times.Once);
        document.Verify(x => x.Dispose(), Times.Once);
    }

    [Fact]
    public async Task GenerateAsync_Should_UseDefaultConfigure_When_GeneratorDoesNotOverrideConfigure()
    {
        // Arrange
        var expected = Encoding.UTF8.GetBytes("excel-bytes");
        var document = new Mock<IExcelDocument>();
        document.Setup(x => x.Build()).Returns(expected);

        var factory = new Mock<IExcelDocumentFactory>();
        factory.Setup(x => x.OpenAsync("default-configure-report", It.IsAny<CancellationToken>()))
               .ReturnsAsync(document.Object);

        var sut = new DefaultConfigureExcelReportGenerator(factory.Object);

        // Act
        var result = await sut.GenerateAsync("model-value", CancellationToken.None);

        // Assert
        result.Should().BeSameAs(expected);
        document.Verify(x => x.Set("filled", "model-value", null), Times.Once);
        document.Verify(x => x.Set("configured", It.IsAny<string>(), null), Times.Never);
    }

    private sealed class SampleExcelReportGenerator : ExcelReportGeneratorBase<string>
    {
        public SampleExcelReportGenerator(IExcelDocumentFactory factory) : base(factory)
        {
        }

        public override string ReportKey => "sample-report";

        protected override void Configure(IExcelDocument doc, string model)
        {
            doc.Set("configured", model);
        }

        protected override void Fill(IExcelDocument doc, string model)
        {
            doc.Set("filled", model);
        }
    }

    private sealed class DefaultConfigureExcelReportGenerator : ExcelReportGeneratorBase<string>
    {
        public DefaultConfigureExcelReportGenerator(IExcelDocumentFactory factory) : base(factory)
        {
        }

        public override string ReportKey => "default-configure-report";

        protected override void Fill(IExcelDocument doc, string model)
        {
            doc.Set("filled", model);
        }
    }
}
