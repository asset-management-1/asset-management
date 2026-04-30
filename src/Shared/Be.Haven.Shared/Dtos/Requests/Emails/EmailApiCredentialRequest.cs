namespace Be.Haven.Shared.Dtos.Requests.Emails;

/// <summary>
/// Represents the API credentials required to authenticate 
/// when sending an email request through an external email service.
/// </summary>
public class EmailApiCredentialRequest
{
    /// <summary>
    /// Gets or sets the username used for API authentication.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Gets or sets the password or API key used for API authentication.
    /// </summary>
    public string Password { get; set; }
}
