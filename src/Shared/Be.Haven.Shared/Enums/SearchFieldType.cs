namespace Be.Haven.Shared.Enums;

/// <summary>
/// Specifies how a field should be treated when generating dynamic search filters.
/// </summary>
public enum SearchFieldType
{
    /// <summary>
    /// Indicates that the field is a string and supports operations
    /// such as partial match (e.g., <c>contains</c>).
    /// </summary>
    String,

    /// <summary>
    /// Indicates that the field is non-string (e.g., number, status code)
    /// and uses strict comparison (e.g., <c>equals</c>).
    /// </summary>
    Other
}
