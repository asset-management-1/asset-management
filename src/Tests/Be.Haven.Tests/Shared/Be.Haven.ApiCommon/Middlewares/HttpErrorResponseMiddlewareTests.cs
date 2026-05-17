using System.Reflection;

namespace Be.Haven.Tests.Shared.Be.Haven.ApiCommon.Middlewares;

public sealed class HttpErrorResponseMiddlewareTests
{
    [Theory]
    [InlineData(StatusCodes.Status405MethodNotAllowed, METHOD_NOT_ALLOWED, MSG_METHOD_NOT_ALLOWED)]
    [InlineData(StatusCodes.Status406NotAcceptable, NOT_ACCEPTABLE, MSG_NOT_ACCEPTABLE)]
    [InlineData(StatusCodes.Status413PayloadTooLarge, BAD_REQUEST, MSG_PAYLOAD_TOO_LARGE)]
    [InlineData(StatusCodes.Status415UnsupportedMediaType, UNSUPPORTED_MEDIA_TYPE, MSG_UNSUPPORTED_MEDIA_TYPE)]
    [InlineData(StatusCodes.Status429TooManyRequests, MANY_REQUESTS, MSG_TOO_MANY_REQUESTS)]
    public async Task Invoke_Should_WriteStandardEnvelope_When_ResponseHasSelectedFrameworkStatus(
        int statusCode,
        string expectedCode,
        string expectedMessage)
    {
        // Arrange
        var context = CreateContext("/api/v1/user");
        var sut = CreateSut(nextContext =>
        {
            nextContext.Response.StatusCode = statusCode;
            return Task.CompletedTask;
        });

        // Act
        await sut.Invoke(context);

        // Assert
        var response = await ReadResponseAsync(context);
        context.Response.StatusCode.Should().Be(statusCode);
        response.Error.Code.Should().Be(expectedCode);
        response.Error.Message.Should().Be(expectedMessage);
        response.Meta.CorrelationId.Should().Be("correlation-1");
    }

    [Fact]
    public async Task Invoke_Should_ReturnVersionUnsupported_When_RequestedVersionIsNotSupported()
    {
        // Arrange
        var context = CreateContext("/api/v99/user/user-info");
        var sut = CreateSut(nextContext =>
        {
            nextContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        });

        // Act
        await sut.Invoke(context);

        // Assert
        var response = await ReadResponseAsync(context);
        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        response.Error.Code.Should().Be(API_VERSION_NOT_SUPPORTED);
        response.Error.Message.Should().Contain("/api/v99/user/user-info");
    }

    [Fact]
    public async Task Invoke_Should_ReturnVersionUnsupported_When_RequestedVersionSegmentIsUppercase()
    {
        // Arrange
        var context = CreateContext("/api/V99/user/user-info");
        var sut = CreateSut(nextContext =>
        {
            nextContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        });

        // Act
        await sut.Invoke(context);

        // Assert
        var response = await ReadResponseAsync(context);
        response.Error.Code.Should().Be(API_VERSION_NOT_SUPPORTED);
    }

    [Fact]
    public async Task Invoke_Should_SetVersionMetadata_When_ApiVersionFeatureExists()
    {
        // Arrange
        var context = CreateContext("/api/v1/missing");
        var feature = new Mock<IApiVersioningFeature>();
        feature.SetupGet(x => x.RequestedApiVersion).Returns(new ApiVersion(1, 2));
        context.Features.Set(feature.Object);
        var sut = CreateSut(nextContext =>
        {
            nextContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        });

        // Act
        await sut.Invoke(context);

        // Assert
        var response = await ReadResponseAsync(context);
        response.Meta.Version.Should().Be("1.2");
    }

    [Fact]
    public async Task Invoke_Should_ReturnNotFoundEnvelope_When_PathHasNoUnsupportedApiVersion()
    {
        // Arrange
        var context = CreateContext("/api/v1/missing");
        var sut = CreateSut(nextContext =>
        {
            nextContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        });

        // Act
        await sut.Invoke(context);

        // Assert
        var response = await ReadResponseAsync(context);
        response.Error.Code.Should().Be(NOT_FOUND);
        response.Error.Message.Should().Be(MSG_NOT_FOUND);
    }

    [Fact]
    public async Task Invoke_Should_NotWriteEnvelope_When_NotFoundComesFromMatchedEndpoint()
    {
        // Arrange
        var context = CreateContext("/api/v1/user");
        context.SetEndpoint(new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(), "matched"));
        var sut = CreateSut(nextContext =>
        {
            nextContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        });

        // Act
        await sut.Invoke(context);

        // Assert
        context.Response.Body.Length.Should().Be(0);
    }

    [Fact]
    public async Task Invoke_Should_NotWriteEnvelope_When_ResponseAlreadyHasContentLength()
    {
        // Arrange
        var context = CreateContext("/api/v1/user");
        var sut = CreateSut(nextContext =>
        {
            nextContext.Response.StatusCode = StatusCodes.Status404NotFound;
            nextContext.Response.ContentLength = 12;
            return Task.CompletedTask;
        });

        // Act
        await sut.Invoke(context);

        // Assert
        context.Response.Body.Length.Should().Be(0);
    }

    [Theory]
    [InlineData("")]
    [InlineData("/api/user")]
    [InlineData("/api/v/user")]
    [InlineData("/api/vx/user")]
    [InlineData("/api/v1x/user")]
    public async Task Invoke_Should_ReturnNotFoundEnvelope_When_PathHasNoValidVersionSegment(string path)
    {
        // Arrange
        var context = CreateContext(path);
        var sut = CreateSut(nextContext =>
        {
            nextContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        });

        // Act
        await sut.Invoke(context);

        // Assert
        var response = await ReadResponseAsync(context);
        response.Error.Code.Should().Be(NOT_FOUND);
    }

    [Fact]
    public async Task Invoke_Should_NotWriteEnvelope_When_PathIsSwagger()
    {
        // Arrange
        var context = CreateContext("/swagger/index.html");
        var sut = CreateSut(nextContext =>
        {
            nextContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        });

        // Act
        await sut.Invoke(context);

        // Assert
        context.Response.Body.Length.Should().Be(0);
    }

    [Fact]
    public void MapStatus_Should_ReturnInternalServerFallback_When_StatusIsNotMapped()
    {
        // Arrange
        var sut = CreateSut(_ => Task.CompletedTask);
        var method = typeof(HttpErrorResponseMiddleware).GetMethod(
            "MapStatus",
            BindingFlags.Instance | BindingFlags.NonPublic);

        // Act
        var result = ((string Code, string Message))method.Invoke(
            sut,
            [StatusCodes.Status500InternalServerError, "/api/v1/error"]);

        // Assert
        result.Code.Should().Be(INTERNAL_SERVER);
        result.Message.Should().Be(MSG_REQUEST_FAILED);
    }

    private static HttpErrorResponseMiddleware CreateSut(RequestDelegate next)
    {
        var versionProvider = new Mock<IApiVersionDescriptionProvider>();
        versionProvider
            .SetupGet(x => x.ApiVersionDescriptions)
            .Returns(new[]
            {
                new ApiVersionDescription(new ApiVersion(1, 0), "v1", deprecated: false)
            });

        return new HttpErrorResponseMiddleware(
            next,
            Mock.Of<ILogger<HttpErrorResponseMiddleware>>(),
            versionProvider.Object);
    }

    private static DefaultHttpContext CreateContext(string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.TraceIdentifier = "request-1";
        context.Items[X_CORRELATION_ID] = "correlation-1";
        context.Items[REQUEST_TIMESTAMP] = new DateTime(2026, 5, 17, 1, 2, 3, DateTimeKind.Utc);
        context.Response.Body = new MemoryStream();

        return context;
    }

    private static async Task<ResponseDto<string>> ReadResponseAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();

        return System.Text.Json.JsonSerializer.Deserialize<ResponseDto<string>>(
            body,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}
