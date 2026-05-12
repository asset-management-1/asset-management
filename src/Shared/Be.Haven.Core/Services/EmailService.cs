namespace Be.Haven.Core.Services;

/// <summary>
/// Sends emails through SendGrid using the shared <see cref="IEmailService"/> contract.
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly EmailOptions _emailOptions;
    private readonly SendGridClient _sendGridClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailService"/> class.
    /// </summary>
    /// <param name="logger">The logger used for operational events and failures.</param>
    /// <param name="emailOptions">The configured email settings.</param>
    public EmailService(
        ILogger<EmailService> logger,
        IOptions<EmailOptions> emailOptions)
    {
        _logger = logger;
        _emailOptions = emailOptions.Value;
        _sendGridClient = new SendGridClient(_emailOptions.ApiKey);
    }

    /// <summary>
    /// Sends an email using SendGrid.
    /// </summary>
    /// <param name="request">The email payload to send.</param>
    /// <param name="cancellationToken">The token used to cancel the SendGrid request.</param>
    /// <returns><c>true</c> when SendGrid accepts the request; otherwise <c>false</c>.</returns>
    public async Task<bool> SendEmailAsync(
        EmailRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate required SendGrid configuration before creating and sending the email message.
            if (string.IsNullOrWhiteSpace(_emailOptions.ApiKey) || string.IsNullOrWhiteSpace(_emailOptions.FromEmail))
            {
                _logger.LogError(EmailLogs.MISSING_CONFIGURATION);
                return false;
            }

            // Validate that the request contains at least one valid recipient.
            if (request?.RequestData?.To?.Any(x => !string.IsNullOrWhiteSpace(x.Email)) != true)
            {
                _logger.LogWarning(EmailLogs.INVALID_REQUEST);
                return false;
            }

            var message = new SendGridMessage
            {
                From = new EmailAddress(_emailOptions.FromEmail, _emailOptions.FromName),
                Subject = request.RequestData.Subject,
                HtmlContent = request.RequestData.Body,
                PlainTextContent = request.RequestData.Body
            };

            // Add primary recipients.
            foreach (var recipient in request.RequestData.To.Where(x => !string.IsNullOrWhiteSpace(x.Email)))
            {
                message.AddTo(new EmailAddress(recipient.Email));
            }

            // Add carbon copy recipients when provided.
            if (request.RequestData.Cc is not null)
            {
                foreach (var recipient in request.RequestData.Cc.Where(x => !string.IsNullOrWhiteSpace(x.Email)))
                {
                    message.AddCc(new EmailAddress(recipient.Email));
                }
            }

            // Add blind carbon copy recipients when provided.
            if (request.RequestData.Bcc is not null)
            {
                foreach (var recipient in request.RequestData.Bcc.Where(x => !string.IsNullOrWhiteSpace(x.Email)))
                {
                    message.AddBcc(new EmailAddress(recipient.Email));
                }
            }

            // Add valid base64 attachments when provided.
            if (request.RequestData.Attachments is not null)
            {
                foreach (var attachment in request.RequestData.Attachments.Where(x => !string.IsNullOrWhiteSpace(x.Base64)))
                {
                    message.AddAttachment(
                        attachment.FileName,
                        attachment.Base64,
                        attachment.ContentType,
                        "attachment");
                }
            }

            // Propagate request cancellation to SendGrid instead of letting email sends outlive the caller.
            var response = await _sendGridClient.SendEmailAsync(message, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(EmailLogs.SEND_SUCCESS);
                return true;
            }

            _logger.LogWarning(EmailLogs.SEND_FAILED, response.StatusCode);
            return false;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, EmailLogs.SEND_EXCEPTION);
            return false;
        }
    }
}
