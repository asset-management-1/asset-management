namespace Be.Haven.Tests.Shared.Be.Haven.Core.Handlers;

public sealed class CorrelationIdHandlerTests
{
    [Fact]
    public async Task SendAsync_Should_AddCorrelationHeader_When_HttpContextContainsCorrelationId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Items[X_CORRELATION_ID] = "correlation-from-context";
        var fakeHandler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK));
        using var sut = CreateSut(context, fakeHandler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test/resource");

        // Act
        await sut.SendAsync(request, CancellationToken.None);

        // Assert
        fakeHandler.LastRequest.Headers.GetValues(X_CORRELATION_ID)
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .Be("correlation-from-context");
    }

    [Fact]
    public async Task SendAsync_Should_PreserveExistingCorrelationHeader_When_RequestAlreadyHasHeader()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Items[X_CORRELATION_ID] = "correlation-from-context";
        var fakeHandler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK));
        using var sut = CreateSut(context, fakeHandler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test/resource");
        request.Headers.Add(X_CORRELATION_ID, "caller-correlation");

        // Act
        await sut.SendAsync(request, CancellationToken.None);

        // Assert
        fakeHandler.LastRequest.Headers.GetValues(X_CORRELATION_ID)
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .Be("caller-correlation");
    }

    [Fact]
    public async Task SendAsync_Should_AddCorrelationHeaderFromActivity_When_HttpContextIsMissing()
    {
        // Arrange
        using var activity = new Activity("correlation-test");
        activity.AddBaggage(OTEL_CORRELATION_ID_TAG, "correlation-from-activity");
        activity.Start();
        var fakeHandler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK));
        using var sut = CreateSut(null, fakeHandler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test/resource");

        // Act
        await sut.SendAsync(request, CancellationToken.None);

        // Assert
        fakeHandler.LastRequest.Headers.GetValues(X_CORRELATION_ID)
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .Be("correlation-from-activity");
    }

    private static HttpMessageInvoker CreateSut(
        HttpContext context,
        HttpMessageHandler innerHandler)
    {
        var handler = new CorrelationIdHandler(new HttpContextAccessor
        {
            HttpContext = context
        })
        {
            InnerHandler = innerHandler
        };

        return new HttpMessageInvoker(handler);
    }
}
