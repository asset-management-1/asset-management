namespace Haven.Shared.Dtos;

public class MetaDetailDto
{
    /// <summary>
    /// The unique identifier for the request.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string RequestId { get; set; }

    /// <summary>
    /// The correlation ID used to track requests across services.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string CorrelationId { get; set; }

    /// <summary>
    /// The trace ID for distributed tracing.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string TraceId { get; set; }

    /// <summary>
    /// The span ID for distributed tracing.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string SpanId { get; set; }

    /// <summary>
    /// The version of the response or API.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Version { get; set; }

    /// <summary>
    /// The timestamp when the request was received.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? RequestTimestamp { get; set; }

    /// <summary>
    /// The timestamp when the response was sent.
    /// </summary>
    public DateTime ResponseTimestamp { get; set; } = DateTime.UtcNow;
}