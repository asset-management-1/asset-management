namespace Be.Haven.Core.Models.CodeGeneration;

/// <summary>
/// Groups the inputs required to generate and validate a unique business-readable code.
/// </summary>
public sealed class UniqueCodeGenerationOptions
{
    /// <summary>
    /// Gets or sets the business prefix placed before the numeric random segment.
    /// </summary>
    public string Prefix { get; set; }

    /// <summary>
    /// Gets or sets the initial numeric random segment length.
    /// </summary>
    public int RandomLength { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of collision retry attempts per random segment length.
    /// </summary>
    public int MaxAttempts { get; set; }

    /// <summary>
    /// Gets or sets the callback that checks whether a generated code already exists.
    /// </summary>
    public Func<string, CancellationToken, Task<bool>> ExistsAsync { get; set; }

    /// <summary>
    /// Gets or sets the separator between prefix and numeric random segment.
    /// </summary>
    public string Separator { get; set; } = DEFAULT_CODE_SEPARATOR;

    /// <summary>
    /// Gets or sets the optional exception factory used when all segment lengths collide.
    /// </summary>
    public Func<Exception> ExhaustionExceptionFactory { get; set; }
}
