namespace Haven.Application.Models.Locations;

/// <summary>
/// Represents a resolved location lookup value.
/// </summary>
public class LocationLookupModel
{
    /// <summary>
    /// Gets or sets the location identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the location code.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Gets or sets the location display name.
    /// </summary>
    public string Name { get; set; }
}

