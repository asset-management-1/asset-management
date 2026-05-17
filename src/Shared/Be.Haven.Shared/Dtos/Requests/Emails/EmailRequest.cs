namespace Be.Haven.Shared.Dtos.Requests.Emails;

/// <summary>
/// Represents the root email request containing message details and API credentials.
/// </summary>
public class EmailRequest
{
    /// <summary>
    /// Gets or sets the email message details such as recipients, subject, and body.
    /// </summary>
    public RequestData RequestData { get; set; }

    /// <summary>
    /// Gets or sets the API credentials used for authenticating with the email service provider.
    /// </summary>
    public EmailApiCredentialRequest ApiCredential { get; set; }
}

/// <summary>
/// Represents the data of an email message, including recipients, sender, and content.
/// </summary>
public class RequestData
{
    /// <summary>
    /// Gets or sets the list of primary recipients (To).
    /// </summary>
    public List<EmailAddressRequest> To { get; set; }

    /// <summary>
    /// Gets or sets the list of recipients to be carbon copied (CC).
    /// </summary>
    public List<EmailAddressRequest> Cc { get; set; }

    /// <summary>
    /// Gets or sets the list of recipients to be blind carbon copied (BCC).
    /// </summary>
    public List<EmailAddressRequest> Bcc { get; set; }

    /// <summary>
    /// Gets or sets the sender's email account information.
    /// </summary>
    public EmailAccountRequest From { get; set; }

    /// <summary>
    /// Gets or sets the subject line of the email message.
    /// </summary>
    public string Subject { get; set; }

    /// <summary>
    /// Gets or sets the main content of the email message.
    /// </summary>
    public string Body { get; set; }

    /// <summary>
    /// Gets or sets the list of file attachments included in the email message.
    /// </summary>
    public IEnumerable<EmailAttachment> Attachments { get; set; } = [];
}
