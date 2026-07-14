namespace Authentication.Domain.Entities;

/// <summary>
/// Stores one dynamic value belonging to an authentication master-data category.
/// </summary>
public class MasterDataValue : BaseEntity
{
    /// <summary>
    /// Parent master data type ID.
    /// </summary>
    public long MasterDataTypeId { get; set; }

    /// <summary>
    /// Unique code within a master data type.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Display name of the master data value.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Optional description for the master data value.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Indicates whether the value is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Navigation to parent master data type.
    /// </summary>
    public virtual MasterDataType MasterDataType { get; set; }

    /// <summary>
    /// Collection of parties associated with various party types.
    /// </summary>
    public virtual ICollection<Party> PartyPartyTypes { get; set; } = new List<Party>();

    /// <summary>
    /// Collection of party entities representing the statuses associated with the master data value.
    /// </summary>
    public virtual ICollection<Party> PartyStatuses { get; set; } = new List<Party>();

    /// <summary>
    /// Navigation collection of permissions using this status.
    /// </summary>
    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();

    /// <summary>
    /// Navigation collection of roles using this status.
    /// </summary>
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    /// <summary>
    /// Navigation collection of users using this status.
    /// </summary>
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
