namespace Haven.Domain.Entities;

/// <summary>
/// Represents a tenant or member occupancy in leasing.Occupancies.
/// </summary>
public class Occupancy : BaseEntity
{
    /// <summary>
    /// Gets or sets the linked contract identifier when the tenant signs a contract.
    /// </summary>
    public long? ContractId { get; set; }

    /// <summary>
    /// Gets or sets the linked contract.
    /// </summary>
    public Contract Contract { get; set; }

    /// <summary>
    /// Gets or sets the tenant party identifier.
    /// </summary>
    public long PartyId { get; set; }

    /// <summary>
    /// Gets or sets the tenant party.
    /// </summary>
    public Party Party { get; set; }

    /// <summary>
    /// Gets or sets the property identifier.
    /// </summary>
    public long PropertyId { get; set; }

    /// <summary>
    /// Gets or sets the room/unit identifier.
    /// </summary>
    public long UnitId { get; set; }

    /// <summary>
    /// Gets or sets the occupied room/unit.
    /// </summary>
    public Unit Unit { get; set; }

    /// <summary>
    /// Gets or sets the occupancy start date.
    /// </summary>
    public DateOnly StartDate { get; set; }

    /// <summary>
    /// Gets or sets the occupancy end date.
    /// </summary>
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// Gets or sets the occupancy status master-data identifier.
    /// </summary>
    public long StatusId { get; set; }

    /// <summary>
    /// Gets or sets whether this occupancy is the contract-signing primary tenant.
    /// </summary>
    public bool IsPrimaryTenant { get; set; }
}
