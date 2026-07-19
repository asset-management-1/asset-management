namespace Authentication.Application.Dtos.Authentications.Register;

/// <summary>
/// Represents the temporary register payload stored in Redis until the email OTP is verified.
/// </summary>
public class PendingRegisterCacheRequestDto
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
    /// Gets or sets the trimmed phone number.
    /// </summary>
    public string PhoneNumber { get; set; }

    /// <summary>
    /// Gets or sets the user's full name.
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    /// Gets or sets the requested party type.
    /// </summary>
    public string PartyType { get; set; }

    /// <summary>
    /// Gets or sets the protected password hash.
    /// </summary>
    public string PasswordHash { get; set; }

    /// <summary>
    /// Gets or sets the UTC creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
