namespace Be.Haven.Tests.Shared.Be.Haven.Core.Helpers;

public sealed class EnumConvertHelperTests
{
    [Fact]
    public void ConvertIntToEnum_Should_ReturnEnum_When_ValueIsDefined()
    {
        // Act
        var result = EnumConvertHelper<CoreSampleStatusEnum>.ConvertIntToEnum(1);

        // Assert
        result.Should().Be(CoreSampleStatusEnum.Pending);
    }

    [Fact]
    public void ConvertIntToEnum_Should_ThrowArgumentException_When_ValueIsUndefined()
    {
        // Act
        var act = () => EnumConvertHelper<CoreSampleStatusEnum>.ConvertIntToEnum(999);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("999 is not a valid enum.");
    }

    [Fact]
    public void ConvertStringToEnum_Should_ReturnEnum_When_NameIsDefined()
    {
        // Act
        var result = EnumConvertHelper<CoreSampleStatusEnum>.ConvertStringToEnum(nameof(CoreSampleStatusEnum.Approved));

        // Assert
        result.Should().Be(CoreSampleStatusEnum.Approved);
    }

    [Fact]
    public void ConvertStringToEnum_Should_ThrowArgumentException_When_NameIsUndefined()
    {
        // Act
        var act = () => EnumConvertHelper<CoreSampleStatusEnum>.ConvertStringToEnum("Missing");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Missing is not a valid enum.");
    }

    [Fact]
    public void GetEnumName_Should_ReturnName_When_EnumIsProvided()
    {
        // Act
        var result = EnumConvertHelper<CoreSampleStatusEnum>.GetEnumName(CoreSampleStatusEnum.Pending);

        // Assert
        result.Should().Be(nameof(CoreSampleStatusEnum.Pending));
    }

    [Fact]
    public void GetEnumValue_Should_ReturnNumericValue_When_EnumIsProvided()
    {
        // Act
        var result = EnumConvertHelper<CoreSampleStatusEnum>.GetEnumValue(CoreSampleStatusEnum.Approved);

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public void GetEnumDescription_Should_ReturnDescription_When_NameHasDescriptionAttribute()
    {
        // Act
        var result = EnumConvertHelper<CoreSampleStatusEnum>.GetEnumDescription(nameof(CoreSampleStatusEnum.Pending));

        // Assert
        result.Should().Be("Pending description");
    }

    [Fact]
    public void GetEnumDescription_Should_ReturnEmptyString_When_NameIsMissingOrHasNoDescription()
    {
        // Act
        var missingResult = EnumConvertHelper<CoreSampleStatusEnum>.GetEnumDescription("Missing");
        var noDescriptionResult = EnumConvertHelper<CoreSampleStatusEnum>.GetEnumDescription(nameof(CoreSampleStatusEnum.Approved));

        // Assert
        missingResult.Should().BeEmpty();
        noDescriptionResult.Should().BeEmpty();
    }
}
