namespace Haven.Application.Dtos.Tenants.Common;

/// <summary>
/// Represents an inline tenant profile used when the tenant has no account yet.
/// </summary>
public class TenantProfileRequestDto
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
