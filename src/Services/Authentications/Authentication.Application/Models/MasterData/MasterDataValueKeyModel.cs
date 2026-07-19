namespace Authentication.Application.Models.MasterData;

/// <summary>
/// Represents one master-data type/value key requested by an authentication workflow.
/// </summary>
public sealed class MasterDataValueKeyModel : IEquatable<MasterDataValueKeyModel>
{
    /// <summary>
    /// Initialises a new instance of the <see cref="MasterDataValueKeyModel"/> class.
    /// </summary>
    /// <param name="type">The master-data type code or name.</param>
    /// <param name="value">The master-data value code or name.</param>
    public MasterDataValueKeyModel(string type, string value)
    {
        Type = type;
        Value = value;
    }

    /// <summary>
    /// Gets the master-data type code or name.
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Gets the master-data value code or name.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Determines whether this key equals another key by type and value.
    /// </summary>
    /// <param name="other">The other key to compare.</param>
    /// <returns><c>true</c> when both normalised type and value match; otherwise <c>false</c>.</returns>
    public bool Equals(MasterDataValueKeyModel other)
    {
        return other is not null
               && string.Equals(Type, other.Type, StringComparison.Ordinal)
               && string.Equals(Value, other.Value, StringComparison.Ordinal);
    }

    /// <summary>
    /// Determines whether this key equals another object.
    /// </summary>
    /// <param name="obj">The object to compare.</param>
    /// <returns><c>true</c> when the object is an equal key; otherwise <c>false</c>.</returns>
    public override bool Equals(object obj)
    {
        return Equals(obj as MasterDataValueKeyModel);
    }

    /// <summary>
    /// Gets a hash code from the type and value.
    /// </summary>
    /// <returns>The key hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Type, Value);
    }
}
