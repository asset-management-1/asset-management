namespace Be.Haven.Core.Jobs;

/// <summary>
/// Background service that subscribes to Google Pub/Sub topics and dispatches
/// incoming messages to the appropriate event handlers registered in the application.
/// </summary>
public class GcpSubscriberJob : BackgroundService
{
    /// <summary>
    /// Logger for diagnostic and operational messages.
    /// </summary>
    private readonly ILogger<GcpSubscriberJob> _logger;

    /// <summary>
    /// Used to create scoped service providers to resolve event handlers per message.
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Retry policy applied to event handler execution to handle transient errors (e.g., NpgsqlException).
    /// </summary>
    private readonly AsyncRetryPolicy _retryPolicy;

    /// <summary>
    /// Google Cloud project ID read from environment variable <c>GCP_PROJECT_ID</c>.
    /// </summary>
    private readonly string _gcpProjectId;

    /// <summary>
    /// Maps event name to its corresponding event handler type.
    /// </summary>
    private readonly ConcurrentDictionary<string, Type> _eventHandlers;

    /// <summary>
    /// Maps event name to its corresponding event type.
    /// </summary>
    private readonly ConcurrentDictionary<string, Type> _eventTypes;

    /// <summary>
    /// Configuration options containing details about topics and subscriptions
    /// used for managing message queues in the Google Cloud Pub/Sub environment.
    /// </summary>
    private readonly QueueInfoOptions _queueOptions;

    /// <summary>
    /// Factory used to create Pub/Sub subscriber clients for configured subscriptions.
    /// </summary>
    private readonly Func<SubscriptionName, CancellationToken, Task<SubscriberClient>> _subscriberFactory;

    /// <summary>
    /// A background service responsible for managing subscriber clients for event processing in a Google Cloud Platform (GCP) environment.
    /// Handles initialization of subscribers and manages retry policies for transient failures.
    /// </summary>
    /// <param name="serviceProvider">The root service provider used to create handler scopes.</param>
    /// <param name="logger">The logger used for subscriber diagnostics.</param>
    /// <param name="gcpOption">The configured GCP options.</param>
    /// <param name="queueOptions">The configured Pub/Sub topic and subscription keys.</param>
    /// <param name="subscriberFactory">Optional factory used to create subscriber clients.</param>
    /// <param name="retryDelayFactory">Optional factory used to compute retry delays.</param>
    public GcpSubscriberJob(
        IServiceProvider serviceProvider,
        ILogger<GcpSubscriberJob> logger,
        IOptions<GcpOptions> gcpOption,
        QueueInfoOptions queueOptions,
        Func<SubscriptionName, CancellationToken, Task<SubscriberClient>> subscriberFactory = null,
        Func<int, TimeSpan> retryDelayFactory = null)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _queueOptions = queueOptions;
        _gcpProjectId = gcpOption.Value.ProjectId;
        _subscriberFactory = subscriberFactory ?? CreateSubscriberAsync;
        retryDelayFactory ??= retryAttempt => TimeSpan.FromSeconds(Math.Pow(SECOND_RETRY_NUMBER, retryAttempt));

        // Configure an async retry policy:
        // - Handle all exceptions
        // - Retry 5 times with exponential backoff (2^attempt seconds)
        // - Log a warning on each retry attempt
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                DEFAULT_RETRY_NUMBER,
                retryDelayFactory,
                (exception, timeSpan, retryCount, _) =>
                {
                    _logger.LogWarning(exception, HANDLE_EVENT_RETRY, retryCount, timeSpan);
                });

        _eventHandlers = new(StringComparer.OrdinalIgnoreCase);
        _eventTypes = new(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Background worker entry point:
    /// Starts one subscriber client per registered event type and keeps them running until the host stops.
    /// NOTE: This method assumes topics and subscriptions are already provisioned (e.g., via IaC or CLI).
    /// </summary>
    /// <param name="stoppingToken">Token that is cancelled when the host is shutting down.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // Start one subscriber per registered event type.
            // Each subscriber runs until cancellation is requested.
            var runTasks = _eventTypes.Values.Select(async ev =>
            {
                // Retrieve subscription info from the event type attribute.
                var attributes = ev.GetCustomAttribute<GcpAttribute>()
                    ?? throw new InvalidDataException(string.Format(MISSING_GCP_ATTRIBUTE_SETTING_EXCEPTION, ev.Name));

                if (string.IsNullOrWhiteSpace(attributes.SubscriptionKey) || 
                    !_queueOptions.Topics.TryGetValue(attributes.TopicKey, out var subscriptionId) || 
                    string.IsNullOrWhiteSpace(subscriptionId))
                {
                    _logger.LogWarning(SKIP_EVENT_SUBSCRIPTIONID_EMPTY, ev.Name);
                    return;
                }

                // Build subscription name (topics/subscriptions are assumed to exist already).
                var subscriptionName = SubscriptionName.FromProjectSubscription(_gcpProjectId, subscriptionId);

                // Build the subscriber client through the configured factory.
                var subscriber = await _subscriberFactory(subscriptionName, stoppingToken);

                await using var reg = stoppingToken.Register(() =>
                {
                    _ = subscriber.StopAsync(new SubscriberClient.ShutdownOptions(), CancellationToken.None);
                });

                _logger.LogInformation(SUBSCRIBER_STARTED, subscriptionName);

                // Start receiving messages. This task completes when the subscriber stops.
                await subscriber.StartAsync(async (msg, _) =>
                {
                    // Delegate processing to your handler pipeline.
                    // Return Ack/Nack based on HandleEvent result.
                    var ok = await HandleEvent(msg, stoppingToken);
                    return ok ? SubscriberClient.Reply.Ack : SubscriberClient.Reply.Nack;
                });
            });

            // Run all subscribers until the service is stopped.
            await Task.WhenAll(runTasks);
        }
        catch (OperationCanceledException ex)
        {
            // Normal shutdown triggered by stoppingToken.
            _logger.LogError(ex, EXECUTE_ASYNC_CANCELED);
            throw new InvalidOperationException(EXECUTE_ASYNC_CANCELED_MESSAGE);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, EXECUTE_ASYNC_UNHANDLED_EXCEPTION);
            throw new InvalidOperationException(EXECUTE_ASYNC_UNHANDLED_EXCEPTION_MESSAGE);
        }
    }

    /// <summary>
    /// Registers an event handler for a specific event type.
    /// </summary>
    /// <typeparam name="TEvent">The event type to subscribe to, must implement IBaseMessageEvent.</typeparam>
    /// <typeparam name="THandler">The handler type, must implement IEventHandler.</typeparam>
    /// <exception cref="InvalidOperationException">Thrown when the event or handler type is already registered.</exception>
    public void SubscribeAsync<TEvent, THandler>()
        where TEvent : IBaseMessageEvent
        where THandler : IEventHandler
    {
        var eventName = typeof(TEvent).Name;

        if (!_eventHandlers.TryAdd(eventName, typeof(THandler)))
        {
            throw new ArgumentException(
                string.Format(HANDLER_ALREADY_REGISTERED, eventName));
        }

        if (!_eventTypes.TryAdd(eventName, typeof(TEvent)))
        {
            _eventHandlers.TryRemove(eventName, out _);
            throw new ArgumentException(
                string.Format(EVENT_TYPE_ALREADY_REGISTERED, eventName));
        }
    }

    /// <summary>
    /// Creates the default Google Pub/Sub subscriber client for one subscription.
    /// </summary>
    /// <param name="subscriptionName">The Pub/Sub subscription name.</param>
    /// <param name="cancellationToken">The token used to cancel client creation.</param>
    /// <returns>The created subscriber client.</returns>
    private static async Task<SubscriberClient> CreateSubscriberAsync(
        SubscriptionName subscriptionName,
        CancellationToken cancellationToken)
    {
        // Build the subscriber client with bounded outstanding messages and unlimited byte flow.
        return await new SubscriberClientBuilder
        {
            SubscriptionName = subscriptionName,
            Settings = new SubscriberClient.Settings
            {
                FlowControlSettings = new FlowControlSettings(
                    maxOutstandingElementCount: 10L,
                    maxOutstandingByteCount: null)
            }
        }.BuildAsync(cancellationToken);
    }

    /// <summary>
    /// Handles the processing of a message received from a Google Pub/Sub subscription by routing it
    /// to the appropriate event handler based on the message attributes. Logs the start, completion,
    /// and duration of the operation and manages error handling for unsuccessful processing.
    /// </summary>
    /// <param name="message">The PubsubMessage to be processed, containing attributes and payload data.</param>
    /// <param name="cancellationToken">A token used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// A Task that resolves to a boolean indicating the result of message processing: true if processing
    /// succeeded (acknowledge), false if it encountered an error (negative acknowledgment).
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the required event handler for processing the message cannot be located or is invalid.
    /// </exception>
    private async Task<bool> HandleEvent(
        PubsubMessage message,
        CancellationToken cancellationToken)
    {
        // Start timing the message handling for metrics/logging.
        var stopWatch = Stopwatch.StartNew();

        _logger.LogInformation(HANDLE_EVENT_START, message.MessageId);

        try
        {
            // Extract the routing key from message attributes.
            // This determines which event handler should process the message.
            var eventName = message.Attributes[ROUTING_KEY];

            // Check if a handler has been registered for this event type.
            if (_eventHandlers.TryGetValue(eventName, out var handlerType) && handlerType != null)
            {
                // Create a scoped service provider for dependency injection.
                using var scope = _serviceProvider.CreateScope();

                // Attempts to resolve the event handler instance of the specified <c>handlerType</c>
                // from the current DI scope. Casts it to <see cref="IEventHandler"/> for processing.
                // Throws an <see cref="InvalidOperationException"/> if no matching handler is registered
                // in the DI container or if the resolved instance does not implement <see cref="IEventHandler"/>.
                var msgHandler = scope.ServiceProvider.GetService(handlerType) as IEventHandler ??
                    throw new InvalidOperationException(HANDLER_NOT_RESOLVED_FROM_DI_SIMPLE);

                // Convert the message payload to UTF-8 string.
                var msg = message.Data.ToStringUtf8();

                // Execute the handler within a retry policy for resiliency.
                await _retryPolicy.ExecuteAsync(() => msgHandler.HandleAsync(msg, cancellationToken));
            }

            // Return true to indicate successful handling (ACK).
            return true;
        }
        catch (Exception ex)
        {
            // Log and return false to indicate a processing error (NACK).
            _logger.LogError(ex, HANDLE_EVENT_ERROR);
            return false;
        }
        finally
        {
            // Stop timing and log how long processing took.
            stopWatch.Stop();
            _logger.LogInformation(HANDLE_EVENT_FINISHED, message.MessageId, stopWatch.Elapsed);
        }
    }
}
