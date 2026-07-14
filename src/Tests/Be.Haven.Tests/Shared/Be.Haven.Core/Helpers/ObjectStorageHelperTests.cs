namespace Be.Haven.Tests.Shared.Be.Haven.Core.Helpers;

public sealed class ObjectStorageHelperTests
{
    private static readonly Guid OwnerPublicId = Guid.Parse("fc07838a-daad-46b8-ba8b-eab01715dd65");

    [Fact]
    public async Task UploadOwnerScopedFormFilesAsync_Should_ThrowBatchFailure_When_RequiredFileIsMissing()
    {
        // Arrange
        var storage = new FakeObjectStorageService();
        var request = CreateBatchRequest([
            new ObjectStorageUploadFileModel
            {
                SlotName = "front",
                ObjectTag = "front",
                IsRequired = true
            }
        ]);

        // Act
        var act = () => ObjectStorageHelper.UploadOwnerScopedFormFilesAsync(storage, request);

        // Assert
        var result = await act.Should().ThrowAsync<InvalidOperationException>();
        result.Which.Message.Should().Be(OBJECT_STORAGE_BATCH_UPLOAD_FAILED_MESSAGE);
        result.Which.InnerException.Should().BeOfType<ArgumentException>();
        storage.UploadRequests.Should().BeEmpty();
        storage.DeleteRequests.Should().BeEmpty();
    }

    [Fact]
    public async Task UploadOwnerScopedFormFilesAsync_Should_SkipOptionalMissingFile_When_FileIsNotRequired()
    {
        // Arrange
        var storage = new FakeObjectStorageService();
        var request = CreateBatchRequest([
            new ObjectStorageUploadFileModel
            {
                SlotName = "side",
                ObjectTag = "side",
                IsRequired = false
            }
        ]);

        // Act
        var result = await ObjectStorageHelper.UploadOwnerScopedFormFilesAsync(storage, request);

        // Assert
        result.UploadsBySlot.Should().BeEmpty();
        storage.UploadRequests.Should().BeEmpty();
    }

    [Fact]
    public async Task UploadOwnerScopedFormFilesAsync_Should_UploadFilesBySlot_When_FilesAreProvided()
    {
        // Arrange
        var storage = new FakeObjectStorageService();
        var request = CreateBatchRequest([
            new ObjectStorageUploadFileModel
            {
                SlotName = "front",
                ObjectTag = "front",
                File = CreateFormFile("front.jpg", "image/jpeg"),
                IsRequired = true
            },
            new ObjectStorageUploadFileModel
            {
                SlotName = "side",
                ObjectTag = "side",
                File = CreateFormFile("side.jpg", "image/jpeg")
            }
        ]);

        // Act
        var result = await ObjectStorageHelper.UploadOwnerScopedFormFilesAsync(storage, request);

        // Assert
        result.UploadsBySlot.Keys.Should().BeEquivalentTo(["front", "side"]);
        result.GetUpload("front").ObjectKey.Should().StartWith($"avatars/{OwnerPublicId}/");
        result.GetUpload("side").ObjectKey.Should().Contain("/side-");
        storage.UploadRequests.Should().HaveCount(2);
    }

    [Fact]
    public async Task UploadOwnerScopedFormFilesAsync_Should_UseCanonicalImageContentType_When_MobileMetadataIsGeneric()
    {
        // Arrange
        var storage = new FakeObjectStorageService();
        var request = CreateBatchRequest([
            new ObjectStorageUploadFileModel
            {
                SlotName = "electric-1",
                ObjectTag = "electric",
                File = CreateFormFile("electric-meter.webp", "application/octet-stream"),
                IsRequired = true
            }
        ]);

        // Act
        await ObjectStorageHelper.UploadOwnerScopedFormFilesAsync(storage, request);

        // Assert
        storage.UploadRequests.Should().ContainSingle();
        storage.UploadRequests[0].ContentType.Should().Be(WEBP_IMAGE_CONTENT_TYPE);
    }

    [Fact]
    public async Task UploadOwnerScopedFormFilesAsync_Should_CleanupSuccessfulUploads_When_LaterUploadFails()
    {
        // Arrange
        var storage = new FakeObjectStorageService();
        storage.UploadHandler = (request, _) =>
        {
            if (storage.UploadRequests.Count == 1)
            {
                return Task.FromResult(new ObjectUploadResponseModel
                {
                    BucketName = "fake-bucket",
                    ObjectKey = request.ObjectKey,
                    ContentType = request.ContentType,
                    FileSize = 10,
                    Checksum = "checksum"
                });
            }

            throw new ArgumentException("upload failed");
        };
        var request = CreateBatchRequest([
            new ObjectStorageUploadFileModel
            {
                SlotName = "front",
                ObjectTag = "front",
                File = CreateFormFile("front.jpg", "image/jpeg"),
                IsRequired = true
            },
            new ObjectStorageUploadFileModel
            {
                SlotName = "side",
                ObjectTag = "side",
                File = CreateFormFile("side.jpg", "image/jpeg"),
                IsRequired = true
            }
        ]);

        // Act
        var act = () => ObjectStorageHelper.UploadOwnerScopedFormFilesAsync(storage, request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        storage.DeleteRequests.Should().ContainSingle();
        storage.DeleteRequests[0].Should().Contain("/front-");
    }

    [Fact]
    public async Task UploadOwnerScopedFormFilesAsync_Should_CleanupWithIndependentToken_When_LaterUploadIsCancelled()
    {
        // Arrange
        using var cancellationSource = new CancellationTokenSource();
        var cleanupTokenWasCancelled = true;
        var storage = new FakeObjectStorageService();
        storage.UploadHandler = (request, _) =>
        {
            if (storage.UploadRequests.Count == 1)
            {
                return Task.FromResult(new ObjectUploadResponseModel
                {
                    BucketName = "fake-bucket",
                    ObjectKey = request.ObjectKey,
                    ContentType = request.ContentType,
                    FileSize = 10,
                    Checksum = "checksum"
                });
            }

            cancellationSource.Cancel();
            throw new OperationCanceledException(cancellationSource.Token);
        };
        storage.DeleteHandler = (_, token) =>
        {
            cleanupTokenWasCancelled = token.IsCancellationRequested;
            return Task.FromResult(true);
        };
        var request = CreateBatchRequest([
            new ObjectStorageUploadFileModel
            {
                SlotName = "front",
                ObjectTag = "front",
                File = CreateFormFile("front.jpg", "image/jpeg"),
                IsRequired = true
            },
            new ObjectStorageUploadFileModel
            {
                SlotName = "side",
                ObjectTag = "side",
                File = CreateFormFile("side.jpg", "image/jpeg"),
                IsRequired = true
            }
        ]);

        // Act
        var act = () => ObjectStorageHelper.UploadOwnerScopedFormFilesAsync(
            storage,
            request,
            cancellationSource.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        cleanupTokenWasCancelled.Should().BeFalse();
        storage.DeleteRequests.Should().ContainSingle();
        storage.DeleteRequests[0].Should().Contain("/front-");
    }

    [Fact]
    public async Task CleanupUploadedObjectsAsync_Should_DeleteDistinctObjectKeysAndCountFailures()
    {
        // Arrange
        var storage = new FakeObjectStorageService();
        storage.DeleteHandler = (objectKey, _) => Task.FromResult(objectKey != "objects/two.jpg");
        var uploads = new[]
        {
            new ObjectUploadResponseModel { ObjectKey = "objects/one.jpg" },
            new ObjectUploadResponseModel { ObjectKey = "objects/one.jpg" },
            new ObjectUploadResponseModel { ObjectKey = "objects/two.jpg" },
            new ObjectUploadResponseModel { ObjectKey = null },
            new ObjectUploadResponseModel { ObjectKey = "   " }
        };

        // Act
        var result = await ObjectStorageHelper.CleanupUploadedObjectsAsync(storage, uploads);

        // Assert
        result.AttemptedCount.Should().Be(2);
        result.FailedCount.Should().Be(1);
        result.HasFailures.Should().BeTrue();
        storage.DeleteRequests.Should().Equal("objects/one.jpg", "objects/two.jpg");
    }

    [Fact]
    public async Task UploadOwnerScopedValidatedImageFormFilesAsync_Should_ValidateThenPreserveOriginalUpload()
    {
        // Arrange
        var originalContent = Encoding.UTF8.GetBytes("original-image-content");
        byte[] uploadedContent = null;
        var storage = new FakeObjectStorageService
        {
            UploadHandler = async (upload, cancellationToken) =>
            {
                using var destination = new MemoryStream();
                await upload.Content.CopyToAsync(destination, cancellationToken);
                uploadedContent = destination.ToArray();
                return new ObjectUploadResponseModel
                {
                    BucketName = "fake-bucket",
                    ObjectKey = upload.ObjectKey,
                    ContentType = upload.ContentType,
                    FileSize = uploadedContent.LongLength,
                    Checksum = "checksum"
                };
            }
        };
        var imageValidator = new Mock<IImageOptimizationService>();
        var file = CreateFormFile(originalContent, "front.png", "image/png");
        var request = CreateBatchRequest([
            new ObjectStorageUploadFileModel
            {
                SlotName = "front",
                ObjectTag = "front",
                File = file,
                IsRequired = true
            }
        ]);

        // Act
        var result = await ObjectStorageHelper.UploadOwnerScopedValidatedImageFormFilesAsync(
            storage,
            imageValidator.Object,
            request);

        // Assert
        imageValidator.Verify(service => service.ValidateAsync(file, It.IsAny<CancellationToken>()), Times.Once);
        uploadedContent.Should().Equal(originalContent);
        storage.UploadRequests[0].ContentType.Should().Be("image/png");
        result.GetUpload("front").ObjectKey.Should().EndWith(".png");
    }

    [Fact]
    public async Task UploadOwnerScopedValidatedImageFormFilesAsync_Should_NotUpload_When_AnyDecodeFails()
    {
        // Arrange
        var storage = new FakeObjectStorageService();
        var imageValidator = new Mock<IImageOptimizationService>();
        imageValidator
            .Setup(service => service.ValidateAsync(
                It.Is<IFormFile>(file => file.FileName == "side.jpg"),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException(IMAGE_FILE_INVALID_MESSAGE));
        var request = CreateBatchRequest([
            new ObjectStorageUploadFileModel
            {
                SlotName = "front",
                ObjectTag = "front",
                File = CreateFormFile("front.jpg", "image/jpeg"),
                IsRequired = true
            },
            new ObjectStorageUploadFileModel
            {
                SlotName = "side",
                ObjectTag = "side",
                File = CreateFormFile("side.jpg", "image/jpeg"),
                IsRequired = true
            }
        ]);

        // Act
        var action = () => ObjectStorageHelper.UploadOwnerScopedValidatedImageFormFilesAsync(
            storage,
            imageValidator.Object,
            request);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
        imageValidator.Verify(
            service => service.ValidateAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
        storage.UploadRequests.Should().BeEmpty();
    }

    [Fact]
    public void BuildOwnerScopedObjectKey_Should_BuildOwnerScopedKey_When_RequestIsValid()
    {
        // Arrange
        var request = new ObjectStorageKeyRequestModel
        {
            ConfiguredPrefix = "/avatars/",
            DefaultPrefix = "default",
            OwnerPublicId = OwnerPublicId,
            Tag = "profile",
            FileName = "avatar.png"
        };

        // Act
        var result = ObjectStorageHelper.BuildOwnerScopedObjectKey(request);

        // Assert
        result.Should().StartWith($"avatars/{OwnerPublicId}/");
        result.Should().Contain("/profile-");
        result.Should().EndWith(".png");
    }

    [Fact]
    public void BuildObjectUrl_Should_ReturnPublicUrlOrObjectKey_When_BaseUrlVaries()
    {
        // Act
        var publicUrl = ObjectStorageHelper.BuildObjectUrl("https://cdn.haven.test/", "avatars/a.jpg");
        var privateKey = ObjectStorageHelper.BuildObjectUrl("", "avatars/a.jpg");
        var blankKey = ObjectStorageHelper.BuildObjectUrl("https://cdn.haven.test/", "   ");

        // Assert
        publicUrl.Should().Be("https://cdn.haven.test/avatars/a.jpg");
        privateKey.Should().Be("avatars/a.jpg");
        blankKey.Should().Be("   ");
    }

    [Fact]
    public void GetObjectKey_Should_ReturnObjectKeyOnly_When_PublicUrlOrPrivateKeyIsSupplied()
    {
        // Act
        var publicObjectKey = ObjectStorageHelper.GetObjectKey("https://cdn.haven.test/", "https://cdn.haven.test/vehicles/one.jpg");
        var privateObjectKey = ObjectStorageHelper.GetObjectKey(null, "vehicles/one.jpg");
        var foreignUrl = ObjectStorageHelper.GetObjectKey("https://cdn.haven.test/", "https://other.test/vehicles/one.jpg");

        // Assert
        publicObjectKey.Should().Be("vehicles/one.jpg");
        privateObjectKey.Should().Be("vehicles/one.jpg");
        foreignUrl.Should().BeNull();
    }

    private static ObjectStorageUploadBatchRequestModel CreateBatchRequest(IReadOnlyCollection<ObjectStorageUploadFileModel> files)
    {
        return new ObjectStorageUploadBatchRequestModel
        {
            ConfiguredPrefix = "/avatars/",
            DefaultPrefix = "default",
            OwnerPublicId = OwnerPublicId,
            Files = files
        };
    }

    private static IFormFile CreateFormFile(
        string fileName,
        string contentType)
    {
        var content = Encoding.UTF8.GetBytes("file-bytes");
        var stream = new MemoryStream(content);

        return new FormFile(stream, 0, content.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    private static IFormFile CreateFormFile(
        byte[] content,
        string fileName,
        string contentType)
    {
        return new FormFile(new MemoryStream(content), 0, content.LongLength, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}
