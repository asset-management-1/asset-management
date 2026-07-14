using System.Collections.Concurrent;
using System.Reflection;
using Google.Cloud.PubSub.V1;

namespace Be.Haven.Tests.Shared.Be.Haven.Core.Jobs;

public sealed class GcpSubscriberJobTests
{
    [Fact]
    public void SubscribeAsync_Should_RegisterEventHandler_When_EventIsNew()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var action = () => sut.SubscribeAsync<CoreSubscriberSampleEvent, CoreSubscriberSampleHandler>();

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void SubscribeAsync_Should_ThrowArgumentException_When_EventIsAlreadyRegistered()
    {
        // Arrange
        var sut = CreateSut();
        sut.SubscribeAsync<CoreSubscriberSampleEvent, CoreSubscriberSampleHandler>();

        // Act
        var action = () => sut.SubscribeAsync<CoreSubscriberSampleEvent, CoreSubscriberSampleHandler>();

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SubscribeAsync_Should_RemoveHandlerAndThrowArgumentException_When_EventTypeIsAlreadyRegistered()
    {
        // Arrange
        var sut = CreateSut();
        var eventTypes = GetEventTypes(sut);
        eventTypes.TryAdd(typeof(CoreSubscriberSampleEvent).Name, typeof(CorePublisherNoAttributeEvent));

        // Act
        var action = () => sut.SubscribeAsync<CoreSubscriberSampleEvent, CoreSubscriberSampleHandler>();

        // Assert
        action.Should().Throw<ArgumentException>();
        GetEventHandlers(sut).Should().NotContainKey(typeof(CoreSubscriberSampleEvent).Name);
    }

    [Fact]
    public async Task HandleEvent_Should_ReturnTrue_When_HandlerIsResolved()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<CoreSubscriberSampleHandler>();
        var sut = CreateSut(services.BuildServiceProvider());
        sut.SubscribeAsync<CoreSubscriberSampleEvent, CoreSubscriberSampleHandler>();
        var message = CreateMessage(typeof(CoreSubscriberSampleEvent).Name);

        // Act
        var result = await InvokeHandleEventAsync(sut, message);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HandleEvent_Should_ReturnFalse_When_HandlerCannotBeResolved()
    {
        // Arrange
        var sut = CreateSut();
        sut.SubscribeAsync<CoreSubscriberSampleEvent, CoreSubscriberSampleHandler>();
        var message = CreateMessage(typeof(CoreSubscriberSampleEvent).Name);

        // Act
        var result = await InvokeHandleEventAsync(sut, message);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HandleEvent_Should_ReturnFalse_When_RoutingKeyIsMissing()
    {
        // Arrange
        var sut = CreateSut();
        var message = new PubsubMessage
        {
            MessageId = "message-1",
            Data = ByteString.CopyFromUtf8("""{"name":"Haven"}""")
        };

        // Act
        var result = await InvokeHandleEventAsync(sut, message);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HandleEvent_Should_ReturnTrue_When_HandlerIsNotRegistered()
    {
        // Arrange
        var sut = CreateSut();
        var message = CreateMessage("UnknownEvent");

        // Act
        var result = await InvokeHandleEventAsync(sut, message);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_Should_Complete_When_NoEventTypesAreSubscribed()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var action = () => InvokeExecuteAsync(sut, CancellationToken.None);

        // Assert
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ExecuteAsync_Should_SkipSubscribedEvent_When_SubscriptionIsNotConfigured()
    {
        // Arrange
        var sut = CreateSut();
        sut.SubscribeAsync<CoreSubscriberSampleEvent, CoreSubscriberSampleHandler>();

        // Act
        var action = () => InvokeExecuteAsync(sut, CancellationToken.None);

        // Assert
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ExecuteAsync_Should_AckMessageAndStopSubscriber_When_SubscriptionRuns()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<CoreSubscriberSampleHandler>();
        using var provider = services.BuildServiceProvider();
        using var cts = new CancellationTokenSource();
        var subscriber = new FakeSubscriberClient(CreateMessage(typeof(CoreSubscriberSampleEvent).Name), cts);
        var sut = CreateSut(
            provider,
            CreateQueueOptions(),
            (_, _) => Task.FromResult<SubscriberClient>(subscriber));
        sut.SubscribeAsync<CoreSubscriberSampleEvent, CoreSubscriberSampleHandler>();

        // Act
        await InvokeExecuteAsync(sut, cts.Token);

        // Assert
        subscriber.LastReply.Should().Be(SubscriberClient.Reply.Ack);
        subscriber.StopCallCount.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_Should_NackMessage_When_HandlerFails()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var subscriber = new FakeSubscriberClient(CreateMessage(typeof(CoreSubscriberSampleEvent).Name), cts);
        var sut = CreateSut(
            new ServiceCollection().BuildServiceProvider(),
            CreateQueueOptions(),
            (_, _) => Task.FromResult<SubscriberClient>(subscriber));
        sut.SubscribeAsync<CoreSubscriberSampleEvent, CoreSubscriberSampleHandler>();

        // Act
        await InvokeExecuteAsync(sut, cts.Token);

        // Assert
        subscriber.LastReply.Should().Be(SubscriberClient.Reply.Nack);
    }

    [Fact]
    public async Task ExecuteAsync_Should_RetryAndAckMessage_When_HandlerFailsOnce()
    {
        // Arrange
        CoreSubscriberRetryHandler.AttemptCount = 0;
        var services = new ServiceCollection();
        services.AddScoped<CoreSubscriberRetryHandler>();
        using var provider = services.BuildServiceProvider();
        using var cts = new CancellationTokenSource();
        var subscriber = new FakeSubscriberClient(CreateMessage(typeof(CoreSubscriberSampleEvent).Name), cts);
        var sut = CreateSut(
            provider,
            CreateQueueOptions(),
            (_, _) => Task.FromResult<SubscriberClient>(subscriber),
            _ => TimeSpan.Zero);
        sut.SubscribeAsync<CoreSubscriberSampleEvent, CoreSubscriberRetryHandler>();

        // Act
        await InvokeExecuteAsync(sut, cts.Token);

        // Assert
        subscriber.LastReply.Should().Be(SubscriberClient.Reply.Ack);
        CoreSubscriberRetryHandler.AttemptCount.Should().Be(2);
    }

    [Fact]
    public async Task ExecuteAsync_Should_ThrowInvalidOperationException_When_SubscribedEventHasNoGcpAttribute()
    {
        // Arrange
        var sut = CreateSut();
        sut.SubscribeAsync<CorePublisherNoAttributeEvent, CoreSubscriberSampleHandler>();

        // Act
        var action = () => InvokeExecuteAsync(sut, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage(EXECUTE_ASYNC_UNHANDLED_EXCEPTION_MESSAGE);
    }

    [Fact]
    public async Task ExecuteAsync_Should_ThrowInvalidOperationException_When_SubscriberFactoryIsCanceled()
    {
        // Arrange
        var sut = CreateSut(
            new ServiceCollection().BuildServiceProvider(),
            CreateQueueOptions(),
            (_, _) => throw new OperationCanceledException());
        sut.SubscribeAsync<CoreSubscriberSampleEvent, CoreSubscriberSampleHandler>();

        // Act
        var action = () => InvokeExecuteAsync(sut, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage(EXECUTE_ASYNC_CANCELED_MESSAGE);
    }

    [Fact]
    public async Task ExecuteAsync_Should_ThrowInvalidOperationException_When_CancellationIsRequestedBeforeSubscriberStarts()
    {
        // Arrange
        var previousEmulatorHost = Environment.GetEnvironmentVariable("PUBSUB_EMULATOR_HOST");
        Environment.SetEnvironmentVariable("PUBSUB_EMULATOR_HOST", "localhost:1");
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var sut = CreateSut(CreateQueueOptions());
        sut.SubscribeAsync<CoreSubscriberSampleEvent, CoreSubscriberSampleHandler>();

        try
        {
            // Act
            var action = () => InvokeExecuteAsync(sut, cts.Token);

            // Assert
            await action.Should().ThrowAsync<InvalidOperationException>();
        }
        finally
        {
            Environment.SetEnvironmentVariable("PUBSUB_EMULATOR_HOST", previousEmulatorHost);
        }
    }

    private static GcpSubscriberJob CreateSut() =>
        CreateSut(new ServiceCollection().BuildServiceProvider());

    private static GcpSubscriberJob CreateSut(IServiceProvider serviceProvider) =>
        CreateSut(serviceProvider, new QueueInfoOptions());

    private static GcpSubscriberJob CreateSut(QueueInfoOptions queueOptions) =>
        CreateSut(new ServiceCollection().BuildServiceProvider(), queueOptions);

    private static GcpSubscriberJob CreateSut(
        IServiceProvider serviceProvider,
        QueueInfoOptions queueOptions) =>
        CreateSut(serviceProvider, queueOptions, null);

    private static GcpSubscriberJob CreateSut(
        IServiceProvider serviceProvider,
        QueueInfoOptions queueOptions,
        Func<SubscriptionName, CancellationToken, Task<SubscriberClient>> subscriberFactory) =>
        CreateSut(serviceProvider, queueOptions, subscriberFactory, null);

    private static GcpSubscriberJob CreateSut(
        IServiceProvider serviceProvider,
        QueueInfoOptions queueOptions,
        Func<SubscriptionName, CancellationToken, Task<SubscriberClient>> subscriberFactory,
        Func<int, TimeSpan> retryDelayFactory) =>
        new(
            serviceProvider,
            Mock.Of<ILogger<GcpSubscriberJob>>(),
            Options.Create(new GcpOptions
            {
                ProjectId = "project-id"
            }),
            queueOptions,
            subscriberFactory,
            retryDelayFactory);

    private static QueueInfoOptions CreateQueueOptions() =>
        new()
        {
            Topics =
            {
                ["core-topic"] = "core-subscription-id"
            }
        };

    private static PubsubMessage CreateMessage(string eventName)
    {
        var message = new PubsubMessage
        {
            MessageId = "message-1",
            Data = ByteString.CopyFromUtf8("""{"name":"Haven"}""")
        };
        message.Attributes[ROUTING_KEY] = eventName;

        return message;
    }

    private static async Task<bool> InvokeHandleEventAsync(
        GcpSubscriberJob sut,
        PubsubMessage message)
    {
        var method = typeof(GcpSubscriberJob).GetMethod(
            "HandleEvent",
            BindingFlags.Instance | BindingFlags.NonPublic);
        var task = (Task<bool>)method.Invoke(
            sut,
            [message, CancellationToken.None]);

        return await task;
    }

    private static async Task InvokeExecuteAsync(
        GcpSubscriberJob sut,
        CancellationToken cancellationToken)
    {
        var method = typeof(GcpSubscriberJob).GetMethod(
            "ExecuteAsync",
            BindingFlags.Instance | BindingFlags.NonPublic);
        var task = (Task)method.Invoke(
            sut,
            [cancellationToken]);

        await task;
    }

    private static ConcurrentDictionary<string, Type> GetEventHandlers(GcpSubscriberJob sut) =>
        GetPrivateDictionary(sut, "_eventHandlers");

    private static ConcurrentDictionary<string, Type> GetEventTypes(GcpSubscriberJob sut) =>
        GetPrivateDictionary(sut, "_eventTypes");

    private static ConcurrentDictionary<string, Type> GetPrivateDictionary(
        GcpSubscriberJob sut,
        string fieldName)
    {
        var field = typeof(GcpSubscriberJob).GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);

        return (ConcurrentDictionary<string, Type>)field.GetValue(sut);
    }

    private sealed class FakeSubscriberClient : SubscriberClient
    {
        private readonly PubsubMessage _message;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public FakeSubscriberClient(
            PubsubMessage message,
            CancellationTokenSource cancellationTokenSource)
        {
            _message = message;
            _cancellationTokenSource = cancellationTokenSource;
        }

        public Reply LastReply { get; private set; }

        public int StopCallCount { get; private set; }

        public override async Task StartAsync(SubscriptionHandler handler)
        {
            LastReply = await handler.HandleMessage(_message, CancellationToken.None);
            _cancellationTokenSource.Cancel();
        }

        public override Task StopAsync(
            ShutdownOptions shutdownOptions,
            CancellationToken cancellationToken)
        {
            StopCallCount++;

            return Task.CompletedTask;
        }
    }

    private sealed class CoreSubscriberRetryHandler : IEventHandler
    {
        public static int AttemptCount { get; set; }

        public Task HandleAsync(string message, CancellationToken ct)
        {
            AttemptCount++;
            if (AttemptCount == 1)
            {
                throw new InvalidOperationException("transient failure");
            }

            return Task.CompletedTask;
        }
    }
}
