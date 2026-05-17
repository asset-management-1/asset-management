namespace Authentication.Infrastructure.Models.MasterData;

/// <summary>
/// Represents one row returned by the batched master-data Dapper lookup.
/// </summary>
internal sealed class MasterDataValueLookupRowModel
{
    /// <summary>
    /// Gets or sets the normalized lookup type returned from the requested input.
    /// </summary>
    public string LookupType { get; set; }

    /// <summary>
    /// Gets or sets the normalized lookup value returned from the requested input.
    /// </summary>
    public string LookupValue { get; set; }

    /// <summary>
    /// Gets or sets the master-data value identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the owning master-data type identifier.
    /// </summary>
    public long MasterDataTypeId { get; set; }

    /// <summary>
    /// Gets or sets the master-data value code.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Gets or sets the master-data value name.
    /// </summary>
    public string Name { get; set; }
}
