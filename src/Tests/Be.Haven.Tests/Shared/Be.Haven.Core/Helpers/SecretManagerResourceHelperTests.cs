namespace Be.Haven.Tests.Shared.Be.Haven.Core.Helpers;

public sealed class SecretManagerResourceHelperTests
{
    [Fact]
    public void BuildClient_Should_CreateGlobalClient_When_LocationIsGlobal()
    {
        // Arrange
        using var credentialFile = new ServiceAccountCredentialFile();
        var previousCredentialPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialFile.Path);

        try
        {
            // Act
            var result = SecretManagerResourceHelper.BuildClient("global");

            // Assert
            result.Should().NotBeNull();
        }
        finally
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", previousCredentialPath);
        }
    }

    [Fact]
    public void BuildClient_Should_CreateRegionalClient_When_LocationIsRegional()
    {
        // Arrange
        using var credentialFile = new ServiceAccountCredentialFile();
        var previousCredentialPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialFile.Path);

        try
        {
            // Act
            var result = SecretManagerResourceHelper.BuildClient("asia-southeast1");

            // Assert
            result.Should().NotBeNull();
        }
        finally
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", previousCredentialPath);
        }
    }

    [Theory]
    [MemberData(nameof(GlobalLocations))]
    public void BuildResourcePrefix_Should_ReturnGlobalPrefix_When_LocationIsGlobalOrBlank(string location)
    {
        // Act
        var result = SecretManagerResourceHelper.BuildResourcePrefix("haven-project", location);

        // Assert
        result.Should().Be("projects/haven-project/secrets");
    }

    [Fact]
    public void BuildResourcePrefix_Should_ReturnRegionalPrefix_When_LocationIsRegional()
    {
        // Act
        var result = SecretManagerResourceHelper.BuildResourcePrefix("haven-project", "asia-southeast1");

        // Assert
        result.Should().Be("projects/haven-project/locations/asia-southeast1/secrets");
    }

    public static IEnumerable<object[]> GlobalLocations()
    {
        yield return [null];
        yield return [string.Empty];
        yield return ["global"];
        yield return ["GLOBAL"];
    }

    private sealed class ServiceAccountCredentialFile : IDisposable
    {
        public ServiceAccountCredentialFile()
        {
            using var rsa = RSA.Create(2048);
            var privateKey = rsa.ExportPkcs8PrivateKeyPem().Replace("\n", "\\n");
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"haven-secret-manager-{Guid.NewGuid():N}.json");
            var json =
                $$"""
                {
                  "type": "service_account",
                  "project_id": "haven-test",
                  "private_key_id": "test-key",
                  "private_key": "{{privateKey}}",
                  "client_email": "haven-test@haven-test.iam.gserviceaccount.com",
                  "client_id": "1234567890",
                  "token_uri": "https://oauth2.googleapis.com/token"
                }
                """;

            File.WriteAllText(Path, json);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (File.Exists(Path))
            {
                File.Delete(Path);
            }
        }
    }
}
