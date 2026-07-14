namespace Haven.Application.Dtos.Tenants.Join;

/// <summary>
/// Represents the room preview shown after a tenant scans a room QR code.
/// </summary>
public class TenantJoinPreviewResponseDto
{
    /// <summary>
    /// Gets or sets the compact property information.
    /// </summary>
    public TenantPropertySummaryResponseDto Property { get; set; }

    /// <summary>
    /// Gets or sets the compact room information.
    /// </summary>
    public TenantJoinRoomSummaryResponseDto Room { get; set; }
}
