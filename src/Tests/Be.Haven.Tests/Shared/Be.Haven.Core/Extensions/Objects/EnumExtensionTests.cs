namespace Be.Haven.Tests.Shared.Be.Haven.Core.Extensions.Objects;

public sealed class EnumExtensionTests
{
    [Fact]
    public void ToInt_Should_ReturnUnderlyingValue_When_EnumIsDefined()
    {
        // Act
        var result = CoreSampleStatusEnum.Approved.ToInt();

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public void TryFromInt_Should_ReturnEnumValue_When_ValueIsDefined()
    {
        // Act
        var success = EnumExtension.TryFromInt<CoreSampleStatusEnum>(1, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().Be(CoreSampleStatusEnum.Pending);
    }

    [Fact]
    public void TryFromInt_Should_ReturnFalse_When_ValueIsUndefined()
    {
        // Act
        var success = EnumExtension.TryFromInt<CoreSampleStatusEnum>(999, out var result);

        // Assert
        success.Should().BeFalse();
        result.Should().Be(default);
    }

    [Fact]
    public void TryParseIgnoreCase_Should_ParseDefinedName_When_CasingDiffers()
    {
        // Act
        var success = EnumExtension.TryParseIgnoreCase<CoreSampleStatusEnum>("approved", out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().Be(CoreSampleStatusEnum.Approved);
    }

    [Fact]
    public void GetDescription_Should_ReturnDescriptionAttribute_When_AttributeExists()
    {
        // Act
        var result = CoreSampleStatusEnum.Pending.GetDescription();

        // Assert
        result.Should().Be("Pending description");
    }

    [Fact]
    public void GetDisplayName_Should_ReturnDisplayAttribute_When_AttributeExists()
    {
        // Act
        var result = CoreSampleStatusEnum.Pending.GetDisplayName();

        // Assert
        result.Should().Be("Pending display");
    }

    [Fact]
    public void ToMasterDataCode_Should_ReturnUppercaseName_When_ValueIsDefined()
    {
        // Act
        var result = CoreSampleStatusEnum.Pending.ToMasterDataCode();

        // Assert
        result.Should().Be("PENDING");
    }

    [Fact]
    public void ToMasterDataCode_Should_ReturnNull_When_ValueIsUndefined()
    {
        // Arrange
        var value = (CoreSampleStatusEnum)999;

        // Act
        var result = value.ToMasterDataCode();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ToDictionary_Should_MapEnumValuesToDescriptions_When_DescriptionModeIsEnabled()
    {
        // Act
        var result = EnumExtension.ToDictionary<CoreSampleStatusEnum>();

        // Assert
        result.Should().ContainKey(1).WhoseValue.Should().Be("Pending description");
        result.Should().ContainKey(2).WhoseValue.Should().Be(nameof(CoreSampleStatusEnum.Approved));
    }
}
