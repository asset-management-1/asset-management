namespace Haven.Core.Interfaces.Softs.IIsActive;

/// <summary>
/// Defines a contract for entities that support active/inactive status.
/// </summary>
public interface IIsActive
{
    /// <summary>
    /// Gets or sets a value indicating whether the entity is active.
    /// </summary>
    bool IsActive { get; set; }
}