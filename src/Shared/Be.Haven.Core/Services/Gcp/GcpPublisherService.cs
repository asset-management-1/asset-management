namespace Be.Haven.Core.Services.Gcp;

/// <summary>
/// Publishes messages to Google Cloud Pub/Sub.
/// This service reads the GCP project ID from an environment variable and sends serialized messages
/// to the topic specified by <see cref="GcpAttribute"/> on the message type.
/// </summary>
public class GcpPublisherService : IGcpPublisherService
{
    private readonly QueueInfoOptions _queueOptions;
    private readonly ILogger<GcpPublisherService> _logger;
    private readonly IJsonSerializerService _serializerService;
    private readonly string _gcpProjectId;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Provides a service for publishing messages to a Google Cloud Platform (GCP) Pub/Sub topic.
    /// Handles the serialization of messages and utilizes configurations for GCP and queuing options.
    /// </summary>
    public GcpPublisherService(
        ILogger<GcpPublisherService> logger,
        IJsonSerializerService serializerService,
        IOptions<GcpOptions> gcpOption,
        QueueInfoOptions queueOptions, 
        IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _serializerService = serializerService;
        _queueOptions = queueOptions;
        _httpContextAccessor = httpContextAccessor;
        _gcpProjectId = gcpOption.Value.ProjectId;
    }

    /// <summary>
    /// Publishes a message to a configured GCP topic asynchronously.
    /// Handles serialization of the message and manages exceptions during the publication process.
    /// </summary>
    /// <typeparam name="T">The type of the message, which must implement <see cref="IBaseMessageEvent"/>.</typeparam>
    /// <param name="message">The message object to publish.</param>
    /// <param name="cancellationToken">The token used to signal request cancellation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the GCP project ID is not configured or when message serialization fails.</exception>
    public async Task PublishMessageAsync<T>(T message, CancellationToken cancellationToken) where T : IBaseMessageEvent
    {
        try
        {
            _logger.LogInformation(GPSPUBLISHER_SERVICE_PUBLISH_ASYNC_START, typeof(T).Name);

            // Retrieve Pub/Sub topic information from the message type attribute
            var attributes = typeof(T).GetCustomAttribute<GcpAttribute>();

            if (attributes is null || 
                string.IsNullOrWhiteSpace(attributes.TopicKey) || 
                !_queueOptions.Topics.TryGetValue(attributes.TopicKey, out var topicId) || 
                string.IsNullOrWhiteSpace(topicId))
            {
                throw new InvalidOperationException(
                    string.Format(MISSING_GCP_ATTRIBUTE_SETTING_TOPICID, typeof(T).Name));
            }
            // Resolve correlation/trace id
            var correlationId =
                _httpContextAccessor.HttpContext?.Items[X_CORRELATION_ID]?.ToString()
                ?? Activity.Current?.GetTagItem(OTEL_CORRELATION_ID_TAG)?.ToString()
                ?? Activity.Current?.TraceId.ToString()
                ?? Guid.NewGuid().ToString("N");
            
            message.TraceId = correlationId;
            
            // Create the topic name using the project ID and topic ID from the attribute
            var topicName = new TopicName(_gcpProjectId, topicId);
        
            // Serialize the message and wrap it in a PubsubMessage
            var pubSubMessage = new PubsubMessage
            {
                Data = ByteString.CopyFromUtf8(_serializerService.SerializeIgnoreToPascalProperties(message)),
                Attributes =
                {
                    { ROUTING_KEY, typeof(T).Name },
                    { X_CORRELATION_ID, correlationId }
                }
            };
            
            var act = Activity.Current;
            if (act is not null)
            {
                // W3C context propagation for downstream tracing
                if (!string.IsNullOrWhiteSpace(act.Id))
                    pubSubMessage.Attributes[TRACEPARENT] = act.Id;
                
                pubSubMessage.Attributes[TRACE_ID] = act.TraceId.ToString();
            }
            
            // Build a PublisherClient for the specified topic.
            // EmulatorDetection = EmulatorOrProduction: use emulator if PUBSUB_EMULATOR_HOST is set, otherwise production.
            // The cancellationToken lets you cancel client creation if needed.
            var publisher = await new PublisherClientBuilder
            {
                TopicName = topicName,
                EmulatorDetection = EmulatorDetection.EmulatorOrProduction,
            }.BuildAsync(cancellationToken);

            // Publish the message to Google Cloud Pub/Sub
            var msgId = await publisher.PublishAsync(pubSubMessage);

            _logger.LogInformation(GPSPUBLISHER_PUBLISHASYNC_MESSAGE_PUBLISHED, msgId);
        }
        catch (Exception ex)
        {
            // Log the error and throw a wrapped InvalidOperationException
            _logger.LogError(ex, GPSPUBLISHER_SERVICEPUBLISHASYNC_SEND_MESSAGEERRORLOG);
            throw new InvalidOperationException(GPSPUBLISHER_SERVICEPUBLISHASYNC_SENDMESSAGE_ERROREXCEPTION, ex);
        }
    }
}
