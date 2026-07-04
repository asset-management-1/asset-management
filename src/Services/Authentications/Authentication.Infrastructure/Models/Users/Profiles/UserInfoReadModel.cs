namespace Authentication.Infrastructure.Models.Users.Profiles;

/// <summary>
/// Represents the single-row Dapper projection used to assemble current-user information.
/// </summary>
public class UserInfoReadModel
{
    /// <summary>
    /// Gets or sets the user's full name.
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    /// Gets or sets the username used to sign in.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Gets or sets the user's primary email.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Gets or sets the user's primary phone number.
    /// </summary>
    public string PhoneNumber { get; set; }

    /// <summary>
    /// Gets or sets the public avatar URL.
    /// </summary>
    public string AvatarUrl { get; set; }

    /// <summary>
    /// Gets or sets the user's date of birth.
    /// </summary>
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>
    /// Gets or sets the user's gender code.
    /// </summary>
    public string Gender { get; set; }

    /// <summary>
    /// Gets or sets the active party display name.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the active party context.
    /// </summary>
    public string CurrentContext { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the active party has submitted KYC data.
    /// </summary>
    public bool KycIsSubmitted { get; set; }

    /// <summary>
    /// Gets or sets the optional KYC status.
    /// </summary>
    public string KycStatus { get; set; }

    /// <summary>
    /// Gets or sets the submitted identity document type code.
    /// </summary>
    public string KycIdentifierType { get; set; }

    /// <summary>
    /// Gets or sets the submitted identity document type display name.
    /// </summary>
    public string KycIdentifierTypeDisplayName { get; set; }

    /// <summary>
    /// Gets or sets the masked national identifier.
    /// </summary>
    public string KycMaskedIdentifier { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a front-side identity scan exists.
    /// </summary>
    public bool KycHasFrontFile { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a back-side identity scan exists.
    /// </summary>
    public bool KycHasBackFile { get; set; }

    /// <summary>
    /// Gets or sets the JSON array of available context codes.
    /// </summary>
    public string AvailableContextsJson { get; set; }

    /// <summary>
    /// Gets or sets the JSON array of linked external providers.
    /// </summary>
    public string ExternalProvidersJson { get; set; }

}
