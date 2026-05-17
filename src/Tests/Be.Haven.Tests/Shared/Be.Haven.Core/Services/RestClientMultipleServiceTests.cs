using Polly.CircuitBreaker;

namespace Be.Haven.Tests.Shared.Be.Haven.Core.Services;

public sealed class RestClientMultipleServiceTests
{
    [Fact]
    public async Task GetAsync_Should_SendQueryHeadersAndAuthorization_When_RequestIsConfigured()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK));
        var client = new HttpClient(handler);
        var factory = new FakeHttpClientFactory(client);
        var request = new BaseHttpRequest
        {
            HttpClientName = "external-api",
            BaseAddress = "https://example.test/api",
            RequestUri = "users",
            AuthenticationValue = new AuthenticationHeaderValue("Bearer", "token"),
            AdditionalHeaders = new Dictionary<string, string>
            {
                ["X-Trace"] = "trace-1"
            },
            AdditionalQueryParams = new Dictionary<string, string>
            {
                ["search"] = "hello world"
            }
        };
        var sut = CreateSut(factory);

        // Act
        var result = await sut.GetAsync(request);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        factory.LastClientName.Should().Be("external-api");
        handler.LastRequest.Method.Should().Be(HttpMethod.Get);
        handler.LastRequest.RequestUri.Query.Should().Be("?search=hello%20world");
        handler.LastRequest.Headers.Authorization.Scheme.Should().Be("Bearer");
        handler.LastRequest.Headers.Authorization.Parameter.Should().Be("token");
        handler.LastRequest.Headers.GetValues("X-Trace").Should().ContainSingle().Which.Should().Be("trace-1");
    }

    [Fact]
    public async Task PostAsync_Should_SendJsonPayload_When_ContentTypeIsJson()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.Created));
        var factory = new FakeHttpClientFactory(new HttpClient(handler));
        var request = new BaseHttpRequest
        {
            HttpClientName = "external-api",
            BaseAddress = "https://example.test",
            RequestUri = "submit",
            AuthenticationValue = new AuthenticationHeaderValue("Bearer", "token"),
            ContentType = TEXT_JSON,
            RequestData = """{"name":"Haven"}"""
        };
        var sut = CreateSut(factory);

        // Act
        var result = await sut.PostAsync(request);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        handler.LastRequest.Method.Should().Be(HttpMethod.Post);
        handler.LastRequest.Headers.Authorization.Parameter.Should().Be("token");
        handler.LastContentType.Should().Be(TEXT_JSON);
        handler.LastContentText.Should().Be("""{"name":"Haven"}""");
    }

    [Fact]
    public async Task PostAsync_Should_SendXmlPayload_When_ContentTypeIsTextXml()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK));
        var factory = new FakeHttpClientFactory(new HttpClient(handler));
        var request = new BaseHttpRequest
        {
            RequestUri = "https://example.test/xml",
            ContentType = TEXT_XML,
            RequestData = "<root />"
        };
        var sut = CreateSut(factory);

        // Act
        var result = await sut.PostAsync(request);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        handler.LastContentType.Should().Be(TEXT_XML);
        handler.LastContentText.Should().Be("<root />");
    }

    [Fact]
    public async Task PostAsync_Should_ReturnGatewayTimeout_When_HttpRequestFails()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(new HttpRequestException("network failed"));
        var factory = new FakeHttpClientFactory(new HttpClient(handler));
        var request = new BaseHttpRequest
        {
            HttpClientName = "external-api",
            BaseAddress = "https://example.test",
            RequestUri = "submit",
            ContentType = TEXT_JSON,
            RequestData = "{}"
        };
        var sut = CreateSut(factory);

        // Act
        var result = await sut.PostAsync(request);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.GatewayTimeout);
        (await result.Content.ReadAsStringAsync()).Should().Contain("network failed");
    }

    [Fact]
    public async Task PostAsync_Should_ReturnGatewayTimeout_When_RequestTimesOut()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(new TaskCanceledException("request cancelled"));
        var factory = new FakeHttpClientFactory(new HttpClient(handler));
        var request = new BaseHttpRequest
        {
            BaseAddress = "https://example.test",
            RequestUri = "submit",
            ContentType = TEXT_JSON,
            RequestData = "{}"
        };
        var sut = CreateSut(factory);

        // Act
        var result = await sut.PostAsync(request);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.GatewayTimeout);
        (await result.Content.ReadAsStringAsync()).Should().Contain("request cancelled");
    }

    [Fact]
    public async Task PostAsync_Should_ReturnServiceUnavailable_When_CircuitIsOpen()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(new BrokenCircuitException("circuit open"));
        var factory = new FakeHttpClientFactory(new HttpClient(handler));
        var request = new BaseHttpRequest
        {
            BaseAddress = "https://example.test",
            RequestUri = "submit",
            ContentType = TEXT_JSON,
            RequestData = "{}"
        };
        var sut = CreateSut(factory);

        // Act
        var result = await sut.PostAsync(request);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        (await result.Content.ReadAsStringAsync()).Should().Contain("circuit open");
    }

    [Fact]
    public async Task PostAsync_Should_ReturnInternalServerError_When_UnexpectedExceptionOccurs()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(new InvalidOperationException("unexpected"));
        var factory = new FakeHttpClientFactory(new HttpClient(handler));
        var request = new BaseHttpRequest
        {
            BaseAddress = "https://example.test",
            RequestUri = "submit",
            ContentType = TEXT_JSON,
            RequestData = "{}"
        };
        var sut = CreateSut(factory);

        // Act
        var result = await sut.PostAsync(request);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        (await result.Content.ReadAsStringAsync()).Should().Contain("unexpected");
    }

    [Fact]
    public async Task PostAsync_Should_SendFormUrlEncodedPayload_When_ContentTypeIsFormUrlEncoded()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK));
        var factory = new FakeHttpClientFactory(new HttpClient(handler));
        var request = new BaseHttpRequest
        {
            BaseAddress = "https://example.test",
            RequestUri = "token",
            ContentType = APPLICATION_FORM_URLENCODED,
            RequestFormData =
            [
                new KeyValuePair<string, string>("grant_type", "client credentials"),
                new KeyValuePair<string, string>("scope", "profile")
            ]
        };
        var sut = CreateSut(factory);

        // Act
        var result = await sut.PostAsync(request);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        handler.LastContentType.Should().Be(APPLICATION_FORM_URLENCODED);
        handler.LastContentText.Should().Contain("grant_type=client+credentials");
        handler.LastContentText.Should().Contain("scope=profile");
    }

    [Fact]
    public async Task PostAsync_Should_SendMultipartFile_When_ContentTypeIsMultipart()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK));
        var factory = new FakeHttpClientFactory(new HttpClient(handler));
        var file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("file-content")), 0, 12, "file", "avatar.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };
        var request = new BaseHttpRequest
        {
            BaseAddress = "https://example.test",
            RequestUri = "upload",
            ContentType = MULTIPART_FORM_DATA,
            File = file
        };
        var sut = CreateSut(factory);

        // Act
        var result = await sut.PostAsync(request);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        handler.LastContentType.Should().Be(MULTIPART_FORM_DATA);
        handler.LastContentText.Should().Contain("avatar.jpg");
        handler.LastContentText.Should().Contain("file-content");
    }

    [Fact]
    public async Task PostAsync_Should_SendEmptyMultipartPayload_When_FileIsMissing()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK));
        var factory = new FakeHttpClientFactory(new HttpClient(handler));
        var request = new BaseHttpRequest
        {
            RequestUri = "https://example.test/upload",
            ContentType = MULTIPART_FORM_DATA,
            File = null
        };
        var sut = CreateSut(factory);

        // Act
        var result = await sut.PostAsync(request);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        handler.LastContentType.Should().Be(MULTIPART_FORM_DATA);
    }

    [Fact]
    public async Task PostAsync_Should_SendStreamPayload_When_RequestStreamIsProvided()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.Accepted));
        var factory = new FakeHttpClientFactory(new HttpClient(handler));
        var request = new BaseHttpRequest
        {
            BaseAddress = "https://example.test",
            RequestUri = "objects/file.jpg",
            ContentType = "image/jpeg",
            RequestStream = new MemoryStream(Encoding.UTF8.GetBytes("binary-body"))
        };
        var sut = CreateSut(factory);

        // Act
        var result = await sut.PostAsync(request);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Accepted);
        handler.LastContentType.Should().Be("image/jpeg");
        handler.LastContentText.Should().Be("binary-body");
    }

    [Fact]
    public async Task PutAsync_Should_SendJsonPayload_When_RequestIsConfigured()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.Accepted));
        var factory = new FakeHttpClientFactory(new HttpClient(handler));
        var request = new BaseHttpRequest
        {
            HttpClientName = "external-api",
            BaseAddress = "https://example.test/api",
            RequestUri = "profiles/1",
            ContentType = TEXT_JSON,
            RequestData = """{"name":"Updated"}""",
            AuthenticationValue = new AuthenticationHeaderValue("Bearer", "token"),
            AdditionalHeaders = new Dictionary<string, string>
            {
                ["X-Trace"] = "trace-2"
            }
        };
        var sut = CreateSut(factory);

        // Act
        var result = await sut.PutAsync(request);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Accepted);
        factory.LastClientName.Should().Be("external-api");
        handler.LastRequest.Method.Should().Be(HttpMethod.Put);
        handler.LastRequest.Headers.Authorization.Parameter.Should().Be("token");
        handler.LastRequest.Headers.GetValues("X-Trace").Should().ContainSingle().Which.Should().Be("trace-2");
        handler.LastContentType.Should().Be(TEXT_JSON);
        handler.LastContentText.Should().Be("""{"name":"Updated"}""");
    }

    [Fact]
    public async Task PutAsync_Should_ReturnGatewayTimeout_When_HttpRequestFails()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(new HttpRequestException("network failed"));
        var factory = new FakeHttpClientFactory(new HttpClient(handler));
        var request = new BaseHttpRequest
        {
            BaseAddress = "https://example.test",
            RequestUri = "profiles/1",
            ContentType = TEXT_JSON,
            RequestData = "{}"
        };
        var sut = CreateSut(factory);

        // Act
        var result = await sut.PutAsync(request);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.GatewayTimeout);
        (await result.Content.ReadAsStringAsync()).Should().Contain("network failed");
    }

    [Fact]
    public async Task PutAsync_Should_ReturnGatewayTimeout_When_RequestTimesOut()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(new TaskCanceledException("request cancelled"));
        var factory = new FakeHttpClientFactory(new HttpClient(handler));
        var request = new BaseHttpRequest
        {
            BaseAddress = "https://example.test",
            RequestUri = "profiles/1",
            ContentType = TEXT_JSON,
            RequestData = "{}"
        };
        var sut = CreateSut(factory);

        // Act
        var result = await sut.PutAsync(request);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.GatewayTimeout);
        (await result.Content.ReadAsStringAsync()).Should().Contain("request cancelled");
    }

    [Fact]
    public async Task PutAsync_Should_ReturnServiceUnavailable_When_CircuitIsOpen()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(new BrokenCircuitException("circuit open"));
        var factory = new FakeHttpClientFactory(new HttpClient(handler));
        var request = new BaseHttpRequest
        {
            BaseAddress = "https://example.test",
            RequestUri = "profiles/1",
            ContentType = TEXT_JSON,
            RequestData = "{}"
        };
        var sut = CreateSut(factory);

        // Act
        var result = await sut.PutAsync(request);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        (await result.Content.ReadAsStringAsync()).Should().Contain("circuit open");
    }

    [Fact]
    public async Task PutAsync_Should_ReturnInternalServerError_When_UnexpectedExceptionOccurs()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(new InvalidOperationException("unexpected"));
        var factory = new FakeHttpClientFactory(new HttpClient(handler));
        var request = new BaseHttpRequest
        {
            BaseAddress = "https://example.test",
            RequestUri = "profiles/1",
            ContentType = TEXT_JSON,
            RequestData = "{}"
        };
        var sut = CreateSut(factory);

        // Act
        var result = await sut.PutAsync(request);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        (await result.Content.ReadAsStringAsync()).Should().Contain("unexpected");
    }

    [Fact]
    public async Task DeleteAsync_Should_SendQueryParameters_When_RequestHasQueryData()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.NoContent));
        var factory = new FakeHttpClientFactory(new HttpClient(handler));
        var request = new BaseHttpRequest
        {
            BaseAddress = "https://example.test",
            RequestUri = "objects",
            AdditionalQueryParams = new Dictionary<string, string>
            {
                ["id"] = "abc 123"
            }
        };
        var sut = CreateSut(factory);

        // Act
        var result = await sut.DeleteAsync(request);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        handler.LastRequest.Method.Should().Be(HttpMethod.Delete);
        handler.LastRequest.RequestUri.Query.Should().Be("?id=abc%20123");
    }

    [Fact]
    public async Task DeleteAsync_Should_SendAuthorizationHeaderAndPreserveUri_When_QueryDataIsMissing()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.NoContent));
        var factory = new FakeHttpClientFactory(new HttpClient(handler));
        var request = new BaseHttpRequest
        {
            RequestUri = "https://example.test/objects/one",
            AuthenticationValue = new AuthenticationHeaderValue("Bearer", "token"),
            AdditionalQueryParams = null
        };
        var sut = CreateSut(factory);

        // Act
        var result = await sut.DeleteAsync(request);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        handler.LastRequest.Headers.Authorization.Parameter.Should().Be("token");
        handler.LastRequest.RequestUri.ToString().Should().Be("https://example.test/objects/one");
    }

    [Fact]
    public async Task GetAsync_Should_ReplaceExistingHeader_When_HeaderAlreadyExists()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK));
        var client = new HttpClient(handler);
        client.DefaultRequestHeaders.TryAddWithoutValidation("X-Trace", "old");
        var factory = new FakeHttpClientFactory(client);
        var request = new BaseHttpRequest
        {
            RequestUri = "https://example.test/users",
            AdditionalHeaders = new Dictionary<string, string>
            {
                ["X-Trace"] = "new"
            }
        };
        var sut = CreateSut(factory);

        // Act
        var result = await sut.GetAsync(request);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        handler.LastRequest.Headers.GetValues("X-Trace").Should().ContainSingle().Which.Should().Be("new");
    }

    private static RestClientMultipleService CreateSut(IHttpClientFactory factory) =>
        new(
            factory,
            Mock.Of<ILogger<RestClientMultipleService>>());
}
