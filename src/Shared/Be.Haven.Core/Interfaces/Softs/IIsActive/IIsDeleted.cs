namespace Be.Haven.Core.Interfaces.Softs.IIsActive;

/// <summary>
/// Defines a contract for entities that support active/inactive status.
/// </summary>
public interface IIsDeleted
{
    /// <summary>
    /// Gets or sets a value indicating whether the entity is marked as deleted.
    /// </summary>
    bool IsDeleted { get; set; }
}
