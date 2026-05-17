using PubSubTopicName = Google.Cloud.PubSub.V1.TopicName;
using PubsubMessage = Google.Cloud.PubSub.V1.PubsubMessage;

namespace Be.Haven.Tests.Shared.Be.Haven.Core.Services.Gcp;

public sealed class GcpPublisherServiceTests
{
    [Fact]
    public async Task PublishMessageAsync_Should_PublishPreparedMessage_When_EventHasConfiguredTopic()
    {
        // Arrange
        PubSubTopicName capturedTopic = null;
        PubsubMessage capturedMessage = null;
        var message = new CorePublisherSampleEvent
        {
            Name = "created"
        };
        var publisher = new Mock<IGcpPubSubPublisherClient>();
        publisher
            .Setup(x => x.PublishAsync(It.IsAny<PubsubMessage>()))
            .Callback<PubsubMessage>(model => capturedMessage = model)
            .ReturnsAsync("message-id");
        var factory = new Mock<IGcpPubSubPublisherClientFactory>();
        factory
            .Setup(x => x.CreateAsync(It.IsAny<PubSubTopicName>(), It.IsAny<CancellationToken>()))
            .Callback<PubSubTopicName, CancellationToken>((topic, _) => capturedTopic = topic)
            .ReturnsAsync(publisher.Object);
        var serializer = new Mock<IJsonSerializerService>();
        serializer
            .Setup(x => x.SerializeIgnoreToPascalProperties(message))
            .Returns("""{"Name":"created"}""");
        var sut = CreateSut(
            serializer,
            factory,
            CreateHttpContextAccessor("correlation-123"));

        // Act
        await sut.PublishMessageAsync(message, CancellationToken.None);

        // Assert
        message.TraceId.Should().Be("correlation-123");
        capturedTopic.ProjectId.Should().Be("project-id");
        capturedTopic.TopicId.Should().Be("topic-id");
        capturedMessage.Data.ToStringUtf8().Should().Be("""{"Name":"created"}""");
        capturedMessage.Attributes[ROUTING_KEY].Should().Be(nameof(CorePublisherSampleEvent));
        capturedMessage.Attributes[X_CORRELATION_ID].Should().Be("correlation-123");
    }

    [Fact]
    public async Task PublishMessageAsync_Should_PropagateActivityTraceAttributes_When_ActivityExists()
    {
        // Arrange
        using var activity = new Activity("publish-test");
        activity.Start();
        PubsubMessage capturedMessage = null;
        var message = new CorePublisherSampleEvent
        {
            Name = "created"
        };
        var publisher = new Mock<IGcpPubSubPublisherClient>();
        publisher
            .Setup(x => x.PublishAsync(It.IsAny<PubsubMessage>()))
            .Callback<PubsubMessage>(model => capturedMessage = model)
            .ReturnsAsync("message-id");
        var factory = new Mock<IGcpPubSubPublisherClientFactory>();
        factory
            .Setup(x => x.CreateAsync(It.IsAny<PubSubTopicName>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(publisher.Object);
        var serializer = new Mock<IJsonSerializerService>();
        serializer
            .Setup(x => x.SerializeIgnoreToPascalProperties(message))
            .Returns("""{"Name":"created"}""");
        var sut = CreateSut(
            serializer,
            factory,
            new HttpContextAccessor());

        // Act
        await sut.PublishMessageAsync(message, CancellationToken.None);

        // Assert
        capturedMessage.Attributes[TRACEPARENT].Should().Be(activity.Id);
        capturedMessage.Attributes[TRACE_ID].Should().Be(activity.TraceId.ToString());
    }

    [Fact]
    public async Task PublishMessageAsync_Should_ThrowWrappedException_When_EventHasNoGcpAttribute()
    {
        // Arrange
        var factory = new Mock<IGcpPubSubPublisherClientFactory>();
        var sut = CreateSut(
            new Mock<IJsonSerializerService>(),
            factory,
            new HttpContextAccessor());

        // Act
        var action = () => sut.PublishMessageAsync(new CorePublisherNoAttributeEvent(), CancellationToken.None);

        // Assert
        var exception = await action.Should().ThrowAsync<InvalidOperationException>();
        exception.Which.InnerException.Should().BeOfType<InvalidOperationException>();
        factory.Verify(x => x.CreateAsync(It.IsAny<PubSubTopicName>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task PublishMessageAsync_Should_ThrowWrappedException_When_TopicKeyIsNotConfigured()
    {
        // Arrange
        var sut = CreateSut(
            new Mock<IJsonSerializerService>(),
            new Mock<IGcpPubSubPublisherClientFactory>(),
            new HttpContextAccessor(),
            new QueueInfoOptions());

        // Act
        var action = () => sut.PublishMessageAsync(new CorePublisherSampleEvent(), CancellationToken.None);

        // Assert
        var exception = await action.Should().ThrowAsync<InvalidOperationException>();
        exception.Which.InnerException.Should().BeOfType<InvalidOperationException>();
    }

    private static GcpPublisherService CreateSut(
        Mock<IJsonSerializerService> serializer,
        Mock<IGcpPubSubPublisherClientFactory> factory,
        IHttpContextAccessor accessor,
        QueueInfoOptions queueOptions = null) =>
        new(
            Mock.Of<ILogger<GcpPublisherService>>(),
            serializer.Object,
            Options.Create(new GcpOptions
            {
                ProjectId = "project-id"
            }),
            queueOptions ?? new QueueInfoOptions
            {
                Topics =
                {
                    ["core-topic"] = "topic-id"
                }
            },
            accessor,
            factory.Object);

    private static IHttpContextAccessor CreateHttpContextAccessor(string correlationId)
    {
        var context = new DefaultHttpContext();
        context.Items[X_CORRELATION_ID] = correlationId;

        return new HttpContextAccessor
        {
            HttpContext = context
        };
    }
}
