namespace Authentication.Domain.Entities;

public partial class Permission : BaseEntity
{
    /// <summary>
    /// Unique permission code.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Display name of permission.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Module/group that owns this permission.
    /// </summary>
    public string Module { get; set; }

    /// <summary>
    /// Optional description for permission.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Status master data value ID.
    /// </summary>
    public long StatusId { get; set; }

    /// <summary>
    /// Navigation collection of role-permission mappings.
    /// </summary>
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    /// <summary>
    /// Navigation to status master data value.
    /// </summary>
    public virtual MasterDataValue Status { get; set; }
}
