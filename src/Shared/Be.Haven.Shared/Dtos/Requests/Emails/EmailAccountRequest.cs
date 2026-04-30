namespace Be.Haven.Shared.Dtos.Requests.Emails;

/// <summary>
/// Represents the email account credentials used for sending emails.
/// </summary>
public class EmailAccountRequest
{
    /// <summary>
    /// Gets or sets the sender's email address.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Gets or sets the password associated with the email account.
    /// </summary>
    public string Password { get; set; }
}
