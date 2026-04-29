namespace Haven.Shared.Dtos.Options;

/// <summary>
/// Represents configuration settings for connecting to an external email service.
/// </summary>
public class EmailOptions
{
    /// <summary>
    /// Gets or sets the base URL of the external email API service.
    /// </summary>
    public string BaseUrl { get; set; }

    /// <summary>
    /// Gets or sets the username used to authenticate with the email API.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Gets or sets the password or API key used for authentication with the email API.
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// Gets or sets the specific endpoint or route of the email API used to send emails.
    /// </summary>
    public string EndPoints { get; set; }

    /// <summary>
    /// Gets or sets the default sender's email address for outgoing messages.
    /// </summary>
    public string NameFrom { get; set; }

    /// <summary>
    /// Gets or sets the password associated with the default sender's email account.
    /// </summary>
    public string PasswordFrom { get; set; }
}
