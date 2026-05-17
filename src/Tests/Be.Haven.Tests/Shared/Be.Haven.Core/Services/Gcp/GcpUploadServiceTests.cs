using StorageObject = Google.Apis.Storage.v1.Data.Object;
using Google.Apis.Auth.OAuth2;

namespace Be.Haven.Tests.Shared.Be.Haven.Core.Services.Gcp;

public sealed class GcpUploadServiceTests
{
    [Fact]
    public async Task UploadAsync_Should_ReturnFileUploadResponse_When_StorageUploadSucceeds()
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("file"));
        var request = new FileUploadRequest
        {
            BucketName = "haven",
            ObjectName = "avatars/a.jpg",
            ContentType = "image/jpeg",
            BasePublicUrl = "https://cdn.test",
            FileStream = stream
        };
        var storage = new Mock<StorageClient>();
        storage
            .Setup(x => x.UploadObjectAsync(
                request.BucketName,
                request.ObjectName,
                request.ContentType,
                stream,
                It.IsAny<UploadObjectOptions>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<IProgress<Google.Apis.Upload.IUploadProgress>>()))
            .ReturnsAsync(new StorageObject
            {
                Bucket = request.BucketName,
                Name = request.ObjectName,
                ContentType = request.ContentType
            });
        var sut = CreateSut(storage);

        // Act
        var result = await sut.UploadAsync(request, CancellationToken.None);

        // Assert
        result.ObjectName.Should().Be(request.ObjectName);
        result.ContentType.Should().Be(request.ContentType);
        result.Url.Should().Be("https://cdn.test/haven/avatars/a.jpg");
    }

    [Fact]
    public async Task UploadAsync_Should_ThrowArgumentException_When_StorageUploadFails()
    {
        // Arrange
        var request = new FileUploadRequest
        {
            BucketName = "haven",
            ObjectName = "avatars/a.jpg",
            ContentType = "image/jpeg",
            BasePublicUrl = "https://cdn.test",
            FileStream = new MemoryStream(Encoding.UTF8.GetBytes("file"))
        };
        var storage = new Mock<StorageClient>();
        storage
            .Setup(x => x.UploadObjectAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Stream>(),
                It.IsAny<UploadObjectOptions>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<IProgress<Google.Apis.Upload.IUploadProgress>>()))
            .ThrowsAsync(new InvalidOperationException("storage down"));
        var sut = CreateSut(storage);

        // Act
        var action = () => sut.UploadAsync(request, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage(UPLOAD_FAILED);
    }

    [Fact]
    public async Task DeleteAsync_Should_ReturnTrue_When_StorageDeleteSucceeds()
    {
        // Arrange
        var storage = new Mock<StorageClient>();
        storage
            .Setup(x => x.DeleteObjectAsync(
                "haven",
                "avatars/a.jpg",
                It.IsAny<DeleteObjectOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var sut = CreateSut(storage);

        // Act
        var result = await sut.DeleteAsync("haven", "avatars/a.jpg", CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_Should_ReturnFalse_When_StorageDeleteFails()
    {
        // Arrange
        var storage = new Mock<StorageClient>();
        storage
            .Setup(x => x.DeleteObjectAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DeleteObjectOptions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("storage down"));
        var sut = CreateSut(storage);

        // Act
        var result = await sut.DeleteAsync("haven", "avatars/a.jpg", CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetSignedUrl_Should_ThrowArgumentException_When_SignerCreationFails()
    {
        // Arrange
        var storage = new Mock<StorageClient>();
        var sut = CreateSut(
            storage,
            _ => throw new InvalidOperationException("signer unavailable"));

        // Act
        var action = () => sut.GetSignedUrl(
            "haven",
            "avatars/a.jpg",
            TimeSpan.FromMinutes(5),
            CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage(SIGNED_URL_FAILED);
    }

    [Fact]
    public async Task GetSignedUrl_Should_ReturnSignedUri_When_SignerSucceeds()
    {
        // Arrange
        var storage = new Mock<StorageClient>();
        var signer = CreateOfflineUrlSigner();
        var sut = CreateSut(storage, _ => signer);

        // Act
        var result = await sut.GetSignedUrl(
            "haven",
            "avatars/a.jpg",
            TimeSpan.FromMinutes(5),
            CancellationToken.None);

        // Assert
        result.AbsoluteUri.Should().StartWith("https://storage.googleapis.com/haven/avatars/a.jpg?");
        result.Query.Should().Contain("X-Goog-Signature=");
    }

    private static GcpUploadService CreateSut(
        Mock<StorageClient> storage,
        Func<StorageClient, UrlSigner> createUrlSigner = null) =>
        new(
            storage.Object,
            Mock.Of<ILogger<GcpUploadService>>(),
            createUrlSigner);

    private static UrlSigner CreateOfflineUrlSigner()
    {
        using var rsa = RSA.Create(2048);
        var privateKey = rsa.ExportPkcs8PrivateKeyPem();
        var credential = new ServiceAccountCredential(
            new ServiceAccountCredential.Initializer("haven-test@haven-test.iam.gserviceaccount.com")
            {
                ProjectId = "haven-test",
                KeyId = "test-key"
            }.FromPrivateKey(privateKey));

        return UrlSigner.FromCredential(credential);
    }
}
