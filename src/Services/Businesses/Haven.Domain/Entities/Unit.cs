namespace Haven.Domain.Entities;

/// <summary>
/// Represents a rentable room/unit in asset.Units.
/// </summary>
public class Unit : BaseEntity
{
    /// <summary>
    /// Gets or sets the internal parent property identifier.
    /// </summary>
    public long PropertyId { get; set; }

    /// <summary>
    /// Gets or sets the parent property navigation.
    /// </summary>
    public Property Property { get; set; }

    /// <summary>
    /// Gets or sets the unit code unique within the property.
    /// </summary>
    public string UnitCode { get; set; }

    /// <summary>
    /// Gets or sets the display name for the unit.
    /// </summary>
    public string UnitName { get; set; }

    /// <summary>
    /// Gets or sets the unit type master-data identifier.
    /// </summary>
    public long UnitTypeId { get; set; }

    /// <summary>
    /// Gets or sets the rental mode master-data identifier.
    /// </summary>
    public long RentalModeId { get; set; }

    /// <summary>
    /// Gets or sets the floor number. Haven V1 does not use a Floors table.
    /// </summary>
    public int? FloorNumber { get; set; }

    /// <summary>
    /// Gets or sets the optional unit area in square meters.
    /// </summary>
    public decimal? AreaSqm { get; set; }

    /// <summary>
    /// Gets or sets the bedroom count.
    /// </summary>
    public int? BedroomCount { get; set; }

    /// <summary>
    /// Gets or sets the logical bed/slot count for shared-bed units.
    /// </summary>
    public int? BedCount { get; set; }

    /// <summary>
    /// Gets or sets the optional gender restriction master-data identifier.
    /// </summary>
    public long? GenderRestrictionId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether pets are allowed.
    /// </summary>
    public bool IsPetAllowed { get; set; }

    /// <summary>
    /// Gets or sets the bathroom count.
    /// </summary>
    public int? BathroomCount { get; set; }

    /// <summary>
    /// Gets or sets the suggested base rent amount.
    /// </summary>
    public decimal BaseRentAmount { get; set; }

    /// <summary>
    /// Gets or sets the suggested default deposit amount.
    /// </summary>
    public decimal DefaultDepositAmount { get; set; }

    /// <summary>
    /// Gets or sets the unit status master-data identifier.
    /// </summary>
    public long StatusId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this unit is published publicly.
    /// </summary>
    public bool IsPublished { get; set; }

    /// <summary>
    /// Gets or sets a landlord-facing note for the unit.
    /// </summary>
    public string Note { get; set; }

    /// <summary>
    /// Gets the selectable packages configured for this unit.
    /// </summary>
    public ICollection<UnitPackage> UnitPackages { get; } = new List<UnitPackage>();
}
