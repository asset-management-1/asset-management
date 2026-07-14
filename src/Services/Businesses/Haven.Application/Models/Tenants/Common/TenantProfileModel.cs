namespace Haven.Application.Models.Tenants.Common;

/// <summary>
/// Represents a tenant profile used to create an unlinked tenant party.
/// </summary>
public class TenantProfileModel
{
    /// <summary>
    /// Gets or sets the tenant display name.
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    /// Gets or sets the tenant phone number.
    /// </summary>
    public string Phone { get; set; }

    /// <summary>
    /// Gets or sets the tenant email.
    /// </summary>
    public string Email { get; set; }
}
