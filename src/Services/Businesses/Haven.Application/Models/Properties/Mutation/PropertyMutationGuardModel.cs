namespace Haven.Application.Models.Properties.Mutation;

/// <summary>
/// Represents dependency counters that decide whether a property graph can be safely mutated.
/// </summary>
public class PropertyMutationGuardModel
{
    /// <summary>
    /// Gets or sets the number of contracts linked to the property rooms.
    /// </summary>
    public int ContractCount { get; set; }

    /// <summary>
    /// Gets or sets the number of active occupancies linked to the property rooms.
    /// </summary>
    public int OccupancyCount { get; set; }

    /// <summary>
    /// Gets or sets the number of invoices or invoice lines linked to the property rooms.
    /// </summary>
    public int InvoiceCount { get; set; }

    /// <summary>
    /// Gets or sets the number of room vehicles linked to the property rooms.
    /// </summary>
    public int VehicleCount { get; set; }

    /// <summary>
    /// Gets or sets the number of document links attached to the property or its rooms.
    /// </summary>
    public int DocumentLinkCount { get; set; }

    /// <summary>
    /// Gets a value indicating whether any guarded dependency exists.
    /// </summary>
    public bool HasDependencies =>
        ContractCount > 0
        || OccupancyCount > 0
        || InvoiceCount > 0
        || VehicleCount > 0
        || DocumentLinkCount > 0;

    /// <summary>
    /// Gets a value indicating whether pending property delete must be blocked.
    /// </summary>
    public bool HasDeleteBlockingDependencies =>
        ContractCount > 0 || OccupancyCount > 0;
}
