namespace Haven.Shared.Configurations;

/// <summary>
/// Represents the configuration for a searchable field, including
/// the OData field name and how the field should be treated during search operations.
/// </summary>
public class SearchFieldConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SearchFieldConfiguration"/> class.
    /// </summary>
    /// <param name="fieldName">
    /// The name of the OData field to be searched.
    /// </param>
    /// <param name="fieldType">
    /// Indicates the type of the field for search logic
    /// (e.g., string-based search or numeric-based comparison).
    /// </param>
    public SearchFieldConfiguration(
        string fieldName,
        SearchFieldType fieldType = SearchFieldType.String)
    {
        FieldName = fieldName;
        FieldType = fieldType;
    }

    /// <summary>
    /// Gets or sets the exact OData field name used in the search filter.
    /// </summary>
    public string FieldName { get; set; }

    /// <summary>
    /// Gets or sets the search behavior type applied to this field,
    /// such as <see cref="SearchFieldType.String"/> or <see cref="SearchFieldType.Other"/>.
    /// </summary>
    public SearchFieldType FieldType { get; set; }
}
