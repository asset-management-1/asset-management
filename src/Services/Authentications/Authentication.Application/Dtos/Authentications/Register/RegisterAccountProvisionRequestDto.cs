namespace Authentication.Application.Dtos.Authentications.Register;

/// <summary>
/// Represents the prepared register payload used to project pending-register data into Party and User entities.
/// </summary>
public class RegisterAccountProvisionRequestDto
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
    /// Gets or sets the normalized username.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Gets or sets the normalized email.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Gets or sets the phone number.
    /// </summary>
    public string PhoneNumber { get; set; }

    /// <summary>
    /// Gets or sets the full name.
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    /// Gets or sets the password hash.
    /// </summary>
    public string PasswordHash { get; set; }
}
