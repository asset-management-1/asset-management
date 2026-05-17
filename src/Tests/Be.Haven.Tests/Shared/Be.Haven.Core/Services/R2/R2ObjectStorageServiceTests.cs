namespace Be.Haven.Tests.Shared.Be.Haven.Core.Services.R2;

public sealed class R2ObjectStorageServiceTests
{
    [Fact]
    public async Task UploadAsync_Should_SendSignedPutRequestAndReturnMetadata_When_R2ReturnsSuccess()
    {
        // Arrange
        var bytes = Encoding.UTF8.GetBytes("hello-r2");
        var checksum = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        var thirdParty = new Mock<IThirdPartyApiService>();
        BaseThirdPartyApiRequest capturedRequest = null;
        byte[] capturedPayload = [];
        thirdParty
            .Setup(x => x.HandleDynamicHttpRequest(
                It.IsAny<BaseThirdPartyApiRequest>(),
                It.IsAny<CancellationToken>()))
            .Returns<BaseThirdPartyApiRequest, CancellationToken>(async (request, cancellationToken) =>
            {
                capturedRequest = request;
                using var memory = new MemoryStream();
                await request.ContentStream.CopyToAsync(memory, cancellationToken);
                capturedPayload = memory.ToArray();

                return new HttpResponseMessage(HttpStatusCode.OK);
            });
        var sut = CreateSut(thirdParty, CreateOptions());
        await using var stream = new MemoryStream(bytes);

        // Act
        var result = await sut.UploadAsync(new ObjectUploadRequestModel
        {
            Content = stream,
            ContentType = " image/png ",
            ObjectKey = "avatars/a b.jpg"
        });

        // Assert
        result.BucketName.Should().Be("haven-assets");
        result.ObjectKey.Should().Be("avatars/a b.jpg");
        result.ContentType.Should().Be("image/png");
        result.FileSize.Should().Be(bytes.Length);
        result.Checksum.Should().Be(checksum);
        capturedRequest.HttpClientName.Should().Be(UPLOAD_R2_OBJECT);
        capturedRequest.Method.Should().Be(PUT);
        capturedRequest.Endpoint.Should().Be("https://r2.example.com/haven-assets/avatars/a b.jpg");
        capturedRequest.ContentType.Should().Be("image/png");
        capturedPayload.Should().Equal(bytes);
        capturedRequest.Headers[R2_AMZ_CONTENT_SHA256_HEADER].Should().Be(checksum);
        capturedRequest.Headers[AUTHORIZATION_HEADER].Should().StartWith($"{R2_SIGNATURE_ALGORITHM} Credential=access-key/");
    }

    [Fact]
    public async Task UploadAsync_Should_UseDefaultContentType_When_ContentTypeIsBlank()
    {
        // Arrange
        var thirdParty = new Mock<IThirdPartyApiService>();
        BaseThirdPartyApiRequest capturedRequest = null;
        thirdParty
            .Setup(x => x.HandleDynamicHttpRequest(
                It.IsAny<BaseThirdPartyApiRequest>(),
                It.IsAny<CancellationToken>()))
            .Returns<BaseThirdPartyApiRequest, CancellationToken>((request, _) =>
            {
                capturedRequest = request;

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            });
        var sut = CreateSut(thirdParty, CreateOptions());
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes("file"));

        // Act
        var result = await sut.UploadAsync(new ObjectUploadRequestModel
        {
            Content = stream,
            ContentType = "   ",
            ObjectKey = "avatars/file.bin"
        });

        // Assert
        result.ContentType.Should().Be(R2_DEFAULT_CONTENT_TYPE);
        capturedRequest.ContentType.Should().Be(R2_DEFAULT_CONTENT_TYPE);
    }

    [Fact]
    public async Task UploadAsync_Should_WrapUploadFailure_When_R2ReturnsFailure()
    {
        // Arrange
        var thirdParty = new Mock<IThirdPartyApiService>();
        thirdParty
            .Setup(x => x.HandleDynamicHttpRequest(
                It.IsAny<BaseThirdPartyApiRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("""{"secretAccessKey":"secret"}""")
            });
        var sut = CreateSut(thirdParty, CreateOptions());
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes("file"));

        // Act
        var act = () => sut.UploadAsync(new ObjectUploadRequestModel
        {
            Content = stream,
            ContentType = "image/png",
            ObjectKey = "avatars/file.png"
        });

        // Assert
        var result = await act.Should().ThrowAsync<ArgumentException>();
        result.Which.Message.Should().Be(R2_UPLOAD_FAILED_MESSAGE);
        result.Which.InnerException.Should().BeOfType<HttpRequestException>();
    }

    [Fact]
    public async Task UploadAsync_Should_ThrowInvalidOperationException_When_R2OptionsAreMissing()
    {
        // Arrange
        var thirdParty = new Mock<IThirdPartyApiService>();
        var sut = CreateSut(thirdParty, new R2StorageOptions());
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes("file"));

        // Act
        var act = () => sut.UploadAsync(new ObjectUploadRequestModel
        {
            Content = stream,
            ContentType = "image/png",
            ObjectKey = "avatars/file.png"
        });

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage(R2_OPTIONS_MISSING_MESSAGE);
        thirdParty.Verify(x => x.HandleDynamicHttpRequest(
            It.IsAny<BaseThirdPartyApiRequest>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.NoContent)]
    [InlineData(HttpStatusCode.NotFound)]
    public async Task DeleteAsync_Should_ReturnTrue_When_R2ReturnsSuccessOrNotFound(HttpStatusCode statusCode)
    {
        // Arrange
        var thirdParty = new Mock<IThirdPartyApiService>();
        BaseThirdPartyApiRequest capturedRequest = null;
        thirdParty
            .Setup(x => x.HandleDynamicHttpRequest(
                It.IsAny<BaseThirdPartyApiRequest>(),
                It.IsAny<CancellationToken>()))
            .Returns<BaseThirdPartyApiRequest, CancellationToken>((request, _) =>
            {
                capturedRequest = request;

                return Task.FromResult(new HttpResponseMessage(statusCode));
            });
        var sut = CreateSut(thirdParty, CreateOptions());

        // Act
        var result = await sut.DeleteAsync("avatars/file.png");

        // Assert
        result.Should().BeTrue();
        capturedRequest.Method.Should().Be(DELETE);
        capturedRequest.Endpoint.Should().Be("https://r2.example.com/haven-assets/avatars/file.png");
        capturedRequest.Headers[R2_AMZ_CONTENT_SHA256_HEADER].Should().Be(R2_EMPTY_PAYLOAD_HASH);
    }

    [Fact]
    public async Task DeleteAsync_Should_ReturnFalse_When_R2ReturnsFailure()
    {
        // Arrange
        var thirdParty = new Mock<IThirdPartyApiService>();
        thirdParty
            .Setup(x => x.HandleDynamicHttpRequest(
                It.IsAny<BaseThirdPartyApiRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("provider failed")
            });
        var sut = CreateSut(thirdParty, CreateOptions());

        // Act
        var result = await sut.DeleteAsync("avatars/file.png");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_Should_ReturnFalse_When_ThirdPartyCallThrows()
    {
        // Arrange
        var thirdParty = new Mock<IThirdPartyApiService>();
        thirdParty
            .Setup(x => x.HandleDynamicHttpRequest(
                It.IsAny<BaseThirdPartyApiRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("network failed"));
        var sut = CreateSut(thirdParty, CreateOptions());

        // Act
        var result = await sut.DeleteAsync("avatars/file.png");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_Should_ThrowArgumentException_When_ObjectKeyIsBlank()
    {
        // Arrange
        var thirdParty = new Mock<IThirdPartyApiService>();
        var sut = CreateSut(thirdParty, CreateOptions());

        // Act
        var act = () => sut.DeleteAsync("   ");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage($"{R2_OBJECT_KEY_REQUIRED_MESSAGE}*");
        thirdParty.Verify(x => x.HandleDynamicHttpRequest(
            It.IsAny<BaseThirdPartyApiRequest>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    private static R2ObjectStorageService CreateSut(
        Mock<IThirdPartyApiService> thirdParty,
        R2StorageOptions options)
    {
        return new R2ObjectStorageService(
            thirdParty.Object,
            Options.Create(options),
            Mock.Of<ILogger<R2ObjectStorageService>>());
    }

    private static R2StorageOptions CreateOptions()
    {
        return new R2StorageOptions
        {
            Endpoint = "https://r2.example.com/",
            BucketName = "haven-assets",
            AccessKeyId = "access-key",
            SecretAccessKey = "secret-key"
        };
    }
}
