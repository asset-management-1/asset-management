namespace Be.Haven.Tests.Shared.Be.Haven.Core.Formatters;

public sealed class MultiLineExceptionTextFormatterTests
{
    [Fact]
    public void Format_Should_ThrowArgumentNullException_When_LogEventIsNull()
    {
        // Arrange
        var sut = new MultiLineExceptionTextFormatter("[{Level}] {Message} {Exception}");
        using var output = new StringWriter();

        // Act
        var action = () => sut.Format(null, output);

        // Assert
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("logEvent");
    }

    [Fact]
    public void Format_Should_ThrowArgumentNullException_When_OutputIsNull()
    {
        // Arrange
        var sut = new MultiLineExceptionTextFormatter("[{Level}] {Message} {Exception}");
        var logEvent = new LogEvent(
            new DateTimeOffset(2026, 5, 17, 0, 0, 0, TimeSpan.Zero),
            LogEventLevel.Information,
            null,
            new MessageTemplateParser().Parse("Hello"),
            []);

        // Act
        var action = () => sut.Format(logEvent, null);

        // Assert
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("output");
    }

    [Fact]
    public void Format_Should_WriteMaskedMainLineAndSingleExceptionLine_When_ExceptionContainsNewLines()
    {
        // Arrange
        var sut = new MultiLineExceptionTextFormatter("[{Level:u3}] {Message:lj}{Exception}");
        var exception = new InvalidOperationException("first line\nsecond line");
        var logEvent = new LogEvent(
            new DateTimeOffset(2026, 5, 17, 0, 0, 0, TimeSpan.Zero),
            LogEventLevel.Error,
            exception,
            new MessageTemplateParser().Parse("Login failed password=super-secret"),
            []);
        using var output = new StringWriter(CultureInfo.InvariantCulture);

        // Act
        sut.Format(logEvent, output);

        // Assert
        var lines = output.ToString()
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        lines.Should().HaveCount(2);
        lines[0].Should().Contain("Login failed password=******");
        lines[1].Should().Contain("Exception=System.InvalidOperationException: first line | second line");
    }

    [Fact]
    public void Format_Should_WriteOnlyMainLine_When_LogEventHasNoException()
    {
        // Arrange
        var sut = new MultiLineExceptionTextFormatter("[{Level:u3}] {Message:lj}{Exception}");
        var logEvent = new LogEvent(
            new DateTimeOffset(2026, 5, 17, 0, 0, 0, TimeSpan.Zero),
            LogEventLevel.Information,
            null,
            new MessageTemplateParser().Parse("Healthy"),
            []);
        using var output = new StringWriter(CultureInfo.InvariantCulture);

        // Act
        sut.Format(logEvent, output);

        // Assert
        var lines = output.ToString()
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        lines.Should().ContainSingle()
            .Which.Should().Contain("Healthy");
    }
}
