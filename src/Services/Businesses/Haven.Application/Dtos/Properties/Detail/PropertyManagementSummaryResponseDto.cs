namespace Haven.Application.Dtos.Properties.Detail;

/// <summary>
/// Represents operational counters for the building detail hub.
/// </summary>
public class PropertyManagementSummaryResponseDto
{
    /// <summary>
    /// Gets or sets the current total tenant quantity.
    /// </summary>
    public int TotalTenants { get; set; }

    /// <summary>
    /// Gets or sets the active total contract quantity.
    /// </summary>
    public int TotalActiveContracts { get; set; }

    /// <summary>
    /// Gets or sets the total document quantity linked to the property.
    /// </summary>
    public int TotalDocuments { get; set; }

    /// <summary>
    /// Gets or sets the total unpaid invoice quantity.
    /// </summary>
    public int TotalUnpaidInvoices { get; set; }

    /// <summary>
    /// Gets or sets the total overdue invoice quantity.
    /// </summary>
    public int TotalOverdueInvoices { get; set; }
}

