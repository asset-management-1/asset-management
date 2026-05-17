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
        result.GetUpload("side").ObjectKey.Should().Contain("-side-");
        storage.UploadRequests.Should().HaveCount(2);
    }

    [Fact]
    public async Task UploadOwnerScopedFormFilesAsync_Should_ReturnCallerOwnedResult_When_ResultFactoryIsProvided()
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
            }
        ]);

        // Act
        var result = await ObjectStorageHelper.UploadOwnerScopedFormFilesAsync(
            storage,
            request,
            uploadResult => uploadResult.GetUpload("front").ObjectKey);

        // Assert
        result.Should().Contain("-front-");
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
        storage.DeleteRequests[0].Should().Contain("-front-");
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
    public void ResolvePrefix_Should_ReturnDefaultOrTrimConfiguredPrefix_When_PrefixVaries()
    {
        // Act
        var defaultResult = ObjectStorageHelper.ResolvePrefix("   ", "avatars");
        var configuredResult = ObjectStorageHelper.ResolvePrefix("/vehicles/", "avatars");

        // Assert
        defaultResult.Should().Be("avatars");
        configuredResult.Should().Be("vehicles");
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
        result.Should().Contain("-profile-");
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
}
