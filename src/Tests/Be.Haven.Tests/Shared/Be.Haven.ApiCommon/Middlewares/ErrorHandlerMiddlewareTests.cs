using Microsoft.AspNetCore.Http.Features;

namespace Be.Haven.Tests.Shared.Be.Haven.ApiCommon.Middlewares;

public sealed class ErrorHandlerMiddlewareTests
{
    [Theory]
    [MemberData(nameof(ExceptionCases))]
    public async Task Invoke_Should_WriteStandardErrorEnvelope_When_NextThrows(
        Exception exception,
        int expectedStatusCode,
        string expectedCode,
        string expectedMessage)
    {
        // Arrange
        var context = CreateContext();
        var sut = CreateSut(_ => throw exception);

        // Act
        await sut.Invoke(context);

        // Assert
        var response = await ReadResponseAsync(context);
        context.Response.StatusCode.Should().Be(expectedStatusCode);
        context.Response.ContentType.Should().Contain(TEXT_JSON);
        response.Success.Should().BeFalse();
        response.Error.Code.Should().Be(expectedCode);
        response.Error.Message.Should().Be(expectedMessage);
        response.Meta.CorrelationId.Should().Be("correlation-1");
    }

    [Fact]
    public async Task Invoke_Should_NotWriteResponse_When_RequestWasCanceled()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();
        var context = CreateContext();
        context.RequestAborted = cancellationTokenSource.Token;
        var sut = CreateSut(_ => throw new OperationCanceledException(cancellationTokenSource.Token));

        // Act
        await sut.Invoke(context);

        // Assert
        context.Response.Body.Length.Should().Be(0);
    }

    [Fact]
    public async Task Invoke_Should_NotRewriteResponse_When_ResponseAlreadyStarted()
    {
        // Arrange
        var context = CreateContext();
        context.Features.Set<IHttpResponseFeature>(new StartedResponseFeature(context.Response.Body));
        var sut = CreateSut(async currentContext =>
        {
            throw new InvalidOperationException("after response started");
        });

        // Act
        await sut.Invoke(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        context.Response.Body.Length.Should().Be(0);
    }

    [Fact]
    public async Task Invoke_Should_SetVersionMetadata_When_ApiVersionFeatureExists()
    {
        // Arrange
        var context = CreateContext();
        var feature = new Mock<IApiVersioningFeature>();
        feature.SetupGet(x => x.RequestedApiVersion).Returns(new ApiVersion(1, 2));
        context.Features.Set(feature.Object);
        var sut = CreateSut(_ => throw new ArgumentException("bad argument"));

        // Act
        await sut.Invoke(context);

        // Assert
        var response = await ReadResponseAsync(context);
        response.Meta.Version.Should().Be("1.2");
    }

    [Fact]
    public async Task Invoke_Should_NotWriteResponse_When_RequestIsAlreadyCanceledBeforeErrorEnvelopeWrite()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();
        var context = CreateContext();
        context.RequestAborted = cancellationTokenSource.Token;
        var sut = CreateSut(_ => throw new ArgumentException("bad argument"));

        // Act
        await sut.Invoke(context);

        // Assert
        context.Response.Body.Length.Should().Be(0);
    }

    [Fact]
    public async Task Invoke_Should_LogAndContinue_When_ResponseBodyThrowsDuringErrorEnvelopeWrite()
    {
        // Arrange
        var context = CreateContext();
        context.Response.Body = new ThrowingWriteStream();
        var sut = CreateSut(_ => throw new ArgumentException("bad argument"));

        // Act
        var action = async () => await sut.Invoke(context);

        // Assert
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Invoke_Should_LogAndContinue_When_RequestIsCanceledDuringErrorEnvelopeWrite()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var context = CreateContext();
        context.RequestAborted = cancellationTokenSource.Token;
        context.Response.Body = new CancelingWriteStream(cancellationTokenSource);
        var sut = CreateSut(_ => throw new ArgumentException("bad argument"));

        // Act
        var action = async () => await sut.Invoke(context);

        // Assert
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Invoke_Should_WriteApiExceptionDetails_When_PublicSafeDetailsAreProvided()
    {
        // Arrange
        var details = new List<ErrorDetailDto>
        {
            new()
            {
                Field = "action",
                Issue = "selection_required"
            }
        };
        var context = CreateContext();
        var sut = CreateSut(_ => throw new ApiException(
            "Selection is required",
            "error_selection_required",
            StatusCodes.Status400BadRequest,
            details));

        // Act
        await sut.Invoke(context);

        // Assert
        var response = await ReadResponseAsync(context);
        response.Error.Details.Should().ContainSingle(detail =>
            detail.Field == "action"
            && detail.Issue.ToString() == "selection_required");
    }

    public static TheoryData<Exception, int, string, string> ExceptionCases()
    {
        return new TheoryData<Exception, int, string, string>
        {
            {
                new ApiException("domain failed", "error_domain", StatusCodes.Status400BadRequest),
                StatusCodes.Status400BadRequest,
                "error_domain",
                "domain failed"
            },
            {
                new DistributedLockUnavailableException(new InvalidOperationException("redis detail")),
                StatusCodes.Status503ServiceUnavailable,
                SERVICE_UNAVAILABLE,
                SERVICE_UNAVAILABLE_MESSAGE
            },
            {
                new ArgumentException("bad argument"),
                StatusCodes.Status400BadRequest,
                BAD_REQUEST,
                "bad argument"
            },
            {
                new ValidationException(
                [
                    new ValidationFailure("Email", "Email is invalid.")
                ]),
                StatusCodes.Status400BadRequest,
                BAD_REQUEST,
                "Email is invalid."
            },
            {
                new KeyNotFoundException("not found"),
                StatusCodes.Status404NotFound,
                NOT_FOUND,
                "not found"
            },
            {
                new UnauthorizedAccessException("no access"),
                StatusCodes.Status401Unauthorized,
                UNAUTHORIZED,
                "no access"
            },
            {
                new HttpStatusCodeException("too many", MANY_REQUESTS, StatusCodes.Status429TooManyRequests),
                StatusCodes.Status429TooManyRequests,
                MANY_REQUESTS,
                "too many"
            },
            {
                new HttpStatusCodeException("too many default", null, StatusCodes.Status429TooManyRequests),
                StatusCodes.Status429TooManyRequests,
                MANY_REQUESTS,
                "too many default"
            },
            {
                new HttpStatusCodeException("bad request", null, StatusCodes.Status400BadRequest),
                StatusCodes.Status400BadRequest,
                BAD_REQUEST,
                "bad request"
            },
            {
                new HttpStatusCodeException("unauthorized", null, StatusCodes.Status401Unauthorized),
                StatusCodes.Status401Unauthorized,
                UNAUTHORIZED,
                "unauthorized"
            },
            {
                new HttpStatusCodeException("forbidden", null, StatusCodes.Status403Forbidden),
                StatusCodes.Status403Forbidden,
                FORBIDDEN,
                "forbidden"
            },
            {
                new HttpStatusCodeException("missing", null, StatusCodes.Status404NotFound),
                StatusCodes.Status404NotFound,
                NOT_FOUND,
                "missing"
            },
            {
                new HttpStatusCodeException("method", null, StatusCodes.Status405MethodNotAllowed),
                StatusCodes.Status405MethodNotAllowed,
                METHOD_NOT_ALLOWED,
                "method"
            },
            {
                new HttpStatusCodeException("not acceptable", null, StatusCodes.Status406NotAcceptable),
                StatusCodes.Status406NotAcceptable,
                NOT_ACCEPTABLE,
                "not acceptable"
            },
            {
                new HttpStatusCodeException("large", null, StatusCodes.Status413PayloadTooLarge),
                StatusCodes.Status413PayloadTooLarge,
                BAD_REQUEST,
                "large"
            },
            {
                new HttpStatusCodeException("media", null, StatusCodes.Status415UnsupportedMediaType),
                StatusCodes.Status415UnsupportedMediaType,
                UNSUPPORTED_MEDIA_TYPE,
                "media"
            },
            {
                new HttpStatusCodeException("dependency", null, StatusCodes.Status503ServiceUnavailable),
                StatusCodes.Status503ServiceUnavailable,
                SERVICE_UNAVAILABLE,
                "dependency"
            },
            {
                new HttpStatusCodeException("internal", null, StatusCodes.Status500InternalServerError),
                StatusCodes.Status500InternalServerError,
                INTERNAL_SERVER,
                "internal"
            },
            {
                new HttpStatusCodeException("unknown", null, StatusCodes.Status418ImATeapot),
                StatusCodes.Status418ImATeapot,
                INTERNAL_SERVER,
                "unknown"
            },
            {
                new InvalidOperationException("internal detail"),
                StatusCodes.Status500InternalServerError,
                INTERNAL_SERVER,
                UNEXPECTED_SERVER_ERROR
            }
        };
    }

    private static ErrorHandlerMiddleware CreateSut(RequestDelegate next)
    {
        return new ErrorHandlerMiddleware(
            next,
            Mock.Of<ILogger<ErrorHandlerMiddleware>>());
    }

    private static DefaultHttpContext CreateContext()
    {
        var context = new DefaultHttpContext();
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

    private sealed class StartedResponseFeature : IHttpResponseFeature
    {
        public StartedResponseFeature(Stream body)
        {
            Body = body;
        }

        public int StatusCode { get; set; } = StatusCodes.Status200OK;

        public string ReasonPhrase { get; set; }

        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();

        public Stream Body { get; set; }

        public bool HasStarted => true;

        public void OnCompleted(Func<object, Task> callback, object state)
        {
        }

        public void OnStarting(Func<object, Task> callback, object state)
        {
        }
    }

    private sealed class ThrowingWriteStream : MemoryStream
    {
        public override ValueTask WriteAsync(
            ReadOnlyMemory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            throw new IOException("write failed");
        }
    }

    private sealed class CancelingWriteStream : MemoryStream
    {
        private readonly CancellationTokenSource _cancellationTokenSource;

        public CancelingWriteStream(CancellationTokenSource cancellationTokenSource)
        {
            _cancellationTokenSource = cancellationTokenSource;
        }

        public override ValueTask WriteAsync(
            ReadOnlyMemory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            _cancellationTokenSource.Cancel();
            throw new OperationCanceledException(_cancellationTokenSource.Token);
        }
    }
}
