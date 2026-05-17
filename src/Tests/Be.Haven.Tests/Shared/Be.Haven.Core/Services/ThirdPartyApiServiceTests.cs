namespace Be.Haven.Tests.Shared.Be.Haven.Core.Services;

public sealed class ThirdPartyApiServiceTests
{
    private static readonly object MappingLock = new();
    private static bool _isMappingRegistered;

    [Fact]
    public async Task HandleApiData_Should_ReturnContent_When_ResponseIsSuccessful()
    {
        // Arrange
        var request = new BaseThirdPartyApiRequest
        {
            Method = GET
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"success":true}""")
        };
        var rest = new Mock<IRestClientMultipleService>();
        rest.Setup(x => x.GetAsync(It.IsAny<BaseHttpRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        var sut = CreateSut(rest);

        // Act
        var result = await sut.HandleApiData(request);

        // Assert
        result.Should().Be("""{"success":true}""");
    }

    [Fact]
    public async Task HandleApiData_Should_ThrowHttpStatusCodeException_When_ResponseIsNotSuccessful()
    {
        // Arrange
        var request = new BaseThirdPartyApiRequest
        {
            Method = GET
        };
        var response = new HttpResponseMessage(HttpStatusCode.BadGateway)
        {
            ReasonPhrase = "gateway",
            Content = new StringContent("failed")
        };
        var rest = new Mock<IRestClientMultipleService>();
        rest.Setup(x => x.GetAsync(It.IsAny<BaseHttpRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        var sut = CreateSut(rest);

        // Act
        var action = () => sut.HandleApiData(request);

        // Assert
        await action.Should().ThrowAsync<HttpStatusCodeException>()
            .Where(x => x.StatusCode == (int)HttpStatusCode.BadGateway);
    }

    [Fact]
    public async Task HandleApiDataTyped_Should_DeserializeContent_When_ResponseIsSuccessful()
    {
        // Arrange
        var request = new BaseThirdPartyApiRequest
        {
            Method = GET
        };
        var expected = new CoreSerializationModel
        {
            DisplayName = "Haven"
        };
        var rest = new Mock<IRestClientMultipleService>();
        rest.Setup(x => x.GetAsync(It.IsAny<BaseHttpRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"displayName":"Haven"}""")
            });
        var serializer = new Mock<IJsonSerializerService>();
        serializer.Setup(x => x.Serialize(It.IsAny<object>())).Returns("{}");
        serializer.Setup(x => x.Deserialize<CoreSerializationModel>("""{"displayName":"Haven"}""")).Returns(expected);
        var sut = CreateSut(rest, serializer);

        // Act
        var result = await sut.HandleApiData<BaseThirdPartyApiRequest, CoreSerializationModel>(request);

        // Assert
        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task HandleApiDataTyped_Should_ThrowHttpStatusCodeException_When_ResponseIsNotSuccessful()
    {
        // Arrange
        var request = new BaseThirdPartyApiRequest
        {
            Method = GET
        };
        var rest = new Mock<IRestClientMultipleService>();
        rest.Setup(x => x.GetAsync(It.IsAny<BaseHttpRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                ReasonPhrase = "bad",
                Content = new StringContent("failed")
            });
        var sut = CreateSut(rest);

        // Act
        var action = () => sut.HandleApiData<BaseThirdPartyApiRequest, CoreSerializationModel>(request);

        // Assert
        await action.Should().ThrowAsync<HttpStatusCodeException>()
            .Where(x => x.StatusCode == StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task HandleDynamicHttpRequest_Should_MapRequestAndDispatchPost_When_MethodIsPost()
    {
        // Arrange
        EnsureMappingRegistered();
        BaseHttpRequest captured = null;
        var file = CreateFormFile("avatar.jpg", "image/jpeg", "image");
        var request = new BaseThirdPartyApiRequest
        {
            HttpClientName = "upload-client",
            Method = POST,
            BaseUrl = "https://example.test",
            Endpoint = "upload",
            Content = "{}",
            ContentType = TEXT_JSON,
            File = file,
            Headers = new Dictionary<string, string>
            {
                ["X-Api-Key"] = "secret"
            },
            QueryParameters = new Dictionary<string, string>
            {
                ["tenant"] = "one"
            }
        };
        var rest = new Mock<IRestClientMultipleService>();
        rest.Setup(x => x.PostAsync(It.IsAny<BaseHttpRequest>(), It.IsAny<CancellationToken>()))
            .Callback<BaseHttpRequest, CancellationToken>((model, _) => captured = model)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
        var sut = CreateSut(rest);

        // Act
        var result = await sut.HandleDynamicHttpRequest(request);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        captured.HttpClientName.Should().Be("upload-client");
        captured.BaseAddress.Should().Be("https://example.test");
        captured.RequestUri.Should().Be("upload");
        captured.RequestData.Should().Be("{}");
        captured.ContentType.Should().Be(TEXT_JSON);
        captured.File.Should().BeSameAs(file);
        captured.AdditionalHeaders["X-Api-Key"].Should().Be("secret");
        captured.AdditionalQueryParams["tenant"].Should().Be("one");
    }

    [Fact]
    public async Task HandleDynamicHttpRequest_Should_ThrowArgumentException_When_MethodIsUnsupported()
    {
        // Arrange
        var request = new BaseThirdPartyApiRequest
        {
            Method = "PATCH"
        };
        var sut = CreateSut(new Mock<IRestClientMultipleService>());

        // Act
        var action = () => sut.HandleDynamicHttpRequest(request);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage(HTTP_METHOD_NOT_SUPPORTED);
    }

    [Theory]
    [InlineData(GET)]
    [InlineData(PUT)]
    [InlineData(DELETE)]
    public async Task HandleDynamicHttpRequest_Should_DispatchToConfiguredRestMethod_When_MethodIsSupported(string method)
    {
        // Arrange
        EnsureMappingRegistered();
        var request = new BaseThirdPartyApiRequest
        {
            Method = method
        };
        var rest = new Mock<IRestClientMultipleService>();
        rest.Setup(x => x.GetAsync(It.IsAny<BaseHttpRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
        rest.Setup(x => x.PutAsync(It.IsAny<BaseHttpRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Accepted));
        rest.Setup(x => x.DeleteAsync(It.IsAny<BaseHttpRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NoContent));
        var sut = CreateSut(rest);

        // Act
        var result = await sut.HandleDynamicHttpRequest(request);

        // Assert
        result.StatusCode.Should().Be(method switch
        {
            GET => HttpStatusCode.OK,
            PUT => HttpStatusCode.Accepted,
            _ => HttpStatusCode.NoContent
        });
    }

    private static ThirdPartyApiService CreateSut(Mock<IRestClientMultipleService> rest) =>
        CreateSut(rest, CreateSerializer());

    private static ThirdPartyApiService CreateSut(
        Mock<IRestClientMultipleService> rest,
        Mock<IJsonSerializerService> serializer) =>
        new(
            rest.Object,
            Mock.Of<ILogger<ThirdPartyApiService>>(),
            serializer.Object);

    private static Mock<IJsonSerializerService> CreateSerializer()
    {
        var serializer = new Mock<IJsonSerializerService>();
        serializer.Setup(x => x.Serialize(It.IsAny<object>())).Returns("{}");

        return serializer;
    }

    private static IFormFile CreateFormFile(
        string fileName,
        string contentType,
        string content)
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        return new FormFile(stream, 0, stream.Length, "File", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    private static void EnsureMappingRegistered()
    {
        lock (MappingLock)
        {
            if (_isMappingRegistered)
            {
                return;
            }

            new BaseMappingConfig().Register(TypeAdapterConfig.GlobalSettings);
            _isMappingRegistered = true;
        }
    }
}
