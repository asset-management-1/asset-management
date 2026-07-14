namespace Haven.Application.Dtos.Tenants.Detail;

/// <summary>
/// Represents tenant occupancy detail for one room.
/// </summary>
public class TenantDetailResponseDto : TenantListItemResponseDto
{
    /// <summary>
    /// Gets or sets the tenant email when available.
    /// </summary>
    public string Email { get; set; }
}
