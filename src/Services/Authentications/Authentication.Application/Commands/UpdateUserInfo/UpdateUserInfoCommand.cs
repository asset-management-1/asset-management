namespace Authentication.Application.Commands.UpdateUserInfo;

/// <summary>
/// Represents a request to update profile fields for the current authenticated user.
/// </summary>
public class UpdateUserInfoCommand : ICommand<ResponseDto<OperationStatusResponseDto>>
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
    /// Gets or sets the optional avatar image. Send as a binary multipart file part.
    /// </summary>
    public IFormFile AvatarFile { get; set; }

    /// <summary>
    /// Gets or sets the user's date of birth.
    /// </summary>
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>
    /// Gets or sets the gender. Accepted values: <c>Male</c>, <c>Female</c>.
    /// </summary>
    public GenderEnum? Gender { get; set; }
}
