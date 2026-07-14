namespace Haven.Application.Models.Tenants.Join;

/// <summary>
/// Represents a request to preview a room join token.
/// </summary>
public class TenantJoinPreviewRequestModel
{
    /// <summary>
    /// Gets or sets the token from the QR payload.
    /// </summary>
    public string Token { get; set; }
}
