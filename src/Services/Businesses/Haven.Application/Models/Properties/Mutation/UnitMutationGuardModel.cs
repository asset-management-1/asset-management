namespace Haven.Application.Models.Properties.Mutation;

/// <summary>
/// Represents dependency counters for one room/unit.
/// </summary>
public class UnitMutationGuardModel
{
    /// <summary>
    /// Gets or sets the room public identifier.
    /// </summary>
    public Guid UnitPublicId { get; set; }

    /// <summary>
    /// Gets or sets the number of contracts linked to the room.
    /// </summary>
    public int ContractCount { get; set; }

    /// <summary>
    /// Gets or sets the number of active occupancies linked to the room.
    /// </summary>
    public int OccupancyCount { get; set; }

    /// <summary>
    /// Gets or sets the number of invoices or invoice lines linked to the room.
    /// </summary>
    public int InvoiceCount { get; set; }

    /// <summary>
    /// Gets or sets the number of vehicles linked to the room.
    /// </summary>
    public int VehicleCount { get; set; }

    /// <summary>
    /// Gets or sets the number of document links attached to the room.
    /// </summary>
    public int DocumentLinkCount { get; set; }

    /// <summary>
    /// Gets a value indicating whether the room has active dependency data.
    /// </summary>
    public bool HasDependencies =>
        ContractCount > 0
        || OccupancyCount > 0
        || InvoiceCount > 0
        || VehicleCount > 0
        || DocumentLinkCount > 0;

    /// <summary>
    /// Gets a value indicating whether room/floor deletion must be blocked.
    /// </summary>
    public bool HasDeleteBlockingDependencies =>
        ContractCount > 0 || OccupancyCount > 0;
}
