namespace Authentication.Application.Dtos.Users.Profiles;

/// <summary>
/// Represents the authenticated user information returned by the auth API.
/// </summary>
public class UserInfoResponseDto
{
    /// <summary>
    /// Full name shown for the current user account.
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    /// Username used to sign in.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Primary email address of the user.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Primary phone number of the user.
    /// </summary>
    public string PhoneNumber { get; set; }

    /// <summary>
    /// Public avatar URL stored on the user profile.
    /// </summary>
    public string AvatarUrl { get; set; }

    /// <summary>
    /// User date of birth.
    /// </summary>
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>
    /// User gender.
    /// </summary>
    public GenderEnum? Gender { get; set; }

    /// <summary>
    /// Display name resolved from the linked party.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// The party context currently active for the user.
    /// </summary>
    public PartyTypeEnum? CurrentContext { get; set; }

    /// <summary>
    /// All available party contexts linked to the user.
    /// </summary>
    public List<PartyTypeEnum> AvailableContexts { get; set; } = [];

    /// <summary>
    /// Linked external providers.
    /// </summary>
    public List<ExternalProviderResponseDto> ExternalProviders { get; set; } = [];

    /// <summary>
    /// KYC summary for the current identity user across linked party contexts.
    /// </summary>
    public KycSummaryResponseDto KycSummary { get; set; } = new();

}
