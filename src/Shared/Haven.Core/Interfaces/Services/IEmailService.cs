namespace Haven.Core.Interfaces.Services;

/// <summary>
/// Defines the contract for an email service responsible for sending emails.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email using the specified email request details.
    /// </summary>
    /// <param name="request">The email request containing recipients, content, and credentials.</param>
    /// <returns>
    /// A boolean value indicating whether the email was sent successfully.
    /// </returns>
    Task<bool> SendEmailAsync(EmailRequest request);
}
