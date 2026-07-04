namespace Haven.Application.Models.Properties.Create;

/// <summary>
/// Represents the resolved input used to map one generated unit entity.
/// </summary>
public class PropertyUnitBuildModel
{
    /// <summary>
    /// Gets or sets the parent property entity.
    /// </summary>
    public Property Property { get; set; }

    /// <summary>
    /// Gets or sets the floor number.
    /// </summary>
    public int FloorNumber { get; set; }

    /// <summary>
    /// Gets or sets the generated unit code.
    /// </summary>
    public string UnitCode { get; set; }

    /// <summary>
    /// Gets or sets the unit display name.
    /// </summary>
    public string UnitName { get; set; }

    /// <summary>
    /// Gets or sets the resolved unit type identifier.
    /// </summary>
    public long UnitTypeId { get; set; }

    /// <summary>
    /// Gets or sets the resolved rental mode identifier.
    /// </summary>
    public long RentalModeId { get; set; }

    /// <summary>
    /// Gets or sets the resolved unit status identifier.
    /// </summary>
    public long StatusId { get; set; }

    /// <summary>
    /// Gets or sets the optional unit area.
    /// </summary>
    public decimal? AreaSqm { get; set; }

    /// <summary>
    /// Gets or sets the base rent amount.
    /// </summary>
    public decimal BaseRentAmount { get; set; }

    /// <summary>
    /// Gets or sets the default deposit amount.
    /// </summary>
    public decimal DefaultDepositAmount { get; set; }

    /// <summary>
    /// Gets or sets the optional bed/slot count.
    /// </summary>
    public int? BedCount { get; set; }

    /// <summary>
    /// Gets or sets whether pets are allowed.
    /// </summary>
    public bool IsPetAllowed { get; set; }
}

