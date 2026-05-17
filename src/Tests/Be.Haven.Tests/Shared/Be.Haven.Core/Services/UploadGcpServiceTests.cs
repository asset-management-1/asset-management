namespace Be.Haven.Tests.Shared.Be.Haven.Core.Services;

public sealed class UploadGcpServiceTests
{
    [Fact]
    public async Task UploadFilesToGcpAsync_Should_BuildMultipartThirdPartyRequest_When_Called()
    {
        // Arrange
        BaseThirdPartyApiRequest captured = null;
        var file = CreateFormFile();
        var expected = new UploadGcpResponse
        {
            Success = true,
            Data = Guid.NewGuid()
        };
        var thirdParty = new Mock<IThirdPartyApiService>();
        thirdParty
            .Setup(x => x.HandleApiData<BaseThirdPartyApiRequest, UploadGcpResponse>(It.IsAny<BaseThirdPartyApiRequest>()))
            .Callback<BaseThirdPartyApiRequest>(request => captured = request)
            .ReturnsAsync(expected);
        var request = new UploadGcpRequest
        {
            File = file,
            Description = "avatar",
            UploadedBy = "codex",
            ResourceId = Guid.NewGuid()
        };
        var sut = CreateSut(thirdParty);

        // Act
        var result = await sut.UploadFilesToGcpAsync(request);

        // Assert
        result.Should().BeSameAs(expected);
        captured.HttpClientName.Should().Be(UPLOAD_GCP_SERVICE);
        captured.Method.Should().Be(POST);
        captured.BaseUrl.Should().Be("https://media.test");
        captured.Endpoint.Should().Be("/files");
        captured.ContentType.Should().Be(MULTIPART_FORM_DATA);
        captured.File.Should().BeSameAs(file);
        captured.Headers[UPLOAD_GCP_API_KEY].Should().Be("api-key");
        captured.QueryParameters[UPLOAD_GCP_DESCRIPTION].Should().Be("avatar");
        captured.QueryParameters[UPLOAD_GCP_UPLOAD_BY].Should().Be("codex");
        captured.QueryParameters[UPLOAD_GCP_THUMBNAIL].Should().Be(bool.TrueString);
        captured.QueryParameters[UPLOAD_GCP_RESOURCE_ID].Should().Be(request.ResourceId.ToString());
    }

    [Fact]
    public async Task GetImageByFileIdAsync_Should_BuildGetThirdPartyRequest_When_Called()
    {
        // Arrange
        BaseThirdPartyApiRequest captured = null;
        var fileId = Guid.NewGuid();
        var expected = new GetImageByFileIdResponse
        {
            Success = true
        };
        var thirdParty = new Mock<IThirdPartyApiService>();
        thirdParty
            .Setup(x => x.HandleApiData<BaseThirdPartyApiRequest, GetImageByFileIdResponse>(It.IsAny<BaseThirdPartyApiRequest>()))
            .Callback<BaseThirdPartyApiRequest>(request => captured = request)
            .ReturnsAsync(expected);
        var sut = CreateSut(thirdParty);

        // Act
        var result = await sut.GetImageByFileIdAsync(fileId);

        // Assert
        result.Should().BeSameAs(expected);
        captured.HttpClientName.Should().Be(UPLOAD_GCP_GET_IMAGE_BY_FILE_ID);
        captured.Method.Should().Be(GET);
        captured.BaseUrl.Should().Be("https://media.test");
        captured.Endpoint.Should().Be($"/files/{fileId}");
        captured.ContentType.Should().Be(TEXT_JSON);
        captured.Headers[UPLOAD_GCP_API_KEY].Should().Be("api-key");
    }

    private static UploadGcpService CreateSut(Mock<IThirdPartyApiService> thirdParty) =>
        new(
            thirdParty.Object,
            Options.Create(new UploadGcpOptions
            {
                BaseUrl = "https://media.test",
                ApiKey = "api-key",
                EndPoints = new MediaEnpoints
                {
                    UploadFile = "/files",
                    GetFileById = "/files/{0}"
                }
            }));

    private static IFormFile CreateFormFile()
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("image"));

        return new FormFile(stream, 0, stream.Length, "File", "avatar.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };
    }
}
