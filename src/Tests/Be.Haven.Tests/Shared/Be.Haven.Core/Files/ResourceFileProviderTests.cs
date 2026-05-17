using Be.Haven.ApiCommon.Extensions;
using Be.Haven.Core.Files;

namespace Be.Haven.Tests.Shared.Be.Haven.Core.Files;

public sealed class ResourceFileProviderTests
{
    [Fact]
    public async Task GetAsync_Should_ReturnEmbeddedResourceBytes_When_KeyMapsToExistingResource()
    {
        // Arrange
        var sut = new ResourceFileProvider(
            typeof(ApplicationRegistration).Assembly,
            new Dictionary<string, string>
            {
                ["swagger-script"] = SwaggerUiConstants.OPERATION_SEARCH_SCRIPT_RESOURCE_NAME
            },
            Mock.Of<ILogger<ResourceFileProvider>>());

        // Act
        var result = await sut.GetAsync("swagger-script", CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        Encoding.UTF8.GetString(result).Should().Contain("querySelector");
    }

    [Fact]
    public async Task GetAsync_Should_ThrowInvalidOperationException_When_KeyIsNotConfigured()
    {
        // Arrange
        var sut = new ResourceFileProvider(
            typeof(ApplicationRegistration).Assembly,
            new Dictionary<string, string>(),
            Mock.Of<ILogger<ResourceFileProvider>>());

        // Act
        Func<Task> action = async () => await sut.GetAsync("missing-key", CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*missing-key*");
    }

    [Fact]
    public async Task GetAsync_Should_ThrowInvalidOperationException_When_ResourceNameDoesNotExist()
    {
        // Arrange
        var sut = new ResourceFileProvider(
            typeof(ApplicationRegistration).Assembly,
            new Dictionary<string, string>
            {
                ["missing-resource"] = "Be.Haven.ApiCommon.DoesNotExist.js"
            },
            Mock.Of<ILogger<ResourceFileProvider>>());

        // Act
        Func<Task> action = async () => await sut.GetAsync("missing-resource", CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Be.Haven.ApiCommon.DoesNotExist.js*");
    }
}
