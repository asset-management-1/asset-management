namespace Be.Haven.Core.Helpers;

public static class LogMaskingHelper
{
    /// <summary>
    /// Masks all sensitive data within the input text by processing JSON content,
    /// JSON-like blocks, and plain text. The function identifies sensitive patterns
    /// and replaces them with a masking string.
    /// </summary>
    /// <param name="input">The input string that potentially contains sensitive data to be masked.</param>
    /// <param name="customMask">An optional custom masking string to use when replacing sensitive data.
    /// If not provided, a default masking value is used.</param>
    /// <returns>The input string with sensitive data masked. If no sensitive data is detected,
    /// the original input string is returned unmodified.</returns>
    public static string MaskAllSensitiveData(string input, string customMask = null)
    {
        if (string.IsNullOrWhiteSpace(input) || !ContainsSensitiveKeyword(input))
            return input;

        var mask = string.IsNullOrWhiteSpace(customMask)
            ? DEFAULT_MASK
            : customMask;

        // 1. Full JSON
        if (LooksLikeJson(input))
        {
            var maskedJson = TryMaskFullJson(input, mask);
            if (!string.Equals(maskedJson, input, StringComparison.Ordinal))
                return maskedJson;
        }

        // 2. JSON blocks inside text
        var masked = MaskAllJsonBlocksInText(input, mask);

        // 3. Plain text fallback
        masked = MaskPlainText(masked, mask);

        return masked;
    }

    /// <summary>
    /// Attempts to mask all sensitive data within a JSON string by processing key-value pairs
    /// or nested structures. If the input string is a valid JSON object or array, a masked version
    /// of the JSON is returned. If the input is not valid JSON, the original string is returned unmodified.
    /// </summary>
    /// <param name="text">The input string to be checked and potentially masked if it contains JSON content.</param>
    /// <param name="mask">The masking string to substitute for sensitive data within the JSON.</param>
    /// <returns>The masked JSON string if the input was valid JSON and contained sensitive data, or the original string if masking was unnecessary or the input was not valid JSON.</returns>
    private static string TryMaskFullJson(string text, string mask)
    {
        if (!TryParseJson(text, out var token))
            return text;

        MaskJsonToken(token, mask);
        return token.ToString(Formatting.None);
    }

    /// <summary>
    /// Masks sensitive data within a JSON token by replacing values of sensitive keys or
    /// applying masking recursively to nested JSON structures. This method processes objects,
    /// arrays, and string values within the JSON token to ensure no sensitive data is exposed.
    /// </summary>
    /// <param name="token">The JSON token to be masked, representing an object, array, or value.</param>
    /// <param name="mask">The masking string used to replace sensitive data within the token.</param>
    private static void MaskJsonToken(JToken token, string mask)
    {
        switch (token)
        {
            case JObject obj:
                foreach (var property in obj.Properties().ToList())
                {
                    if (SENSITIVE_KEYS.Contains(property.Name))
                    {
                        property.Value = mask;
                        continue;
                    }

                    MaskJsonToken(property.Value, mask);
                }
                break;

            case JArray array:
                foreach (var item in array)
                {
                    MaskJsonToken(item, mask);
                }
                break;

            case JValue { Type: JTokenType.String } value:
                var str = value.Value?.ToString();
                if (string.IsNullOrWhiteSpace(str))
                    return;

                var maskedValue = MaskAllSensitiveData(str, mask);

                if (!string.Equals(maskedValue, str, StringComparison.Ordinal))
                {
                    value.Value = maskedValue;
                }
                break;
        }
    }

    /// <summary>
    /// Masks all JSON blocks within the provided text by replacing sensitive information
    /// inside JSON structures with the specified mask. The method identifies JSON blocks
    /// in the text, attempts to parse and process them as JSON, and applies masking to
    /// JSON tokens where applicable.
    /// </summary>
    /// <param name="text">The input text potentially containing JSON blocks to be masked.</param>
    /// <param name="mask">The masking string used to replace sensitive data in JSON tokens.</param>
    /// <returns>The text with all recognized JSON blocks having their sensitive data masked.</returns>
    private static string MaskAllJsonBlocksInText(string text, string mask)
    {
        var blocks = ExtractJsonBlocks(text);
        if (blocks.Count == 0)
            return text;

        var sb = new StringBuilder(text);

        for (var i = blocks.Count - 1; i >= 0; i--)
        {
            var block = blocks[i];
            var original = text.Substring(block.Start, block.Length);

            if (!LooksLikeJson(original))
                continue;

            if (!TryParseJson(original, out var token))
                continue;

            MaskJsonToken(token, mask);

            var masked = token.ToString(Formatting.None);

            if (!string.Equals(masked, original, StringComparison.Ordinal))
            {
                sb.Remove(block.Start, block.Length);
                sb.Insert(block.Start, masked);
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Extracts all JSON blocks from the provided text. A JSON block is identified as a complete
    /// structure enclosed within matching curly braces '{' and '}' or square brackets '[' and ']'.
    /// This method handles nested JSON structures and correctly captures blocks at the top level.
    /// </summary>
    /// <param name="text">The textual input from which JSON blocks are to be extracted.</param>
    /// <returns>A list of tuples, where each tuple contains the starting index and the length of a JSON block within the text.</returns>
    private static List<(int Start, int Length)> ExtractJsonBlocks(string text)
    {
        var results = new List<(int Start, int Length)>();
        var stack = new Stack<(char Bracket, int Index)>();

        var inString = false;
        var stringDelimiter = '\0';
        var escape = false;

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];

            if (inString)
            {
                if (escape)
                {
                    escape = false;
                    continue;
                }

                if (c == '\\')
                {
                    escape = true;
                    continue;
                }

                if (c == stringDelimiter)
                {
                    inString = false;
                }

                continue;
            }

            switch (c)
            {
                case '"':
                case '\'':
                    inString = true;
                    stringDelimiter = c;
                    continue;
                case '{':
                case '[':
                    stack.Push((c, i));
                    continue;
                case '}':
                case ']':
                {
                    if (stack.Count == 0)
                        continue;

                    var open = stack.Pop();
                    if (!IsMatching(open.Bracket, c))
                        continue;

                    if (stack.Count == 0)
                    {
                        var start = open.Index;
                        var length = i - start + 1;
                        results.Add((start, length));
                    }

                    break;
                }
            }
        }

        return results;
    }

    /// <summary>
    /// Determines if the provided opening and closing characters form a matching pair of brackets or braces.
    /// This method checks specifically for '{' with '}' and '[' with ']' pairings.
    /// </summary>
    /// <param name="open">The opening character to evaluate (either '{' or '[').</param>
    /// <param name="close">The closing character to evaluate (either '}' or ']').</param>
    /// <returns>True if the characters form a valid matching pair of brackets or braces; otherwise, false.</returns>
    private static bool IsMatching(char open, char close)
    {
        return (open == '{' && close == '}') || (open == '[' && close == ']');
    }

    /// <summary>
    /// Determines whether the input string appears to be a JSON object or array based on its structure.
    /// This method checks for specific JSON-like patterns, such as leading and trailing braces or brackets.
    /// </summary>
    /// <param name="input">The input string to evaluate.</param>
    /// <returns>True if the input string resembles a JSON object or array; otherwise, false.</returns>
    private static bool LooksLikeJson(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        input = input.Trim();

        if ((input.StartsWith('{') && input.EndsWith('}')) ||
            (input.StartsWith('[') && input.EndsWith(']')))
        {
            return input.Contains(':') || input.StartsWith('[');
        }

        return false;
    }

    /// <summary>
    /// Attempts to parse the input string as a JSON object or array. If successful, the parsed JSON token is returned as an output parameter.
    /// </summary>
    /// <param name="input">The input string to be parsed as JSON.</param>
    /// <param name="token">When this method returns, contains the parsed JSON token if the parsing was successful, or null if parsing failed.</param>
    /// <returns>True if the input string was successfully parsed as JSON; otherwise, false.</returns>
    private static bool TryParseJson(string input, out JToken token)
    {
        token = null;

        try
        {
            token = JToken.Parse(input);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Masks sensitive plain text patterns, such as passwords or secrets, in the given text using the specified mask.
    /// </summary>
    /// <param name="text">The input text to be inspected and masked if sensitive data is detected.</param>
    /// <param name="mask">The mask value to replace sensitive data. If null or empty, a default mask will be used.</param>
    /// <returns>A string with sensitive plain text patterns replaced by the specified mask.</returns>
    private static string MaskPlainText(string text, string mask)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        return PLAIN_TEXT_REGEX.Replace(text, match =>
        {
            var key = match.Groups[1].Value;
            var separator = match.Groups[2].Value;
            var quote = match.Groups[3].Value;

            return $"{key}{separator}{quote}{mask}{quote}";
        });
    }

    /// <summary>
    /// Determines whether the input string contains any sensitive keywords from the predefined set of sensitive keys.
    /// </summary>
    /// <param name="input">The input string to be evaluated for sensitive keywords.</param>
    /// <returns>True if the input contains sensitive keywords; otherwise, false.</returns>
    private static bool ContainsSensitiveKeyword(string input)
    {
        return !string.IsNullOrWhiteSpace(input) && SENSITIVE_KEYS.Any(key => input.Contains(key, StringComparison.OrdinalIgnoreCase));
    }
}