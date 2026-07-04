namespace Haven.Infrastructure.Models.MasterData;

/// <summary>
/// Represents one raw master-data row returned by Dapper.
/// </summary>
public class MasterDataRowModel
{
    /// <summary>
    /// Gets or sets the master-data type key requested by the caller.
    /// </summary>
    public string KeyType { get; set; }

    /// <summary>
    /// Gets or sets the master-data code key requested by the caller.
    /// </summary>
    public string KeyCode { get; set; }

    /// <summary>
    /// Gets or sets the resolved master-data value identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the resolved master-data type.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets the resolved master-data code.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Gets or sets the resolved master-data display name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the resolved master-data description.
    /// </summary>
    public string Description { get; set; }
}

