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
    /// Gets or sets the province code.
    /// </summary>
    public string ProvinceCode { get; set; }

    /// <summary>
    /// Gets or sets the province display name.
    /// </summary>
    public string ProvinceName { get; set; }

    /// <summary>
    /// Gets or sets the district code.
    /// </summary>
    public string DistrictCode { get; set; }

    /// <summary>
    /// Gets or sets the district display name.
    /// </summary>
    public string DistrictName { get; set; }

    /// <summary>
    /// Gets or sets the ward code.
    /// </summary>
    public string WardCode { get; set; }

    /// <summary>
    /// Gets or sets the ward display name.
    /// </summary>
    public string WardName { get; set; }

    /// <summary>
    /// Gets or sets the street address.
    /// </summary>
    public string StreetAddress { get; set; }

    /// <summary>
    /// Gets or sets the formatted address.
    /// </summary>
    public string FormattedAddress { get; set; }

    /// <summary>
    /// Gets or sets latitude.
    /// </summary>
    public decimal? Latitude { get; set; }

    /// <summary>
    /// Gets or sets longitude.
    /// </summary>
    public decimal? Longitude { get; set; }
}
