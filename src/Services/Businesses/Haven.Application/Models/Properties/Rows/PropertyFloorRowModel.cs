namespace Haven.Application.Models.Properties.Rows;

/// <summary>
/// Row model for a floor summary derived from units.
/// </summary>
public class PropertyFloorRowModel
{
    /// <summary>
    /// Gets or sets the parent property public identifier.
    /// </summary>
    public Guid PropertyPublicId { get; set; }

    /// <summary>
    /// Gets or sets the floor number derived from unit rows.
    /// </summary>
    public int? FloorNumber { get; set; }

    /// <summary>
    /// Gets or sets the number of room units on the floor.
    /// </summary>
    public int UnitCount { get; set; }

    /// <summary>
    /// Gets or sets the number of available room units on the floor.
    /// </summary>
    public int AvailableUnitCount { get; set; }

    /// <summary>
    /// Gets or sets the number of occupied room units on the floor.
    /// </summary>
    public int OccupiedUnitCount { get; set; }

    /// <summary>
    /// Gets or sets the number of maintenance room units on the floor.
    /// </summary>
    public int MaintenanceUnitCount { get; set; }

}

