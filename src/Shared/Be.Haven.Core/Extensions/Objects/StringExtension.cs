namespace Be.Haven.Core.Extensions.Objects;

public static class StringExtension
{
    /// <summary>
    /// Normalizes optional text by trimming it and treating blank values as missing.
    /// </summary>
    /// <param name="value">The source text value.</param>
    /// <returns>The trimmed value, or <c>null</c> when the source is empty.</returns>
    public static string NormalizeOptional(this string value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    /// <summary>
    /// Normalizes an email address for consistent cache keys, lookups, and persistence.
    /// </summary>
    /// <param name="email">The source email address.</param>
    /// <returns>The trimmed lowercase email address, or <c>null</c> when the source is empty.</returns>
    public static string NormalizeEmail(this string email)
    {
        return email.NormalizeOptional()?.ToLowerInvariant();
    }

    /// <summary>
    /// Normalizes a text code into an uppercase alphanumeric value.
    /// </summary>
    /// <param name="value">The source text code.</param>
    /// <returns>The uppercase alphanumeric code, or <c>null</c> when the source is empty.</returns>
    public static string NormalizeAlphanumericCode(this string value)
    {
        var normalizedValue = value.NormalizeOptional();

        return normalizedValue is null
            ? null
            : new string(normalizedValue.ToUpperInvariant().Where(char.IsLetterOrDigit).ToArray());
    }

    /// <summary>
    /// Removes the specified value from the end of the source string.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="value">The value to remove.</param>
    /// <returns>The modified string.</returns>
    public static string RemoveEndWith(this string source, string value)
    {
        if (!string.IsNullOrEmpty(source) && source.Length > 0 && source.EndsWith(value))
        {
            return source.Remove(source.Length - 1, 1).RemoveEndWith(value);
        }
        return source;
    }

    /// <summary>
    /// Removes the specified value from the start of the source string.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="value">The value to remove.</param>
    /// <returns>The modified string.</returns>
    public static string RemoveStartWith(this string source, string value)
    {
        if (!string.IsNullOrEmpty(source) && source.Length > 0 && source.StartsWith(value))
        {
            return source.Remove(0, 1).RemoveStartWith(value);
        }
        return source;
    }

    /// <summary>
    /// Converts the first character of the source string to lowercase.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <returns>The modified string.</returns>
    public static string FirstCharToLowerCase(this string source)
    {
        if (!string.IsNullOrEmpty(source) && char.IsUpper(source[0]))
        {
            return source.Length == 1
                ? char.ToLower(source[0]).ToString()
                : char.ToLower(source[0]) + source[1..];
        }
        return source;
    }

    /// <summary>
    /// Converts the first character of the source string to uppercase.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <returns>The modified string.</returns>
    public static string FirstCharToUpperCase(this string source)
    {
        if (!string.IsNullOrEmpty(source) && char.IsLower(source[0]))
        {
            return source.Length == 1
                ? char.ToUpper(source[0]).ToString()
                : char.ToUpper(source[0]) + source[1..];
        }
        return source;
    }

    /// <summary>
    /// Converts the source string to a non-Unicode string by replacing Unicode characters with their ASCII equivalents.
    /// </summary>
    /// <param name="text">The source string.</param>
    /// <returns>The modified string.</returns>
    public static string ToNonUnicode(this string text)
    {
        string[] arr1 = [ "á", "à", "ả", "ã", "ạ", "â", "ấ", "ầ", "ẩ", "ẫ", "ậ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ",
                                            "đ",
                                            "é","è","ẻ","ẽ","ẹ","ê","ế","ề","ể","ễ","ệ",
                                            "í","ì","ỉ","ĩ","ị",
                                            "ó","ò","ỏ","õ","ọ","ô","ố","ồ","ổ","ỗ","ộ","ơ","ớ","ờ","ở","ỡ","ợ",
                                            "ú","ù","ủ","ũ","ụ","ư","ứ","ừ","ử","ữ","ự",
                                            "ý","ỳ","ỷ","ỹ","ỵ",];

        string[] arr2 = [ "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a",
                                            "d",
                                            "e","e","e","e","e","e","e","e","e","e","e",
                                            "i","i","i","i","i",
                                            "o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o",
                                            "u","u","u","u","u","u","u","u","u","u","u",
                                            "y","y","y","y","y",];

        for (int i = 0; i < arr1.Length; i++)
        {
            text = text.Replace(arr1[i], arr2[i]);
            text = text.Replace(arr1[i].ToUpper(), arr2[i].ToUpper());
        }
        return text;
    }

    /// <summary>
    /// Removes the specified characters from the source string.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="charsToRemove">The characters to remove.</param>
    /// <returns>The string with specified characters removed.</returns>
    public static string RemoveCharacters(
        this string source,
        params char[] charsToRemove)
    {
        if (string.IsNullOrEmpty(source)
            || charsToRemove is null
            || charsToRemove.Length == 0)
        {
            return source;
        }
        foreach (var c in charsToRemove)
        {
            source = source.Replace(c.ToString(), "");
        }
        return source;
    }

    /// <summary>
    /// Retrieves a specific segment from a URL path based on the provided index.
    /// </summary>
    /// <param name="source">The source string representing the URL.</param>
    /// <param name="number">The index of the segment to retrieve.</param>
    /// <returns>The segment at the specified index from the URL path, or the original source string if it is null or empty.</returns>
    public static string GetSecretValue(
        this string source,
        int number)
    {
        if (string.IsNullOrEmpty(source))
        {
            return source;
        }

        Uri uri = new(source);
        return uri.Segments[number].Trim('/');
    }

    /// <summary>
    /// Checks if the source string matches the specified pattern.
    /// </summary>
    /// <param name="source">The source string to check.</param>
    /// <param name="pattern">The regular expression pattern to match against.</param>
    /// <returns>True if the source string matches the pattern; otherwise, false.</returns>
    public static bool IsMatchPattern(
        this string source,
        string pattern)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(pattern))
        {
            return false;
        }

        try
        {
            var regex = new Regex(pattern, RegexOptions.None, TimeSpan.FromSeconds(5));

            // Check if the source string matches the pattern
            return regex.IsMatch(source);

            // Use Regex to check if the source string matches the pattern
        }
        catch (ArgumentException)
        {
            // Handle invalid regular expression patterns
            return false;
        }
    }

    /// <summary>
    /// Generates a formatted prefix string by combining the service name 
    /// and the provided code using an underscore separator.
    /// </summary>
    /// <param name="serviceName">
    /// The name of the service or component generating the code prefix.
    /// </param>
    /// <param name="code">
    /// The unique identifier or extension code to append to the service name.
    /// </param>
    /// <returns>
    /// A combined string in the format <c>{serviceName}_{code}</c>.
    /// </returns>
    public static string GeneratePrefixExtension(string serviceName, string code)
    {
        return $"{serviceName}_{code}";
    }
}
