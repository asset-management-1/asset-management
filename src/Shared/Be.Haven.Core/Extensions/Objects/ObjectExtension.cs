namespace Be.Haven.Core.Extensions.Objects;

/// <summary>
/// Provides extension methods for objects.
/// </summary>
public static class ObjectExtension
{
    /// <summary>
    /// Converts an object to a dictionary with optional prefix and suffix for keys.
    /// </summary>
    /// <param name="source">The source object to convert.</param>
    /// <param name="prefix">The optional prefix to add to each key.</param>
    /// <param name="suffix">The optional suffix to add to each key.</param>
    /// <param name="bindingAttr">The binding attributes to use when reflecting on the object.</param>
    /// <returns>A dictionary with the object's properties as keys and their values as values.</returns>
    public static IDictionary<string, object> AsDictionary(
        this object source,
        string prefix = null,
        string suffix = null,
        BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance)
    {
        return source.GetType().GetProperties(bindingAttr).ToDictionary
        (
            propInfo => $"{prefix}{propInfo.Name}{suffix}",
            propInfo => propInfo.GetValue(source, null)
        );
    }

    /// <summary>
    /// Sanitizes the values in a dictionary by processing specific object types like files or collections of files.
    /// </summary>
    /// <param name="args">The input dictionary with string keys and object values to be sanitized.</param>
    /// <returns>
    /// A sanitized object. If the dictionary contains one item, the sanitized value of that item is returned.
    /// If the dictionary contains multiple items, a new dictionary with sanitized values is returned.
    /// If the dictionary is null or empty, null is returned.
    /// </returns>
    public static object SanitizeArgs(this IDictionary<string, object> args)
    {
        if (args is null || args.Count == 0) return null;
        if (args.Count == 1) return Sanitize(args.First().Value);

        var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        foreach (var kv in args)
            dict[kv.Key] = Sanitize(kv.Value);
        return dict;
    }

    /// <summary>
    /// Sanitizes a given value by handling specific types such as single file objects or collections of files.
    /// </summary>
    /// <param name="value">The value to be sanitized, which can include types such as IFormFile, collections of IFormFile, or other objects.</param>
    /// <returns>
    /// A sanitized object. For IFormFile, an object with file metadata is returned.
    /// For collections of IFormFile, an array of sanitized file metadata is returned.
    /// For other objects, the input value is returned as is.
    /// </returns>
    private static object Sanitize(object value)
    {
        switch (value)
        {
            case null:
                return null;

            case IFormFile f:
                return new { f.FileName, f.ContentType, f.Length };

            case IEnumerable<IFormFile> files:
                return files.Select(f => new { f.FileName, f.ContentType, f.Length }).ToArray();

            default:
                return value;
        }
    }
}
