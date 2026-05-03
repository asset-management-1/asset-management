namespace Be.Haven.Shared.Dtos.Options;

/// <summary>
/// Represents configuration settings for sending emails.
/// </summary>
public class EmailOptions
{
    /// <summary>
    /// Gets or sets the SendGrid API key.
    /// </summary>
    public string ApiKey { get; set; }

    /// <summary>
    /// Gets or sets the default sender email address.
    /// </summary>
    public string FromEmail { get; set; }

    /// <summary>
    /// Gets or sets the default sender display name.
    /// </summary>
    public string FromName { get; set; }

    /// <summary>
    /// Legacy field kept for backward compatibility with older configuration.
    /// </summary>
    public string BaseUrl { get; set; }

    /// <summary>
    /// Legacy field kept for backward compatibility with older configuration.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Legacy field kept for backward compatibility with older configuration.
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// Legacy field kept for backward compatibility with older configuration.
    /// </summary>
    public string EndPoints { get; set; }

    /// <summary>
    /// Legacy field kept for backward compatibility with older configuration.
    /// </summary>
    public string NameFrom { get; set; }

    /// <summary>
    /// Legacy field kept for backward compatibility with older configuration.
    /// </summary>
    public string PasswordFrom { get; set; }
}
