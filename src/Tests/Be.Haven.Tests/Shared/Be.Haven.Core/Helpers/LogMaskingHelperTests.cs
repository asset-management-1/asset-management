namespace Be.Haven.Tests.Shared.Be.Haven.Core.Helpers;

public sealed class LogMaskingHelperTests
{
    [Fact]
    public void MaskValue_Should_ReturnDefaultMask_When_NoTrailingCharactersAreVisible()
    {
        // Act
        var result = LogMaskingHelper.MaskValue("secret-value");

        // Assert
        result.Should().Be("******");
    }

    [Fact]
    public void MaskValue_Should_PreserveTrailingCharacters_When_VisibleTrailingCharactersAreConfigured()
    {
        // Act
        var result = LogMaskingHelper.MaskValue("secret-value", 5, "***");

        // Assert
        result.Should().Be("***value");
    }

    [Fact]
    public void MaskValue_Should_ReturnOriginalInput_When_InputIsBlank()
    {
        // Act
        var result = LogMaskingHelper.MaskValue("   ");

        // Assert
        result.Should().Be("   ");
    }

    [Fact]
    public void MaskValue_Should_ReturnMaskOnly_When_InputIsShorterThanVisibleSuffix()
    {
        // Act
        var result = LogMaskingHelper.MaskValue("abc", 10, "***");

        // Assert
        result.Should().Be("***");
    }

    [Fact]
    public void MaskAllSensitiveData_Should_MaskSensitiveJsonProperties_When_InputIsJson()
    {
        // Arrange
        var input = """{"email":"user@test.com","password":"super-secret","nested":{"accessToken":"token-12345"}}""";

        // Act
        var result = LogMaskingHelper.MaskAllSensitiveData(input, visibleTrailingCharacters: 4);

        // Assert
        var json = JObject.Parse(result);
        json["email"]?.Value<string>().Should().Be("user@test.com");
        json["password"]?.Value<string>().Should().Be("******cret");
        json["nested"]?["accessToken"]?.Value<string>().Should().Be("******2345");
    }

    [Fact]
    public void MaskAllSensitiveData_Should_MaskJsonBlockInsideText_When_TextContainsJsonPayload()
    {
        // Arrange
        var input = """provider failed: {"refreshToken":"refresh-123"} after retry""";

        // Act
        var result = LogMaskingHelper.MaskAllSensitiveData(input);

        // Assert
        result.Should().Contain("provider failed:");
        result.Should().Contain("\"refreshToken\":\"******\"");
        result.Should().NotContain("refresh-123");
    }

    [Fact]
    public void MaskAllSensitiveData_Should_MaskSensitiveValuesInsideJsonArray_When_InputIsArrayJson()
    {
        // Arrange
        var input = """[{"token":"abc123"},{"password":"secret"}]""";

        // Act
        var result = LogMaskingHelper.MaskAllSensitiveData(input, "***", 2);

        // Assert
        result.Should().Be("""[{"token":"***23"},{"password":"***et"}]""");
    }

    [Fact]
    public void MaskAllSensitiveData_Should_MaskSensitiveStringValuesInsideJsonArray_When_ArrayContainsPlainTextSecrets()
    {
        // Arrange
        var input = """{"token":"abc123","messages":["password=plain-secret",""]}""";

        // Act
        var result = LogMaskingHelper.MaskAllSensitiveData(input);

        // Assert
        var json = JObject.Parse(result);
        json["token"]?.Value<string>().Should().Be("******");
        json["messages"]?[0]?.Value<string>().Should().Be("password=******");
        json["messages"]?[1]?.Value<string>().Should().Be(string.Empty);
    }

    [Fact]
    public void MaskAllSensitiveData_Should_FallbackToPlainTextMasking_When_JsonLikeInputCannotBeParsed()
    {
        // Arrange
        var input = """{"password": } token=abc123""";

        // Act
        var result = LogMaskingHelper.MaskAllSensitiveData(input);

        // Assert
        result.Should().Contain("token=******");
        result.Should().NotContain("abc123");
    }

    [Fact]
    public void TryMaskFullJson_Should_ReturnOriginalText_When_JsonCannotBeParsed()
    {
        // Arrange
        var input = """{"token":}""";

        // Act
        var result = InvokePrivateStatic<string>(
            "TryMaskFullJson",
            input,
            "******",
            0);

        // Assert
        result.Should().Be(input);
    }

    [Fact]
    public void MaskAllSensitiveData_Should_SkipNonJsonBlocks_When_TextContainsBracesWithoutJsonShape()
    {
        // Arrange
        var input = "payload {not-json} token=abc123";

        // Act
        var result = LogMaskingHelper.MaskAllSensitiveData(input);

        // Assert
        result.Should().Contain("{not-json}");
        result.Should().Contain("token=******");
    }

    [Fact]
    public void MaskAllSensitiveData_Should_IgnoreBracesInsideEscapedJsonStrings_When_TextContainsJsonBlock()
    {
        // Arrange
        var input = """failed payload {"token":"abc\"{123}","note":"not secret"} done""";

        // Act
        var result = LogMaskingHelper.MaskAllSensitiveData(input, visibleTrailingCharacters: 3);

        // Assert
        result.Should().Contain("\"token\":\"******23}\"");
        result.Should().Contain("\"note\":\"not secret\"");
    }

    [Fact]
    public void MaskAllSensitiveData_Should_FallbackToPlainTextMasking_When_JsonBlockHasMismatchedBrackets()
    {
        // Arrange
        var input = """failed payload {"password":"secret"] token=abc123""";

        // Act
        var result = LogMaskingHelper.MaskAllSensitiveData(input);

        // Assert
        result.Should().Contain("token=******");
        result.Should().NotContain("abc123");
    }

    [Fact]
    public void MaskAllSensitiveData_Should_MaskPlainTextSecrets_When_InputIsNotJson()
    {
        // Arrange
        var input = "password=abc123; token: bearer-token";

        // Act
        var result = LogMaskingHelper.MaskAllSensitiveData(input);

        // Assert
        result.Should().Contain("password=******");
        result.Should().Contain("token: ******");
        result.Should().NotContain("abc123");
        result.Should().NotContain("bearer-token");
    }

    [Fact]
    public void MaskAllSensitiveData_Should_ReturnOriginalInput_When_NoSensitiveKeywordExists()
    {
        // Arrange
        var input = "normal log payload";

        // Act
        var result = LogMaskingHelper.MaskAllSensitiveData(input);

        // Assert
        result.Should().Be(input);
    }

    [Fact]
    public void LooksLikeJson_Should_ReturnFalse_When_InputIsBlank()
    {
        // Act
        var result = InvokePrivateStatic<bool>(
            "LooksLikeJson",
            "   ");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void MaskPlainText_Should_ReturnOriginalText_When_InputIsBlank()
    {
        // Act
        var result = InvokePrivateStatic<string>(
            "MaskPlainText",
            "   ",
            "******",
            0);

        // Assert
        result.Should().Be("   ");
    }

    private static T InvokePrivateStatic<T>(string methodName, params object[] parameters)
    {
        var method = typeof(LogMaskingHelper).GetMethod(
            methodName,
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

        return (T)method.Invoke(null, parameters);
    }
}
