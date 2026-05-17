namespace Authentication.Application.Dtos.Users.Profiles;

/// <summary>
/// Represents profile fields that can be updated by the current authenticated user.
/// </summary>
public class UpdateUserInfoRequestDto
{
    /// <summary>
    /// Gets or sets the user's full display name.
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    /// Gets or sets the user's primary phone number.
    /// </summary>
    public string PhoneNumber { get; set; }

    /// <summary>
    /// Gets or sets the optional avatar image uploaded through the backend.
    /// </summary>
    public IFormFile AvatarFile { get; set; }

    /// <summary>
    /// Gets or sets the user's date of birth.
    /// </summary>
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>
    /// Gets or sets the gender code, such as MALE or FEMALE.
    /// </summary>
    public string Gender { get; set; }
}
