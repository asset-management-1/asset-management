namespace Authentication.Application.Dtos.Authentications.ExternalProviders;

/// <summary>
/// Carries validated client fields into the external account-provisioning service.
/// </summary>
public class CompleteExternalRegistrationRequestDto
{
    /// <summary>
    /// Gets or sets the supported external provider name.
    /// </summary>
    public string Provider { get; set; }

    /// <summary>
    /// Gets or sets the provider credential to revalidate.
    /// </summary>
    public string ExternalToken { get; set; }

    /// <summary>
    /// Gets or sets the user-confirmed full name.
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    /// Gets or sets the user-entered phone number.
    /// </summary>
    public string PhoneNumber { get; set; }

    /// <summary>
    /// Gets or sets the normalised party type code.
    /// </summary>
    public string PartyType { get; set; }
}
