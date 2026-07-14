namespace Haven.Application.Models.Tenants.Join;

/// <summary>
/// Represents a tenant request to confirm a room join token.
/// </summary>
public class TenantJoinConfirmRequestModel
{
    /// <summary>
    /// Gets or sets the QR token being confirmed.
    /// </summary>
    public string Token { get; set; }

    /// <summary>
    /// Gets or sets the current tenant party context.
    /// </summary>
    public CurrentPartyContextModel CurrentParty { get; set; }
}
