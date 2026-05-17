namespace Be.Haven.Tests.Shared.Be.Haven.Core.Helpers;

public sealed class StringConvertHelperTests
{
    [Fact]
    public void TryConvert_Should_ParsePrimitiveValues_When_InputIsValid()
    {
        // Act
        var intSuccess = StringConvertHelper.TryConvert<int>("42", out var intResult);
        var boolSuccess = StringConvertHelper.TryConvert<bool>("true", out var boolResult);
        var decimalSuccess = StringConvertHelper.TryConvert<decimal>("123.45", out var decimalResult, CultureInfo.InvariantCulture);

        // Assert
        intSuccess.Should().BeTrue();
        intResult.Should().Be(42);
        boolSuccess.Should().BeTrue();
        boolResult.Should().BeTrue();
        decimalSuccess.Should().BeTrue();
        decimalResult.Should().Be(123.45m);
    }

    [Fact]
    public void TryConvert_Should_ParseAdditionalBuiltInValues_When_InputIsValid()
    {
        // Arrange
        var expectedGuid = Guid.NewGuid();

        // Act
        var stringSuccess = StringConvertHelper.TryConvert<string>("raw-value", out var stringResult);
        var guidSuccess = StringConvertHelper.TryConvert<Guid>(expectedGuid.ToString("D"), out var guidResult);
        var longSuccess = StringConvertHelper.TryConvert<long>("9001", out var longResult);
        var doubleSuccess = StringConvertHelper.TryConvert<double>("1,234.5", out var doubleResult);
        var dateTimeSuccess = StringConvertHelper.TryConvert<DateTime>("2026-05-17T10:00:00Z", out var dateTimeResult);
        var offsetSuccess = StringConvertHelper.TryConvert<DateTimeOffset>("2026-05-17T10:00:00+07:00", out var offsetResult);

        // Assert
        stringSuccess.Should().BeTrue();
        stringResult.Should().Be("raw-value");
        guidSuccess.Should().BeTrue();
        guidResult.Should().Be(expectedGuid);
        longSuccess.Should().BeTrue();
        longResult.Should().Be(9001L);
        doubleSuccess.Should().BeTrue();
        doubleResult.Should().Be(1234.5);
        dateTimeSuccess.Should().BeTrue();
        dateTimeResult.Should().Be(new DateTime(2026, 5, 17, 10, 0, 0, DateTimeKind.Utc));
        offsetSuccess.Should().BeTrue();
        offsetResult.Should().Be(new DateTimeOffset(2026, 5, 17, 10, 0, 0, TimeSpan.FromHours(7)));
    }

    [Fact]
    public void TryConvert_Should_ParseEnumByNameOrNumber_When_InputMatchesEnum()
    {
        // Act
        var nameSuccess = StringConvertHelper.TryConvert<CoreSampleStatusEnum>("approved", out var nameResult);
        var numberSuccess = StringConvertHelper.TryConvert<CoreSampleStatusEnum>("1", out var numberResult);

        // Assert
        nameSuccess.Should().BeTrue();
        nameResult.Should().Be(CoreSampleStatusEnum.Approved);
        numberSuccess.Should().BeTrue();
        numberResult.Should().Be(CoreSampleStatusEnum.Pending);
    }

    [Fact]
    public void TryConvert_Should_ReturnFalse_When_EnumInputDoesNotMatchNameOrNumber()
    {
        // Act
        var success = StringConvertHelper.TryConvert<CoreSampleStatusEnum>("missing", out var result);

        // Assert
        success.Should().BeFalse();
        result.Should().Be(default);
    }

    [Fact]
    public void TryConvert_Should_DeserializeJsonObject_When_TargetIsComplexType()
    {
        // Arrange
        var json = """{"requiredCode":"A1","name":"Child"}""";

        // Act
        var success = StringConvertHelper.TryConvert<CoreSampleChildModel>(json, out var result);

        // Assert
        success.Should().BeTrue();
        result.RequiredCode.Should().Be("A1");
        result.Name.Should().Be("Child");
    }

    [Fact]
    public void TryConvert_Should_ReturnFalse_When_ComplexTypeJsonIsInvalid()
    {
        // Act
        var success = StringConvertHelper.TryConvert<CoreSampleChildModel>("{", out var result);

        // Assert
        success.Should().BeFalse();
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-number")]
    public void TryConvert_Should_ReturnFalse_When_InputCannotBeConverted(string input)
    {
        // Act
        var success = StringConvertHelper.TryConvert<int>(input, out var result);

        // Assert
        success.Should().BeFalse();
        result.Should().Be(default);
    }

    [Fact]
    public void TrimGoogleStoragePrefix_Should_RemoveStorageBaseUrl_When_UrlUsesGcpPrefix()
    {
        // Arrange
        var url = "https://storage.googleapis.com/haven-assets/avatar.jpg";

        // Act
        var result = StringConvertHelper.TrimGoogleStoragePrefix(url);

        // Assert
        result.Should().Be("haven-assets/avatar.jpg");
    }

    [Fact]
    public void TrimGoogleStoragePrefix_Should_ReturnOriginalValue_When_UrlIsBlank()
    {
        // Act
        var result = StringConvertHelper.TrimGoogleStoragePrefix("   ");

        // Assert
        result.Should().Be("   ");
    }

    [Fact]
    public void Truncate_Should_ReturnEmptyString_When_TextIsNull()
    {
        // Act
        var result = StringConvertHelper.Truncate(null, 3);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Truncate_Should_ReturnMaxLengthPrefix_When_TextIsLongerThanMax()
    {
        // Act
        var result = StringConvertHelper.Truncate("abcdef", 3);

        // Assert
        result.Should().Be("abc");
    }

    [Fact]
    public void ToKebabCase_Should_ConvertPascalCaseToKebabCase()
    {
        // Act
        var result = StringConvertHelper.ToKebabCase("VehiclePublicId");

        // Assert
        result.Should().Be("vehicle-public-id");
    }

    [Fact]
    public void ToCamelCase_Should_ConvertKebabCaseToCamelCase()
    {
        // Act
        var result = StringConvertHelper.ToCamelCase("vehicle-public-id");

        // Assert
        result.Should().Be("vehiclePublicId");
    }
}
