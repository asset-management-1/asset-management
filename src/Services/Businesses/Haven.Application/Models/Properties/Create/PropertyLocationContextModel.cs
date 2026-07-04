namespace Haven.Application.Models.Properties.Create;

/// <summary>
/// Represents resolved optional location rows for property creation.
/// </summary>
public class PropertyLocationContextModel
{
    /// <summary>
    /// Gets or sets the resolved province, when supplied.
    /// </summary>
    public LocationLookupModel Province { get; set; }

    /// <summary>
    /// Gets or sets the resolved district, when supplied.
    /// </summary>
    public LocationLookupModel District { get; set; }

    /// <summary>
    /// Gets or sets the resolved ward, when supplied.
    /// </summary>
    public LocationLookupModel Ward { get; set; }
}

