namespace Authentication.Application.Dtos.Authentications.ExternalProviders;

/// <summary>
/// Represents either a completed external login or fields for first-time registration.
/// </summary>
public class ExternalLoginResponseDto
{
    /// <summary>
    /// Gets or sets whether the provider identity requires local registration.
    /// </summary>
    public bool IsNewRegistration { get; set; }

    /// <summary>
    /// Gets or sets the token pair for an existing or linked account.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public LoginResponseDto Login { get; set; }

    /// <summary>
    /// Gets or sets provider-derived prefill fields for first-time registration.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ExternalRegistrationPrefillDto Registration { get; set; }
}

/// <summary>
/// Contains provider-derived fields that the mobile client may prefill.
/// </summary>
public class ExternalRegistrationPrefillDto
{
    /// <summary>
    /// Gets or sets the validated provider email.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Gets or sets the provider display name when available.
    /// </summary>
    public string FullName { get; set; }
}
