namespace Authentication.Application.Dtos.Authentications.Register;

/// <summary>
/// Represents the uniqueness-check payload for registration.
/// </summary>
public class RegistrationUniquenessRequestDto
{
    /// <summary>
    /// Gets or sets the normalised username.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Gets or sets the normalised email address.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Gets or sets the phone number to validate.
    /// </summary>
    public string PhoneNumber { get; set; }
}
