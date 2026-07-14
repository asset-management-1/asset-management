namespace Haven.Application.Models.MasterData;

/// <summary>
/// Represents a master-data value key requested by business workflows.
/// </summary>
/// <param name="Type">The master-data type code.</param>
/// <param name="Code">The master-data value code.</param>
public sealed record MasterDataKeyModel(string Type, string Code)
{
    /// <summary>
    /// Creates a master-data key from a known master-data type.
    /// </summary>
    /// <param name="type">The master-data type taxonomy value.</param>
    /// <param name="code">The master-data value code.</param>
    public MasterDataKeyModel(MasterDataTypeEnum type, string code) : this(type.ToString(), code)
    {
    }
}

