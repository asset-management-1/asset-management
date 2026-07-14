namespace Haven.Application.Models.Properties.Rows;

/// <summary>
/// Row model for a whole-building rental contract shown on property detail.
/// </summary>
public class PropertyWholeBuildingRentalRowModel
{
    /// <summary>
    /// Gets or sets the contract public identifier.
    /// </summary>
    public Guid? ContractPublicId { get; set; }

    /// <summary>
    /// Gets or sets the contract code.
    /// </summary>
    public string ContractCode { get; set; }

    /// <summary>
    /// Gets or sets the tenant public identifier.
    /// </summary>
    public Guid? TenantPublicId { get; set; }

    /// <summary>
    /// Gets or sets the tenant display name.
    /// </summary>
    public string TenantName { get; set; }

    /// <summary>
    /// Gets or sets the contract status code.
    /// </summary>
    public string StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the contract status display name.
    /// </summary>
    public string StatusName { get; set; }

    /// <summary>
    /// Gets or sets the whole-building rent amount.
    /// </summary>
    public decimal? TotalRentAmount { get; set; }

    /// <summary>
    /// Gets or sets the contract start date.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Gets or sets the contract end date.
    /// </summary>
    public DateTime? EndDate { get; set; }
}
