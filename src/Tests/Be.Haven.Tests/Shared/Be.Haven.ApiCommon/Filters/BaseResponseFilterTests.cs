namespace Be.Haven.Tests.Shared.Be.Haven.ApiCommon.Filters;

public sealed class BaseResponseFilterTests
{
    [Fact]
    public async Task OnResultExecutionAsync_Should_PopulateMeta_When_ResultIsResponseDto()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.TraceIdentifier = "request-1";
        httpContext.Items[X_CORRELATION_ID] = "correlation-1";
        httpContext.Items[REQUEST_TIMESTAMP] = new DateTime(2026, 5, 17, 1, 2, 3, DateTimeKind.Utc);
        var response = new ResponseDto<string>("ok");
        var result = new ObjectResult(response);
        var context = new ResultExecutingContext(
            new ActionContext(httpContext, new RouteData(), new ActionDescriptor()),
            [],
            result,
            controller: null);
        var sut = new BaseResponseFilter();
        var wasNextCalled = false;

        // Act
        await sut.OnResultExecutionAsync(
            context,
            () =>
            {
                wasNextCalled = true;
                return Task.FromResult(new ResultExecutedContext(context, [], result, null));
            });

        // Assert
        wasNextCalled.Should().BeTrue();
        response.Meta.RequestId.Should().Be("request-1");
        response.Meta.CorrelationId.Should().Be("correlation-1");
        response.Meta.RequestTimestamp.Should().Be(new DateTime(2026, 5, 17, 1, 2, 3, DateTimeKind.Utc));
        response.Meta.ResponseTimestamp.Should().BeAfter(response.Meta.RequestTimestamp.Value);
    }

    [Fact]
    public async Task OnResultExecutionAsync_Should_OnlyCallNext_When_ResultIsNotResponseDto()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var result = new ObjectResult(new { Name = "plain" });
        var context = new ResultExecutingContext(
            new ActionContext(httpContext, new RouteData(), new ActionDescriptor()),
            [],
            result,
            controller: null);
        var sut = new BaseResponseFilter();
        var wasNextCalled = false;

        // Act
        await sut.OnResultExecutionAsync(
            context,
            () =>
            {
                wasNextCalled = true;
                return Task.FromResult(new ResultExecutedContext(context, [], result, null));
            });

        // Assert
        wasNextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task OnResultExecutionAsync_Should_CreateMeta_When_ResponseDtoMetaIsNull()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.TraceIdentifier = "request-2";
        httpContext.Items[X_CORRELATION_ID] = "correlation-2";
        httpContext.Items[REQUEST_TIMESTAMP] = new DateTime(2026, 5, 17, 2, 0, 0, DateTimeKind.Utc);
        var response = new ResponseDto<string>("ok")
        {
            Meta = null
        };
        var result = new ObjectResult(response);
        var context = new ResultExecutingContext(
            new ActionContext(httpContext, new RouteData(), new ActionDescriptor()),
            [],
            result,
            controller: null);
        var sut = new BaseResponseFilter();

        // Act
        await sut.OnResultExecutionAsync(
            context,
            () => Task.FromResult(new ResultExecutedContext(context, [], result, null)));

        // Assert
        response.Meta.Should().NotBeNull();
        response.Meta.RequestId.Should().Be("request-2");
    }

    [Fact]
    public async Task OnResultExecutionAsync_Should_SetVersion_When_RequestFeatureHasVersion()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.TraceIdentifier = "request-3";
        httpContext.Items[X_CORRELATION_ID] = "correlation-3";
        httpContext.Items[REQUEST_TIMESTAMP] = new DateTime(2026, 5, 17, 3, 0, 0, DateTimeKind.Utc);
        httpContext.Features.Set(CreateVersioningFeature(new ApiVersion(2, 1)));
        var response = new ResponseDto<string>("ok");
        var result = new ObjectResult(response);
        var context = new ResultExecutingContext(
            new ActionContext(httpContext, new RouteData(), new ActionDescriptor()),
            [],
            result,
            controller: null);
        var sut = new BaseResponseFilter();

        // Act
        await sut.OnResultExecutionAsync(
            context,
            () => Task.FromResult(new ResultExecutedContext(context, [], result, null)));

        // Assert
        response.Meta.Version.Should().Be("2.1");
    }

    private static IApiVersioningFeature CreateVersioningFeature(ApiVersion version)
    {
        var feature = new Mock<IApiVersioningFeature>();
        feature.SetupGet(x => x.RequestedApiVersion).Returns(version);

        return feature.Object;
    }
}
