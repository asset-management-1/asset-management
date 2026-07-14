namespace Haven.Application.Models.Properties.Rows;

/// <summary>
/// Row model for a property list/detail header.
/// </summary>
public class PropertyRowModel
{
    /// <summary>
    /// Gets or sets the total number of property rows matched before paging.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the internal property identifier.
    /// </summary>
    public long PropertyId { get; set; }

    /// <summary>
    /// Gets or sets the public property identifier.
    /// </summary>
    public Guid PropertyPublicId { get; set; }

    /// <summary>
    /// Gets or sets the property code.
    /// </summary>
    public string PropertyCode { get; set; }

    /// <summary>
    /// Gets or sets the property name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the property description.
    /// </summary>
    public string Description { get; set; }

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

    /// <summary>
    /// Gets or sets total floors.
    /// </summary>
    public int TotalFloors { get; set; }

    /// <summary>
    /// Gets or sets total units.
    /// </summary>
    public int TotalUnits { get; set; }

    /// <summary>
    /// Gets or sets available unit count.
    /// </summary>
    public int AvailableUnitCount { get; set; }

    /// <summary>
    /// Gets or sets occupied unit count.
    /// </summary>
    public int OccupiedUnitCount { get; set; }

    /// <summary>
    /// Gets or sets occupied-unit ratio as a decimal from 0 to 1.
    /// </summary>
    public decimal? OccupancyRate { get; set; }

    /// <summary>
    /// Gets or sets maintenance unit count.
    /// </summary>
    public int MaintenanceUnitCount { get; set; }

    /// <summary>
    /// Gets or sets published unit count.
    /// </summary>
    public int PublishedUnitCount { get; set; }

}

