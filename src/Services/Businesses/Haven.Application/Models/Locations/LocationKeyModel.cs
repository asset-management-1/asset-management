namespace Haven.Application.Models.Locations;

/// <summary>
/// Represents location codes requested by property creation.
/// </summary>
public class LocationKeyModel
{
    /// <summary>
    /// Gets or sets the province code.
    /// </summary>
    public string ProvinceCode { get; set; }

    /// <summary>
    /// Gets or sets the district code.
    /// </summary>
    public string DistrictCode { get; set; }

    /// <summary>
    /// Gets or sets the ward code.
    /// </summary>
    public string WardCode { get; set; }
}

