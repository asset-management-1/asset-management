namespace Haven.Application.Models.MasterData;

/// <summary>
/// Represents a resolved master-data value.
/// </summary>
public class MasterDataValueModel
{
    /// <summary>
    /// Gets or sets the internal master-data value identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the master-data type code.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets the value code.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the optional value description.
    /// </summary>
    public string Description { get; set; }
}

