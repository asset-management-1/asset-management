namespace Be.Haven.ApiCommon.Routing;

public sealed class SlugifyTransformer : IOutboundParameterTransformer
{
    // ---- Compiled regexes ----
    private static readonly Regex AcronymBoundary = new(ACRONYM_BOUNDARY_PATTERN, REGEX_OPTS, REGEX_TIMEOUT);

    private static readonly Regex LowerUpperBoundary = new(LOWER_UPPER_BOUNDARY_PATTERN, REGEX_OPTS, REGEX_TIMEOUT);

    /// <summary>
    /// Converts an outbound route token to kebab-case URL segment.
    /// </summary>
    /// <param name="value">Outbound value (controller/action name).</param>
    /// <returns>
    /// Kebab-cased string; empty string if <paramref name="value"/> is <c>null</c>.
    /// </returns>
    public string TransformOutbound(object value)
    {
        if (value is null) return null;
        var s = value.ToString();
        if (string.IsNullOrEmpty(s)) return s;

        // 1) APIKey -> API-Key (keep acronym as one block)
        s = AcronymBoundary.Replace(s, HYPHEN_REPLACEMENT);

        // 2) userID -> user-ID, FooBar -> Foo-Bar
        s = LowerUpperBoundary.Replace(s, HYPHEN_REPLACEMENT);

        // 3) Lower-case for URL
        return s.ToLowerInvariant();
    }
}