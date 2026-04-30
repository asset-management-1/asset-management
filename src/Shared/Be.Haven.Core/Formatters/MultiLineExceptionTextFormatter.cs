namespace Be.Haven.Core.Formatters;

/// <summary>
/// Custom text formatter that ensures:
/// - The first line is rendered with the full output template (including the message).
/// - Each stack trace line is rendered with the same prefix (timestamp, level,
///   CorrelationId, TraceId, SpanId, etc.) but without repeating the message.
/// This is useful when logs are split line-by-line (e.g., in GCP) and you want
/// consistent context on every line without duplicating the message.
/// </summary>
public sealed class MultiLineExceptionTextFormatter : ITextFormatter
{
    private readonly MessageTemplateTextFormatter _mainLineFormatter;
    private readonly MessageTemplateTextFormatter _stackLineFormatter;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiLineExceptionTextFormatter"/> class.
    /// </summary>
    /// <param name="outputTemplate">
    /// The Serilog output template used to render the main log line. It may contain
    /// {Message} / {Message:lj} and {Exception}; exception tokens will be ignored.
    /// </param>
    /// <param name="formatProvider">
    /// Optional format provider for formatting values. If not provided,
    /// <see cref="CultureInfo.InvariantCulture"/> is used.
    /// </param>

    public MultiLineExceptionTextFormatter(
        string outputTemplate,
        IFormatProvider formatProvider = null)
    {
        // 1) Base template: remove exception tokens so we only render prefix + message.
        var baseTemplate = outputTemplate.Replace(LOG_TOKEN_EXCEPTION, string.Empty);

        // 2) Template for stack trace lines: remove the message token as well.
        var stackTemplate = baseTemplate.Replace(LOG_TOKEN_MESSAGE, string.Empty);

        var provider = formatProvider ?? CultureInfo.InvariantCulture;

        _mainLineFormatter = new MessageTemplateTextFormatter(baseTemplate, provider);
        _stackLineFormatter = new MessageTemplateTextFormatter(stackTemplate, provider);
    }

    /// <summary>
    /// Formats the log event to the specified <see cref="TextWriter"/>.
    /// The first line includes the message; each stack trace line is prefixed
    /// without repeating the message.
    /// </summary>
    /// <param name="logEvent">The log event to format.</param>
    /// <param name="output">The output writer the log event is written to.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="logEvent"/> or <paramref name="output"/> is null.
    /// </exception>
    public void Format(LogEvent logEvent, TextWriter output)
    {
        if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));
        if (output == null) throw new ArgumentNullException(nameof(output));

        // 1) Render the main line with the full template (including the message).
        using var mainWriter = new StringWriter();
        _mainLineFormatter.Format(logEvent, mainWriter);
        var mainLine = LogMaskingHelper.MaskAllSensitiveData(mainWriter.ToString().TrimEnd('\r', '\n'));

        output.WriteLine(mainLine);

        // 2) If there is no exception, we are done.
        if (logEvent.Exception is null)
            return;

        // 3) Render the prefix for exception line (same prefix, but without the message).
        using var stackWriter = new StringWriter();
        _stackLineFormatter.Format(logEvent, stackWriter);
        var stackPrefix = stackWriter.ToString().TrimEnd('\r', '\n');

        // 4) Flatten exception into a single line.
        var oneLineException = logEvent.Exception.ToString()
                                       .Replace("\r\n", ONE_LINE_SEPARATOR)
                                       .Replace("\n", ONE_LINE_SEPARATOR)
                                       .Replace("\r", ONE_LINE_SEPARATOR);

        // 5) Write ONE exception line only.
        output.Write(stackPrefix);
        output.Write(" Exception=");
        output.WriteLine(oneLineException);
    }
}