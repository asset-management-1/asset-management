namespace Haven.Application.Dtos.Properties.Detail;

/// <summary>
/// Represents editable basic information for a property detail screen.
/// </summary>
public class PropertyDetailBasicInfoResponseDto
{
    /// <summary>
    /// Gets or sets the frontend-safe property identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the property code shown to frontend.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Gets or sets the property name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the property type code.
    /// </summary>
    public string PropertyTypeCode { get; set; }

    /// <summary>
    /// Gets or sets the property type display name.
    /// </summary>
    public string PropertyTypeName { get; set; }

    /// <summary>
    /// Gets or sets the property status code.
    /// </summary>
    public string StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the property status display name.
    /// </summary>
    public string StatusName { get; set; }

    /// <summary>
    /// Gets or sets whether the property is published.
    /// </summary>
    public bool IsPublished { get; set; }

    /// <summary>
    /// Gets or sets the thumbnail URL.
    /// </summary>
    public string ThumbnailUrl { get; set; }
}
