using System.Runtime.CompilerServices;
using Google.Api.Gax;
using Google.Cloud.SecretManager.V1;
using SecretVersion = Google.Cloud.SecretManager.V1.SecretVersion;

namespace Be.Haven.Tests.Shared.Be.Haven.Core.Services.Gcp;

public sealed class GcpSecretServiceTests
{
    [Fact]
    public async Task GetByIdAsync_Should_ReturnPlainValue_When_SecretManagerIsDisabled()
    {
        // Arrange
        var sut = CreateDisabledSut();

        // Act
        var result = await sut.GetByIdAsync("plain-secret");

        // Assert
        result.Should().Be("plain-secret");
    }

    [Fact]
    public async Task GetByIdAsync_Should_ThrowArgumentException_When_SecretIdIsBlank()
    {
        // Arrange
        var sut = CreateDisabledSut();

        // Act
        var action = () => sut.GetByIdAsync(" ");

        // Assert
        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage(SECRET_ID_REQUIRED);
    }

    [Fact]
    public async Task GetBytesAsync_Should_ReturnUtf8Bytes_When_SecretManagerIsDisabled()
    {
        // Arrange
        var sut = CreateDisabledSut();

        // Act
        var result = await sut.GetBytesAsync("secret-value");

        // Assert
        Encoding.UTF8.GetString(result).Should().Be("secret-value");
    }

    [Fact]
    public async Task GetBytesAsync_Should_ThrowArgumentException_When_SecretIdIsBlank()
    {
        // Arrange
        var sut = CreateDisabledSut();

        // Act
        var action = () => sut.GetBytesAsync(" ");

        // Assert
        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage(SECRET_ID_REQUIRED);
    }

    [Fact]
    public async Task GetBytesAsync_Should_ReturnSecretBytes_When_SecretManagerIsEnabled()
    {
        // Arrange
        var client = new Mock<SecretManagerServiceClient>();
        client
            .Setup(x => x.AccessSecretVersionAsync(
                "projects/project/secrets/secret-id/versions/2",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccessSecretVersionResponse
            {
                Payload = new SecretPayload
                {
                    Data = ByteString.CopyFromUtf8("secret-value")
                }
            });
        var sut = CreateEnabledSut(client.Object, Mock.Of<ICachingService>());

        // Act
        var result = await sut.GetBytesAsync("secret-id", "2");

        // Assert
        Encoding.UTF8.GetString(result).Should().Be("secret-value");
    }

    [Fact]
    public async Task GetJsonAsync_Should_DeserializePlainJson_When_SecretManagerIsDisabled()
    {
        // Arrange
        var sut = CreateDisabledSut();

        // Act
        var result = await sut.GetJsonAsync<CoreSerializationModel>("""{"DisplayName":"Haven"}""");

        // Assert
        result.DisplayName.Should().Be("Haven");
    }

    [Fact]
    public async Task ExistsAsync_Should_ReturnFalse_When_SecretIdIsBlank()
    {
        // Arrange
        var sut = CreateDisabledSut();

        // Act
        var result = await sut.ExistsAsync(string.Empty);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_Should_ReturnTrue_When_SecretManagerIsDisabledAndSecretIdExists()
    {
        // Arrange
        var sut = CreateDisabledSut();

        // Act
        var result = await sut.ExistsAsync("secret-id");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task VersionExistsAsync_Should_ReturnFalse_When_SecretIdIsBlank()
    {
        // Arrange
        var sut = CreateDisabledSut();

        // Act
        var result = await sut.VersionExistsAsync(" ");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task VersionExistsAsync_Should_ReturnTrue_When_SecretManagerIsDisabledAndSecretIdExists()
    {
        // Arrange
        var sut = CreateDisabledSut();

        // Act
        var result = await sut.VersionExistsAsync("secret-id");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ListVersionsAsync_Should_ReturnEmpty_When_SecretManagerIsDisabled()
    {
        // Arrange
        var sut = CreateDisabledSut();
        var result = new List<SecretVersion>();

        // Act
        await foreach (var version in sut.ListVersionsAsync("secret-id"))
        {
            result.Add(version);
        }

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ListVersionsAsync_Should_ReturnSecretVersions_When_SecretManagerReturnsPagedResults()
    {
        // Arrange
        var versions = new List<SecretVersion>
        {
            new()
            {
                Name = "projects/project/secrets/secret-id/versions/1"
            },
            new()
            {
                Name = "projects/project/secrets/secret-id/versions/2"
            }
        };
        var client = new Mock<SecretManagerServiceClient>();
        client
            .Setup(x => x.ListSecretVersionsAsync(
                It.Is<ListSecretVersionsRequest>(request =>
                    request.Parent == "projects/project/secrets/secret-id" &&
                    request.PageSize == 10),
                It.IsAny<Google.Api.Gax.Grpc.CallSettings>()))
            .Returns(new FakeSecretVersionPagedAsyncEnumerable(versions));
        var sut = CreateEnabledSut(client.Object, Mock.Of<ICachingService>());
        var result = new List<SecretVersion>();

        // Act
        await foreach (var version in sut.ListVersionsAsync("secret-id", 10))
        {
            result.Add(version);
        }

        // Assert
        result.Select(x => x.Name).Should().Equal(
            "projects/project/secrets/secret-id/versions/1",
            "projects/project/secrets/secret-id/versions/2");
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnCachedSecret_When_SecretManagerCacheHits()
    {
        // Arrange
        var cache = new Mock<ICachingService>();
        cache
            .Setup(x => x.GetAsync<string>("secret-id/versions/latest", It.IsAny<CancellationToken>()))
            .ReturnsAsync("cached-secret");
        var sut = CreateEnabledSut(Mock.Of<SecretManagerServiceClient>(), cache.Object);

        // Act
        var result = await sut.GetByIdAsync("secret-id");

        // Assert
        result.Should().Be("cached-secret");
    }

    [Fact]
    public async Task GetByIdAsync_Should_LoadAndCacheSecret_When_SecretManagerCacheMisses()
    {
        // Arrange
        var cache = new Mock<ICachingService>();
        cache
            .Setup(x => x.GetAsync<string>("secret-id/versions/latest", It.IsAny<CancellationToken>()))
            .ReturnsAsync(string.Empty);
        var client = new Mock<SecretManagerServiceClient>();
        client
            .Setup(x => x.AccessSecretVersionAsync(
                "projects/project/secrets/secret-id/versions/latest",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccessSecretVersionResponse
            {
                Payload = new SecretPayload
                {
                    Data = ByteString.CopyFromUtf8("fresh-secret")
                }
            });
        var sut = CreateEnabledSut(client.Object, cache.Object);

        // Act
        var result = await sut.GetByIdAsync("secret-id");

        // Assert
        result.Should().Be("fresh-secret");
        cache.Verify(
            x => x.SetAbsoluteAsync(
                "secret-id/versions/latest",
                "fresh-secret",
                TimeSpan.FromHours(4),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_Should_LoadSecretWithoutCache_When_CacheIsDisabledForRead()
    {
        // Arrange
        var cache = new Mock<ICachingService>();
        var client = new Mock<SecretManagerServiceClient>();
        client
            .Setup(x => x.AccessSecretVersionAsync(
                "projects/project/secrets/secret-id/versions/latest",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccessSecretVersionResponse
            {
                Payload = new SecretPayload
                {
                    Data = ByteString.CopyFromUtf8("fresh-secret")
                }
            });
        var sut = CreateEnabledSut(client.Object, cache.Object);

        // Act
        var result = await sut.GetByIdAsync("secret-id", isCached: false);

        // Assert
        result.Should().Be("fresh-secret");
        cache.Verify(
            x => x.GetAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExistsAsync_Should_ReturnTrue_When_SecretManagerReturnsSecret()
    {
        // Arrange
        var client = new Mock<SecretManagerServiceClient>();
        client
            .Setup(x => x.GetSecretAsync("projects/project/secrets/secret-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Secret());
        var sut = CreateEnabledSut(client.Object, Mock.Of<ICachingService>());

        // Act
        var result = await sut.ExistsAsync("secret-id");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_Should_ReturnFalse_When_SecretManagerThrows()
    {
        // Arrange
        var client = new Mock<SecretManagerServiceClient>();
        client
            .Setup(x => x.GetSecretAsync("projects/project/secrets/secret-id", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("secret unavailable"));
        var sut = CreateEnabledSut(client.Object, Mock.Of<ICachingService>());

        // Act
        var result = await sut.ExistsAsync("secret-id");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task VersionExistsAsync_Should_ReturnTrue_When_SecretVersionExists()
    {
        // Arrange
        var client = new Mock<SecretManagerServiceClient>();
        client
            .Setup(x => x.GetSecretVersionAsync("projects/project/secrets/secret-id/versions/2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SecretVersion());
        var sut = CreateEnabledSut(client.Object, Mock.Of<ICachingService>());

        // Act
        var result = await sut.VersionExistsAsync("secret-id", "2");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task VersionExistsAsync_Should_ReturnFalse_When_SecretVersionLookupThrows()
    {
        // Arrange
        var client = new Mock<SecretManagerServiceClient>();
        client
            .Setup(x => x.GetSecretVersionAsync("projects/project/secrets/secret-id/versions/latest", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("version unavailable"));
        var sut = CreateEnabledSut(client.Object, Mock.Of<ICachingService>());

        // Act
        var result = await sut.VersionExistsAsync("secret-id");

        // Assert
        result.Should().BeFalse();
    }

    private static GcpSecretService CreateDisabledSut() =>
        new(
            null,
            Mock.Of<ILogger<GcpSecretService>>(),
            Mock.Of<ICachingService>(),
            Options.Create(new GcpOptions
            {
                SecretManagerSettings = new SecretManagerOptions
                {
                    IsUseSecret = false
                }
            }),
            "projects/project/secrets");

    private static GcpSecretService CreateEnabledSut(
        SecretManagerServiceClient client,
        ICachingService cachingService) =>
        new(
            client,
            Mock.Of<ILogger<GcpSecretService>>(),
            cachingService,
            Options.Create(new GcpOptions
            {
                SecretManagerSettings = new SecretManagerOptions
                {
                    IsUseSecret = true
                }
            }),
            "projects/project/secrets");

    private sealed class FakeSecretVersionPagedAsyncEnumerable
        : PagedAsyncEnumerable<ListSecretVersionsResponse, SecretVersion>
    {
        private readonly IReadOnlyList<SecretVersion> _versions;

        public FakeSecretVersionPagedAsyncEnumerable(IReadOnlyList<SecretVersion> versions)
        {
            _versions = versions;
        }

        public override IAsyncEnumerable<ListSecretVersionsResponse> AsRawResponses()
        {
            return ReadRawResponsesAsync();
        }

        public override Task<Page<SecretVersion>> ReadPageAsync(
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new Page<SecretVersion>(_versions.Take(pageSize), string.Empty));
        }

        public override IAsyncEnumerator<SecretVersion> GetAsyncEnumerator(
            CancellationToken cancellationToken = default)
        {
            return ReadVersionsAsync(cancellationToken).GetAsyncEnumerator(cancellationToken);
        }

        private async IAsyncEnumerable<ListSecretVersionsResponse> ReadRawResponsesAsync()
        {
            await Task.Yield();

            yield return new ListSecretVersionsResponse
            {
                Versions =
                {
                    _versions
                }
            };
        }

        private async IAsyncEnumerable<SecretVersion> ReadVersionsAsync(
            [EnumeratorCancellation]
            CancellationToken cancellationToken = default)
        {
            foreach (var version in _versions)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Yield();

                yield return version;
            }
        }
    }
}
