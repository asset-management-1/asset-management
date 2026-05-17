namespace Be.Haven.Core.Helpers;

public static class ValidationHelper
{
    /// <summary>
    /// Validates an object and its properties using data annotations.
    /// The method ensures that validation is performed on the object and its entire graph, including nested objects.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the object to validate. It must be a reference type.
    /// </typeparam>
    /// <param name="obj">
    /// The object instance to validate. Throws an exception if the object is null.
    /// </param>
    /// <returns>
    /// A list of <see cref="ValidationResult"/> that contains any validation errors found during the process.
    /// If the object is valid, the returned list is empty.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the <paramref name="obj"/> parameter is null.
    /// </exception>
    public static List<ValidationResult> Validate<T>(T obj) where T : class
    {
        if (obj is null)
        {
            throw new ArgumentNullException(nameof(obj), VALIDATION_OBJECT_NULL_ERROR);
        }

        // Validate the current object against its annotations
        var validationResults = new List<ValidationResult>();
        var visitedObjects = new HashSet<object>();
        ValidateObjectRecursively(obj, validationResults, visitedObjects);

        return validationResults;
    }

    /// <summary>
    /// Recursively validates an object and its properties using data annotations.
    /// The method ensures that the entire object graph is validated, including nested objects and collections.
    /// </summary>
    /// <param name="obj">
    /// The object instance to validate. Objects of string or primitive types are ignored.
    /// Null values are skipped, and circular references are detected to prevent infinite recursion.
    /// </param>
    /// <param name="validationResults">
    /// A collection where the validation results are accumulated. This collection is updated
    /// with any validation errors discovered during the recursive process.
    /// </param>
    /// <param name="visitedObjects">
    /// A set of visited objects used to avoid circular references. This parameter ensures
    /// that an object already validated is not visited again during the recursion.
    /// </param>
    private static void ValidateObjectRecursively(
        object obj,
        List<ValidationResult> validationResults,
        HashSet<object> visitedObjects)
    {
        if (obj == null || !visitedObjects.Add(obj)) return;

        var type = obj.GetType();
        if (obj is string || type.IsValueType || obj is JToken) return;

        Validator.TryValidateObject(obj, new ValidationContext(obj), validationResults, validateAllProperties: true);

        foreach (var property in type.GetProperties().Where(p => p.CanRead && p.GetIndexParameters().Length == 0))
        {
            var propertyValue = property.GetValue(obj);
            switch (propertyValue)
            {
                case null:
                case string:
                case var _ when propertyValue.GetType().IsValueType:
                    continue;
                case IEnumerable<object> collection:
                    foreach (var item in collection.Where(i => i != null))
                    {
                        ValidateObjectRecursively(item, validationResults, visitedObjects);
                    }
                    break;
                default:
                    ValidateObjectRecursively(propertyValue, validationResults, visitedObjects);
                    break;
            }
        }
    }
}
