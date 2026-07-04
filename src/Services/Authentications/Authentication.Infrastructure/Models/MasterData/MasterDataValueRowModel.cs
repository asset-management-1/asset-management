namespace Authentication.Infrastructure.Models.MasterData;

/// <summary>
/// Represents one raw master-data value row returned by Dapper.
/// </summary>
internal sealed class MasterDataValueRowModel
{
    /// <summary>
    /// Gets or sets the master-data type key requested by the caller.
    /// </summary>
    public string KeyType { get; set; }

    /// <summary>
    /// Gets or sets the master-data value key requested by the caller.
    /// </summary>
    public string KeyValue { get; set; }

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
