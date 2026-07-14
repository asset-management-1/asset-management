namespace Haven.Application.Dtos.Properties.Detail;

/// <summary>
/// Represents the active whole-building contract card in property detail.
/// </summary>
public class PropertyWholeBuildingRentalContractResponseDto
{
    /// <summary>
    /// Gets or sets the safe public contract identifier exposed as a UI id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the contract code.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Gets or sets the tenant safe public identifier.
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the tenant or company display name.
    /// </summary>
    public string Tenant { get; set; }

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
