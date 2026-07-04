namespace Haven.Application.Models.MasterData;

/// <summary>
/// Represents a master-data value key requested by business workflows.
/// </summary>
/// <param name="Type">The master-data type code.</param>
/// <param name="Code">The master-data value code.</param>
public sealed record MasterDataKeyModel(string Type, string Code);

