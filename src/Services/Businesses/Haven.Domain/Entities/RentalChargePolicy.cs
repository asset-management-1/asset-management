namespace Haven.Domain.Entities;

/// <summary>
/// Represents a property-level or unit-level rental charge policy.
/// </summary>
public class RentalChargePolicy : BaseEntity
{
    /// <summary>
    /// Gets or sets the internal property identifier.
    /// </summary>
    public long PropertyId { get; set; }

    /// <summary>
    /// Gets or sets the parent property navigation.
    /// </summary>
    public Property Property { get; set; }

    /// <summary>
    /// Gets or sets the optional unit identifier for overrides.
    /// </summary>
    public long? UnitId { get; set; }

    /// <summary>
    /// Gets or sets the optional unit navigation for room-specific policy overrides.
    /// </summary>
    public Unit Unit { get; set; }

    /// <summary>
    /// Gets or sets the invoice line type master-data identifier.
    /// </summary>
    public long LineTypeId { get; set; }

    /// <summary>
    /// Gets or sets the optional vehicle type master-data identifier for parking fees.
    /// </summary>
    public long? VehicleTypeId { get; set; }

    /// <summary>
    /// Gets or sets the display name of the charge.
    /// </summary>
    public string ChargeName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether amount is usage-based unit price.
    /// </summary>
    public bool IsUsageBased { get; set; }

    /// <summary>
    /// Gets or sets the charge amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the common status master-data identifier.
    /// </summary>
    public long StatusId { get; set; }
}
