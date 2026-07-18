namespace Be.Haven.Core.Helpers;

/// <summary>
/// Provides reusable helpers for business-readable code generation.
/// </summary>
public static class CodeGenerationHelper
{
    /// <summary>
    /// Generates a cryptographically secure numeric code with exactly the requested length.
    /// </summary>
    /// <param name="length">The required number of digits.</param>
    /// <returns>The generated zero-preserving numeric code.</returns>
    public static string GenerateNumericCode(int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, MAX_NUMERIC_RANDOM_CODE_LENGTH);

        // Build one digit at a time so leading zeroes remain part of the generated code.
        var builder = new StringBuilder(length);
        for (var index = 0; index < length; index++)
        {
            builder.Append(RandomNumberGenerator.GetInt32(0, 10).ToString(CultureInfo.InvariantCulture));
        }

        return builder.ToString();
    }

    /// <summary>
    /// Generates a formatted code from a prefix and numeric random segment.
    /// </summary>
    /// <param name="prefix">The business prefix to put before the numeric segment.</param>
    /// <param name="randomLength">The required numeric segment length.</param>
    /// <param name="separator">The separator between prefix and random segment.</param>
    /// <returns>The generated code.</returns>
    public static string GenerateCode(
        string prefix,
        int randomLength,
        string separator = DEFAULT_CODE_SEPARATOR)
    {
        // Validate the dynamic generation inputs before building a public business code.
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(randomLength);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(randomLength, MAX_NUMERIC_RANDOM_CODE_LENGTH);

        var normalizedSeparator = separator ?? string.Empty;
        var randomSegment = GenerateNumericCode(randomLength);

        return $"{prefix.Trim()}{normalizedSeparator}{randomSegment}";
    }

    /// <summary>
    /// Generates a formatted unique code by retrying until the supplied existence check returns false.
    /// </summary>
    /// <param name="options">The unique-code generation options.</param>
    /// <param name="cancellationToken">The token used to cancel uniqueness checks.</param>
    /// <returns>The first generated code that does not exist.</returns>
    public static async Task<string> GenerateUniqueCodeAsync(
        UniqueCodeGenerationOptions options,
        CancellationToken cancellationToken = default)
    {
        // Validate retry inputs once so callers get clear failures before any repository call.
        ArgumentNullException.ThrowIfNull(options);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(options.MaxAttempts);
        ArgumentNullException.ThrowIfNull(options.ExistsAsync);

        var randomLength = options.RandomLength;
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(randomLength);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(randomLength, MAX_NUMERIC_RANDOM_CODE_LENGTH);

        while (randomLength <= MAX_NUMERIC_RANDOM_CODE_LENGTH)
        {
            for (var attempt = 0; attempt < options.MaxAttempts; attempt++)
            {
                // Generate a candidate and ask the owning repository whether that code is already taken.
                var code = GenerateCode(
                    options.Prefix,
                    randomLength,
                    options.Separator);
                var exists = await options.ExistsAsync(code, cancellationToken);

                if (!exists)
                {
                    return code;
                }
            }

            // When a short random segment is too crowded, widen the segment and continue.
            randomLength++;
        }

        // Only fail after every GUID-backed random segment length has been exhausted.
        throw options.ExhaustionExceptionFactory?.Invoke()
              ?? new InvalidOperationException(UNIQUE_CODE_GENERATION_FAILED);
    }

}
