namespace Haven.Application.Dtos.Tenants.Common;

/// <summary>
/// Represents the compact property information shown beside tenant rows.
/// </summary>
public class TenantPropertySummaryResponseDto
{
    /// <summary>
    /// Gets or sets the frontend-safe property identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the property display name.
    /// </summary>
    public string Name { get; set; }
}
