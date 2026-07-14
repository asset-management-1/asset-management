namespace Haven.Application.Dtos.Tenants.Join;

/// <summary>
/// Represents the token that FE renders into a room tenant-join QR code.
/// </summary>
public class TenantJoinQrResponseDto
{
    /// <summary>
    /// Gets or sets the short-lived tenant join token.
    /// </summary>
    public string Token { get; set; }
}
