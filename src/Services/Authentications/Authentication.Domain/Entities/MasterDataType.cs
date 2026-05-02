namespace Authentication.Domain.Entities;

public partial class MasterDataType : BaseEntity
{
    /// <summary>
    /// Unique code for master data type.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Display name of master data type.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Optional description of master data type.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Navigation collection of values under this master data type.
    /// </summary>
    public virtual ICollection<MasterDataValue> MasterDataValues { get; set; } = new List<MasterDataValue>();
}
