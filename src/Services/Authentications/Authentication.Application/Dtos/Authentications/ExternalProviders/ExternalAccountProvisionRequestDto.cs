namespace Authentication.Application.Dtos.Authentications.ExternalProviders;

/// <summary>
/// Represents the prepared external-account payload used to project validated provider data into Party and User entities.
/// </summary>
public class ExternalAccountProvisionRequestDto
{
    /// <summary>
    /// Gets or sets the party type identifier.
    /// </summary>
    public long PartyTypeId { get; set; }

    /// <summary>
    /// Gets or sets the party active status identifier.
    /// </summary>
    public long PartyStatusId { get; set; }

    /// <summary>
    /// Gets or sets the user active status identifier.
    /// </summary>
    public long UserStatusId { get; set; }

    /// <summary>
    /// Gets or sets the generated local username.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Gets or sets the normalised email.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Gets or sets the resolved full name.
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    /// Gets or sets the user-confirmed phone number.
    /// </summary>
    public string PhoneNumber { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the email is considered confirmed.
    /// </summary>
    public bool EmailConfirmed { get; set; }
}
