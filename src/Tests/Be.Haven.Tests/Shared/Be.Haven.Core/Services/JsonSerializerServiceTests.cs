namespace Be.Haven.Tests.Shared.Be.Haven.Core.Services;

public sealed class JsonSerializerServiceTests
{
    [Fact]
    public void Serialize_Should_ReturnCamelCaseJsonAndIgnoreNullValues_When_ObjectHasPascalProperties()
    {
        // Arrange
        var sut = new JsonSerializerService();
        var model = new CoreSerializationModel
        {
            DisplayName = "Nguyen Van A",
            OptionalText = null
        };

        // Act
        var result = sut.Serialize(model);

        // Assert
        result.Should().Contain("\"displayName\":\"Nguyen Van A\"");
        result.Should().NotContain("optionalText");
    }

    [Fact]
    public void Serialize_Should_ReturnNull_When_ObjectIsNull()
    {
        // Arrange
        var sut = new JsonSerializerService();

        // Act
        var result = sut.Serialize<CoreSerializationModel>(null);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void SerializeIgnoreToPascalProperties_Should_ReturnPascalCaseJson_When_ObjectHasPascalProperties()
    {
        // Arrange
        var sut = new JsonSerializerService();
        var model = new CoreSerializationModel
        {
            DisplayName = "Nguyen Van A"
        };

        // Act
        var result = sut.SerializeIgnoreToPascalProperties(model);

        // Assert
        result.Should().Contain("\"DisplayName\":\"Nguyen Van A\"");
    }

    [Fact]
    public void Deserialize_Should_UseJsonPropertyAttribute_When_JsonUsesExternalName()
    {
        // Arrange
        var sut = new JsonSerializerService();
        var json = """{"external_name":"external value"}""";

        // Act
        var result = sut.Deserialize<CoreSerializationAttributeModel>(json);

        // Assert
        result.InternalName.Should().Be("external value");
    }

    [Fact]
    public void Deserialize_Should_ReturnDefault_When_JsonIsEmpty()
    {
        // Arrange
        var sut = new JsonSerializerService();

        // Act
        var result = sut.Deserialize<CoreSerializationModel>(string.Empty);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ValidateAndDeserialize_Should_ReturnSuccessResponse_When_JsonIsValid()
    {
        // Arrange
        var sut = new JsonSerializerService();
        var json = """{"displayName":"Nguyen Van A"}""";

        // Act
        var result = sut.ValidateAndDeserialize<CoreSerializationModel>(json);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.DisplayName.Should().Be("Nguyen Van A");
    }

    [Fact]
    public void DeserializeIgnoreProperties_Should_IgnoreJsonPropertyAttribute_When_JsonUsesClrName()
    {
        // Arrange
        var sut = new JsonSerializerService();
        var json = """{"InternalName":"internal value"}""";

        // Act
        var result = sut.DeserializeIgnoreProperties<CoreSerializationAttributeModel>(json);

        // Assert
        result.InternalName.Should().Be("internal value");
    }

    [Fact]
    public void DeserializeIgnoreToPascalProperties_Should_ReadPascalCaseJson_When_JsonUsesClrName()
    {
        // Arrange
        var sut = new JsonSerializerService();
        var json = """{"DisplayName":"Nguyen Van A"}""";

        // Act
        var result = sut.DeserializeIgnoreToPascalProperties<CoreSerializationModel>(json);

        // Assert
        result.DisplayName.Should().Be("Nguyen Van A");
    }

    [Fact]
    public void DeserializeList_Should_ReturnTypedList_When_JsonArrayUsesPascalCase()
    {
        // Arrange
        var sut = new JsonSerializerService();
        var json = """[{"DisplayName":"A"},{"DisplayName":"B"}]""";

        // Act
        var result = sut.DeserializeList<CoreSerializationModel>(json);

        // Assert
        result.Select(x => x.DisplayName).Should().Equal("A", "B");
    }

    [Fact]
    public void DeserializeList_Should_ReturnEmptyList_When_JsonIsBlank()
    {
        // Arrange
        var sut = new JsonSerializerService();

        // Act
        var result = sut.DeserializeList<CoreSerializationModel>("   ");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ValidateAndDeserialize_Should_ReturnFailureResponse_When_JsonIsInvalid()
    {
        // Arrange
        var sut = new JsonSerializerService();

        // Act
        var result = sut.ValidateAndDeserialize<CoreSerializationModel>("{");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Contain("Invalid JSON format");
    }
}
