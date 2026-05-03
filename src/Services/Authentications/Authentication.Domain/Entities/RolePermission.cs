namespace Authentication.Domain.Entities;

public class RolePermission : BaseEntity
{
    /// <summary>
    /// Role ID in the mapping.
    /// </summary>
    public long RoleId { get; set; }

    /// <summary>
    /// Permission ID in the mapping.
    /// </summary>
    public long PermissionId { get; set; }

    /// <summary>
    /// Navigation to mapped permission.
    /// </summary>
    public virtual Permission Permission { get; set; }

    /// <summary>
    /// Navigation to mapped role.
    /// </summary>
    public virtual Role Role { get; set; }
}
