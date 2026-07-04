namespace Haven.Application.Models.Properties.Rows;

/// <summary>
/// Row model for property management counters.
/// </summary>
public class PropertyManagementSummaryRowModel
{
    public int TenantCount { get; set; }

    public int ActiveContractCount { get; set; }

    public int DocumentCount { get; set; }

    public int UnpaidInvoiceCount { get; set; }

    public int OverdueInvoiceCount { get; set; }
}

