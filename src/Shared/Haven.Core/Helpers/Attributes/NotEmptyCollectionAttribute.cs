namespace Haven.Core.Helpers.Attributes;

/// <summary>
/// Validates that a collection has at least one element.
/// Example: [NotEmptyCollection(ErrorMessage = "The collection must not be empty")]
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class NotEmptyCollectionAttribute : ValidationAttribute
{
    /// <summary>
    /// Validates that a collection has at least one element.
    /// Example: [NotEmptyCollection(ErrorMessage = "The collection must not be empty")]
    /// </summary>
    public NotEmptyCollectionAttribute() : base("The collection must not be empty")
    {
    }

    /// <summary>
    /// Checks if the collection contains at least one element
    /// </summary>
    /// <param name="value">Collection to validate</param>
    /// <returns>true if the collection has elements, false otherwise</returns>
    public override bool IsValid(object value)
    {
        if (value is IEnumerable enumerable)
        {
            return enumerable.GetEnumerator().MoveNext();
        }
        return false;
    }
}

