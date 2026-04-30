namespace Be.Haven.Shared.Dtos.Requests.Emails;

/// <summary>
/// Represents a file attachment included in an email message.
/// </summary>
public class EmailAttachment
{
    /// <summary>
    /// Gets or sets the name of the attached file.
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Gets or sets the Base64-encoded string representing the file content.
    /// </summary>
    public string Base64 { get; set; }

    /// <summary>
    /// Gets or sets the MIME content type of the attachment (e.g., "application/pdf").
    /// </summary>
    public string ContentType { get; set; }
}
