namespace Haven.Core.Services;

/// <summary>
/// Provides functionality for sending emails through a configured third-party email service.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IThirdPartyApiService _thirdPartyApiService;
    private readonly IJsonSerializerService _jsonSerializerService;
    private readonly ILogger<EmailService> _logger;
    private readonly EmailOptions _emailOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailService"/> class.
    /// </summary>
    /// <param name="thirdPartyApiService">The service responsible for making third-party API calls.</param>
    /// <param name="jsonSerializerService">The service used for serializing and deserializing JSON data.</param>
    /// <param name="logger">The logger instance for logging application events and errors.</param>
    /// <param name="emailOptions">Configuration options for the email service such as base URL and endpoints.</param>
    public EmailService(
        IThirdPartyApiService thirdPartyApiService,
        IJsonSerializerService jsonSerializerService,
        ILogger<EmailService> logger,
        IOptions<EmailOptions> emailOptions)
    {
        _thirdPartyApiService = thirdPartyApiService;
        _jsonSerializerService = jsonSerializerService;
        _logger = logger;
        _emailOptions = emailOptions.Value;
    }

    /// <summary>
    /// Sends an email using a third-party email API.
    /// </summary>
    /// <param name="request">The email request object containing recipients, subject, body, and attachments.</param>
    /// <returns>
    /// A boolean value indicating whether the email was sent successfully.
    /// </returns>
    public async Task<bool> SendEmailAsync(EmailRequest request)
    {
        try
        {
            // Build the base API request with email payload and configuration
            var req = new BaseThirdPartyApiRequest
            {
                HttpClientName = EMAIL_SERVICE,
                Method = POST,
                BaseUrl = _emailOptions.BaseUrl,
                Endpoint = _emailOptions.EndPoints,
                ContentType = TEXT_JSON,
                Content = _jsonSerializerService.Serialize(request),
            };

            // Execute the request using the third-party API service
            var result = await _thirdPartyApiService.HandleApiData<BaseThirdPartyApiRequest, bool>(req);
            return result;
        }
        catch (Exception ex)
        {
            // Log any unexpected errors during email sending
            _logger.LogError(ex, ERROR_SENDING_EMAIL, request.RequestData.To);
            return false;
        }
    }
}
