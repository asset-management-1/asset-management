namespace Haven.Core.Helpers;

public static class StringConvertHelper
{
    private const string Prefix = GOOGLE_STORAGE_PREFIX;

    /// <summary>
    /// Tries to convert the input string <paramref name="s"/> to a value of type <typeparamref name="T"/>.
    /// Handles common primitives, Guid, DateTime/Offset, enums (name or numeric),
    /// string identity, and finally falls back to JSON deserialization.
    /// </summary>
    public static bool TryConvert<T>(string s, out T value, IFormatProvider provider = null)
    {
        provider ??= CultureInfo.InvariantCulture;
        value = default;

        if (string.IsNullOrWhiteSpace(s))
            return false;

        var target = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

        // Fast path: string identity
        if (target == typeof(string))
        {
            value = (T)(object)s;
            return true;
        }

        try
        {
            // Enums: accept numeric ("1") or name ("Pending"), case-insensitive
            if (target.IsEnum)
            {
                if (int.TryParse(s, NumberStyles.Integer, provider, out var enumNum))
                {
                    value = (T)Enum.ToObject(target, enumNum);
                    return true;
                }

                if (Enum.TryParse(target, s, ignoreCase: true, out var enumVal))
                {
                    value = (T)enumVal;
                    return true;
                }

                return false;
            }

            // Built-in parsers mapped by target type to keep branching low
            var parsers = new Dictionary<Type, Func<string, IFormatProvider, (bool ok, object val)>>
            {
                [typeof(Guid)] = (txt, _) =>
                    (Guid.TryParse(txt, out var g), g),

                [typeof(DateTime)] = (txt, fmt) =>
                    (DateTime.TryParse(txt, fmt,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt), dt),

                [typeof(DateTimeOffset)] = (txt, fmt) =>
                    (DateTimeOffset.TryParse(txt, fmt,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dto), dto),

                [typeof(int)] = (txt, fmt) =>
                    (int.TryParse(txt, NumberStyles.Integer, fmt, out var v), v),

                [typeof(long)] = (txt, fmt) =>
                    (long.TryParse(txt, NumberStyles.Integer, fmt, out var v), v),

                [typeof(decimal)] = (txt, fmt) =>
                    (decimal.TryParse(txt, NumberStyles.Number, fmt, out var v), v),

                [typeof(double)] = (txt, fmt) =>
                    (double.TryParse(txt, NumberStyles.Float | NumberStyles.AllowThousands, fmt, out var v), v),

                [typeof(bool)] = (txt, _) =>
                    (bool.TryParse(txt, out var v), v),
            };

            if (parsers.TryGetValue(target, out var parseFn))
            {
                var (ok, val) = parseFn(s, provider);
                if (!ok) return false;

                value = (T)val;
                return true;
            }

            // Fallback: JSON (e.g., complex objects, arrays, other primitives not listed)
            var obj = JsonConvert.DeserializeObject(s, target, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            });
            if (obj is null) return false;

            value = (T)obj;
            return true;
        }
        catch
        {
            // Keep existing behaviour: never throw from Try*; signal failure instead.
            return false;
        }
    }

    /// <summary>
    /// Removes the Google Cloud Storage base URL prefix and returns only the object path.
    /// </summary>
    /// <param name="url">The full signed URL returned from GCP.</param>
    /// <returns>The trimmed URL starting after the GCS prefix.</returns>
    public static string TrimGoogleStoragePrefix(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return url;

        return url.Replace(Prefix, string.Empty);
    }
    
    /// <summary>
    /// Truncates the given string if its length exceeds the specified maximum length.
    /// </summary>
    /// <param name="s">The string to be truncated.</param>
    /// <param name="max">The maximum length allowed for the string.</param>
    /// <returns>The original string if its length is within the limit; otherwise, a truncated version of the string.</returns>
    public static string Truncate(string s, int max)
    {
        if (string.IsNullOrEmpty(s))
            return string.Empty;

        return s.Length <= max ? s : s[..max];
    }

    /// <summary>
    /// Converts the given string <paramref name="input"/> to kebab-case format.
    /// Replaces uppercase letters with their lowercase equivalents, preceded by a hyphen if they are not at the start.
    /// </summary>
    /// <param name="input">The string to be converted to kebab-case format.</param>
    /// <returns>The input string formatted in kebab-case.</returns>
    public static string ToKebabCase(string input) => Regex.Replace(input, "([a-z0-9])([A-Z])", "$1-$2")
                                                           .ToLowerInvariant();

    /// <summary>
    /// Converts a kebab-case string to camelCase.
    /// </summary>
    /// <param name="kebab">The kebab-case string to be converted.</param>
    /// <returns>A camelCase version of the input string.</returns>
    public static string ToCamelCase(string kebab) => Regex.Replace(kebab, "-([a-zA-Z0-9])", m => m.Groups[1].Value.ToUpperInvariant());
}