using Google.Cloud.SecretManager.V1;
using System.Reflection;

namespace Be.Haven.Tests.Shared.Be.Haven.Core.Extensions.Configurations;

public sealed class SecretConfigurationExtensionsTests
{
    [Fact]
    public async Task ApplySecretsAsync_Should_ReturnOriginalConfiguration_When_SecretManagerIsDisabled()
    {
        // Arrange
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            [$"{GCP_SETTINGS}:SecretManagerSettings:IsUseSecret"] = "false",
            [$"{AUTH_SETTINGS}:SecretKey"] = "local-secret-value"
        });

        // Act
        var result = await configuration.ApplySecretsAsync();

        // Assert
        result.Should().BeSameAs(configuration);
        result[$"{AUTH_SETTINGS}:SecretKey"].Should().Be("local-secret-value");
    }

    [Fact]
    public async Task ApplySecretsAsync_Should_ReturnOriginalConfiguration_When_SecretManagerHasNoReferences()
    {
        // Arrange
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            [$"{GCP_SETTINGS}:ProjectNumber"] = "123456",
            [$"{GCP_SETTINGS}:SecretManagerSettings:IsUseSecret"] = "true",
            [$"{GCP_SETTINGS}:SecretManagerSettings:Location"] = "global"
        });

        // Act
        var result = await configuration.ApplySecretsAsync();

        // Assert
        result.Should().BeSameAs(configuration);
    }

    [Fact]
    public void GetSecretIds_Should_ReturnConfiguredSecretReferences_When_ValuesArePresent()
    {
        // Arrange
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            [$"{AUTH_SETTINGS}:SecretKey"] = "auth-secret-id",
            [$"{EMAIL_SETTINGS}:ApiKey"] = "email-secret-id",
            [$"{EMAIL_SETTINGS}:Password"] = "   "
        });

        // Act
        var result = InvokePrivateStatic<IEnumerable<KeyValuePair<string, string>>>(
            "GetSecretIds",
            configuration).ToList();

        // Assert
        result.Should().Contain(new KeyValuePair<string, string>(
            $"{AUTH_SETTINGS}:SecretKey",
            "auth-secret-id"));
        result.Should().Contain(new KeyValuePair<string, string>(
            $"{EMAIL_SETTINGS}:ApiKey",
            "email-secret-id"));
        result.Should().NotContain(x => x.Key == $"{EMAIL_SETTINGS}:Password");
    }

    [Fact]
    public async Task LoadValuesAsync_Should_ReturnResolvedConfigurationOverlay_When_SecretManagerReturnsValues()
    {
        // Arrange
        var client = new Mock<SecretManagerServiceClient>();
        client
            .Setup(x => x.AccessSecretVersionAsync(
                "projects/project/secrets/auth-secret/versions/latest",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccessSecretVersionResponse
            {
                Payload = new SecretPayload
                {
                    Data = ByteString.CopyFromUtf8("auth-value")
                }
            });
        client
            .Setup(x => x.AccessSecretVersionAsync(
                "projects/project/secrets/email-secret/versions/latest",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccessSecretVersionResponse
            {
                Payload = new SecretPayload
                {
                    Data = ByteString.CopyFromUtf8("email-value")
                }
            });
        var secretReferences = new List<KeyValuePair<string, string>>
        {
            new($"{AUTH_SETTINGS}:SecretKey", "auth-secret"),
            new($"{EMAIL_SETTINGS}:ApiKey", "email-secret")
        };

        // Act
        var result = await InvokePrivateStaticTask<Dictionary<string, string>>(
            "LoadValuesAsync",
            client.Object,
            "projects/project/secrets",
            secretReferences,
            CancellationToken.None);

        // Assert
        result[$"{AUTH_SETTINGS}:SecretKey"].Should().Be("auth-value");
        result[$"{EMAIL_SETTINGS}:ApiKey"].Should().Be("email-value");
    }

    [Fact]
    public async Task ApplySecretsAsync_Should_ReturnConfigurationOverlay_When_SecretManagerIsEnabledAndReferencesExist()
    {
        // Arrange
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            [$"{GCP_SETTINGS}:ProjectNumber"] = "123456",
            [$"{GCP_SETTINGS}:SecretManagerSettings:IsUseSecret"] = "true",
            [$"{GCP_SETTINGS}:SecretManagerSettings:Location"] = "global",
            [$"{AUTH_SETTINGS}:SecretKey"] = "auth-secret"
        });
        var client = new Mock<SecretManagerServiceClient>();
        client
            .Setup(x => x.AccessSecretVersionAsync(
                "projects/123456/secrets/auth-secret/versions/latest",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccessSecretVersionResponse
            {
                Payload = new SecretPayload
                {
                    Data = ByteString.CopyFromUtf8("resolved-auth-secret")
                }
            });
        Func<string, SecretManagerServiceClient> buildClient = location =>
        {
            location.Should().Be("global");

            return client.Object;
        };

        // Act
        var result = await InvokePrivateStaticTask<IConfiguration>(
            "ApplySecretsAsync",
            configuration,
            buildClient,
            CancellationToken.None);

        // Assert
        result.Should().NotBeSameAs(configuration);
        result[$"{AUTH_SETTINGS}:SecretKey"].Should().Be("resolved-auth-secret");
    }

    [Fact]
    public async Task LoadValueAsync_Should_WrapExceptionWithConfigPath_When_SecretManagerFails()
    {
        // Arrange
        var client = new Mock<SecretManagerServiceClient>();
        client
            .Setup(x => x.AccessSecretVersionAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("gcp failed"));
        var secretReference = new KeyValuePair<string, string>(
            $"{AUTH_SETTINGS}:SecretKey",
            "auth-secret");

        // Act
        var action = () => InvokePrivateStaticTask<KeyValuePair<string, string>>(
            "LoadValueAsync",
            client.Object,
            "projects/project/secrets",
            secretReference,
            CancellationToken.None);

        // Assert
        var exception = await action.Should().ThrowAsync<InvalidOperationException>();
        exception.Which.Message.Should().Contain($"{AUTH_SETTINGS}:SecretKey");
        exception.Which.Message.Should().Contain("auth-secret");
        exception.Which.InnerException.Should().BeOfType<InvalidOperationException>();
    }

    private static IConfiguration CreateConfiguration(Dictionary<string, string> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    private static T InvokePrivateStatic<T>(string methodName, params object[] parameters)
    {
        var method = typeof(SecretConfigurationExtensions).GetMethod(
            methodName,
            BindingFlags.Static | BindingFlags.NonPublic);

        return (T)method.Invoke(null, parameters);
    }

    private static async Task<T> InvokePrivateStaticTask<T>(
        string methodName,
        params object[] parameters)
    {
        var method = typeof(SecretConfigurationExtensions)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(x => x.Name == methodName && x.GetParameters().Length == parameters.Length);
        var task = (Task<T>)method.Invoke(null, parameters);

        return await task;
    }
}
