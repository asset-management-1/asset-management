namespace Be.Haven.Shared.Dtos.Requests.Emails;

/// <summary>
/// Represents an email address used in an email message 
/// (for example, in the To, CC, or BCC list).
/// </summary>
public class EmailAddressRequest
{
    /// <summary>
    /// Gets or sets the email address of the recipient.
    /// </summary>
    public string Email { get; set; }
}
