namespace Authentication.Domain.Entities;

/// <summary>
/// Groups authorisation permissions under a stable role code.
/// </summary>
public class Role : BaseEntity
{
    /// <summary>
    /// Unique role code.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Display name of role.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Optional description for role.
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

    /// <summary>
    /// Navigation collection of user-role mappings.
    /// </summary>
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
