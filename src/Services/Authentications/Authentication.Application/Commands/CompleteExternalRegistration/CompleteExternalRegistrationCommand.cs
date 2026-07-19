namespace Authentication.Application.Commands.CompleteExternalRegistration;

/// <summary>
/// Completes local account registration for a provider identity that was not previously linked.
/// </summary>
public class CompleteExternalRegistrationCommand : ICommand<ResponseDto<LoginResponseDto>>
{
    /// <summary>
    /// Gets or sets the supported external provider name.
    /// </summary>
    public string Provider { get; set; }

    /// <summary>
    /// Gets or sets the provider credential that infrastructure must revalidate.
    /// </summary>
    public string ExternalToken { get; set; }

    /// <summary>
    /// Gets or sets the user-confirmed display name.
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    /// Gets or sets the user-entered phone number.
    /// </summary>
    public string PhoneNumber { get; set; }

    /// <summary>
    /// Gets or sets the requested Haven party type code.
    /// </summary>
    public string PartyType { get; set; }
}
