namespace Be.Haven.Tests.Shared.Be.Haven.Core.Extensions.Objects;

public sealed class ObjectExtensionTests
{
    [Fact]
    public void AsDictionary_Should_ReturnPublicPropertyValues_WithPrefixAndSuffix()
    {
        // Arrange
        var source = new CoreSampleQueryModel
        {
            Id = 7,
            Name = "Haven"
        };

        // Act
        var result = source.AsDictionary("pre_", "_suf");

        // Assert
        result.Should().ContainKey("pre_Id_suf").WhoseValue.Should().Be(7);
        result.Should().ContainKey("pre_Name_suf").WhoseValue.Should().Be("Haven");
    }

    [Fact]
    public void SanitizeArgs_Should_ReturnNull_When_DictionaryIsEmpty()
    {
        // Arrange
        var args = new Dictionary<string, object>();

        // Act
        var result = args.SanitizeArgs();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void SanitizeArgs_Should_ReturnSingleSanitizedFileMetadata_When_DictionaryHasOneFormFile()
    {
        // Arrange
        var file = CreateFormFile("avatar.jpg", "image/jpeg", 10);
        var args = new Dictionary<string, object>
        {
            ["AvatarFile"] = file
        };

        // Act
        var result = JObject.FromObject(args.SanitizeArgs());

        // Assert
        result["FileName"]?.Value<string>().Should().Be("avatar.jpg");
        result["ContentType"]?.Value<string>().Should().Be("image/jpeg");
        result["Length"]?.Value<long>().Should().Be(10);
    }

    [Fact]
    public void SanitizeArgs_Should_ReturnDictionaryWithSanitizedFiles_When_DictionaryHasMultipleValues()
    {
        // Arrange
        var files = new[]
        {
            CreateFormFile("front.jpg", "image/jpeg", 5),
            CreateFormFile("back.jpg", "image/jpeg", 6)
        };
        var args = new Dictionary<string, object>
        {
            ["Files"] = files,
            ["Name"] = "Vehicle"
        };

        // Act
        var result = JObject.FromObject(args.SanitizeArgs());

        // Assert
        result["Name"]?.Value<string>().Should().Be("Vehicle");
        result["Files"]?.Should().HaveCount(2);
        result["Files"]?[0]?["FileName"]?.Value<string>().Should().Be("front.jpg");
    }

    [Fact]
    public void SanitizeArgs_Should_ReturnNull_When_SingleArgumentValueIsNull()
    {
        // Arrange
        var args = new Dictionary<string, object>
        {
            ["Value"] = null
        };

        // Act
        var result = args.SanitizeArgs();

        // Assert
        result.Should().BeNull();
    }

    private static FormFile CreateFormFile(string fileName, string contentType, long length)
    {
        var content = Encoding.UTF8.GetBytes(new string('a', (int)length));
        var stream = new MemoryStream(content);

        return new FormFile(stream, 0, content.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}
