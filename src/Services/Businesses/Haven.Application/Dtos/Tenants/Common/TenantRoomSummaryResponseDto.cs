namespace Haven.Application.Dtos.Tenants.Common;

/// <summary>
/// Represents the compact room information shown beside tenant rows.
/// </summary>
public class TenantRoomSummaryResponseDto
{
    /// <summary>
    /// Gets or sets the frontend-safe room identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the room code.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Gets or sets the room display name.
    /// </summary>
    public string Name { get; set; }
}
