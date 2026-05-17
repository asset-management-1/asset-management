namespace Be.Haven.Tests.Shared.Be.Haven.ApiCommon.Middlewares;

public sealed class CorrelationHandlerMiddlewareTests
{
    [Fact]
    public async Task Invoke_Should_GenerateCorrelationId_When_HeaderIsMissing()
    {
        // Arrange
        var wasNextCalled = false;
        var context = new DefaultHttpContext();
        var sut = new CorrelationHandlerMiddleware(_ =>
        {
            wasNextCalled = true;
            return Task.CompletedTask;
        });

        // Act
        await sut.Invoke(context);

        // Assert
        wasNextCalled.Should().BeTrue();
        context.Items[X_CORRELATION_ID].Should().NotBeNull();
        context.Request.Headers[X_CORRELATION_ID].ToString().Should().Be(context.Items[X_CORRELATION_ID].ToString());
        context.Response.Headers[X_CORRELATION_ID].ToString().Should().Be(context.Items[X_CORRELATION_ID].ToString());
        context.Items[REQUEST_TIMESTAMP].Should().BeOfType<DateTime>();
    }

    [Fact]
    public async Task Invoke_Should_PreserveCorrelationId_When_HeaderExists()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers[X_CORRELATION_ID] = "existing-correlation";
        var sut = new CorrelationHandlerMiddleware(_ => Task.CompletedTask);

        // Act
        await sut.Invoke(context);

        // Assert
        context.Items[X_CORRELATION_ID].Should().Be("existing-correlation");
        context.Response.Headers[X_CORRELATION_ID].ToString().Should().Be("existing-correlation");
    }

    [Fact]
    public async Task Invoke_Should_AddActivityTagsAndBaggage_When_ActivityExists()
    {
        // Arrange
        using var activitySource = new ActivitySource("haven-tests");
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "haven-tests",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded
        };
        ActivitySource.AddActivityListener(listener);
        using var activity = activitySource.StartActivity("request");
        var context = new DefaultHttpContext();
        context.Request.Headers[X_CORRELATION_ID] = "activity-correlation";
        var sut = new CorrelationHandlerMiddleware(_ => Task.CompletedTask);

        // Act
        await sut.Invoke(context);

        // Assert
        activity.GetTagItem(OTEL_CORRELATION_ID_TAG).Should().Be("activity-correlation");
        activity.GetBaggageItem(OTEL_CORRELATION_ID_TAG).Should().Be("activity-correlation");
        activity.GetTagItem(OTEL_REQUEST_TIMESTAMP_TAG).Should().NotBeNull();
    }
}
