namespace Haven.Application.Dtos.Tenants.Join;

/// <summary>
/// Represents the minimal room identity shown on the QR join confirmation screen.
/// </summary>
public class TenantJoinRoomSummaryResponseDto
{
    /// <summary>
    /// Gets or sets the frontend-safe room identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the room display name.
    /// </summary>
    public string Name { get; set; }
}
