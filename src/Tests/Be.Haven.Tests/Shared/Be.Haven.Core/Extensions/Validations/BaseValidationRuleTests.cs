namespace Be.Haven.Tests.Shared.Be.Haven.Core.Extensions.Validations;

public sealed class BaseValidationRuleTests
{
    [Fact]
    public void Required_Should_FailValidation_When_StringIsWhitespace()
    {
        // Arrange
        var validator = new InlineValidator<CoreSampleValidationModel>();
        validator.RuleFor(x => x.RequiredText).Required();
        var model = new CoreSampleValidationModel
        {
            RequiredText = "   "
        };

        // Act
        var result = validator.Validate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CoreSampleValidationModel.RequiredText));
    }

    [Fact]
    public void RequiredGuid_Should_FailValidation_When_GuidIsEmpty()
    {
        // Arrange
        var validator = new InlineValidator<CoreSampleValidationModel>();
        validator.RuleFor(x => x.PublicId).RequiredGuid();

        // Act
        var result = validator.Validate(new CoreSampleValidationModel());

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CoreSampleValidationModel.PublicId));
    }

    [Fact]
    public void RequiredAndLengthRules_Should_FailValidation_When_ValuesAreMissingOrOutsideLength()
    {
        // Arrange
        var validator = new InlineValidator<CoreSampleValidationModel>();
        validator.RuleFor(x => x.RequiredText).Required<CoreSampleValidationModel, string>();
        validator.RuleFor(x => x.OptionalStatus).Required();
        validator.RuleFor(x => x.Email).MinLen(5).MaxLen(10);
        validator.RuleFor(x => x.Url).BetweenLen(3, 5);
        var model = new CoreSampleValidationModel
        {
            RequiredText = null,
            Email = "abcd",
            Url = "too-long"
        };

        // Act
        var result = validator.Validate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Select(x => x.PropertyName).Should().Contain([
            nameof(CoreSampleValidationModel.RequiredText),
            nameof(CoreSampleValidationModel.OptionalStatus),
            nameof(CoreSampleValidationModel.Email),
            nameof(CoreSampleValidationModel.Url)
        ]);
    }

    [Fact]
    public void StringRules_Should_FailValidation_When_StringValuesDoNotMatchRules()
    {
        // Arrange
        var validator = new InlineValidator<CoreSampleValidationModel>();
        validator.RuleFor(x => x.Email).EmailFormat();
        validator.RuleFor(x => x.Url).AbsoluteUrl();
        validator.RuleFor(x => x.RegexValue).MatchesRegex("^[A-Z]{3}$");
        var model = new CoreSampleValidationModel
        {
            Email = "not-email",
            Url = "relative/path",
            RegexValue = "abc"
        };

        // Act
        var result = validator.Validate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Select(x => x.PropertyName).Should().Contain([
            nameof(CoreSampleValidationModel.Email),
            nameof(CoreSampleValidationModel.Url),
            nameof(CoreSampleValidationModel.RegexValue)
        ]);
    }

    [Fact]
    public void MatchesRegex_Should_FailValidation_When_PatternIsEmpty()
    {
        // Arrange
        var validator = new InlineValidator<CoreSampleValidationModel>();
        validator.RuleFor(x => x.RegexValue).MatchesRegex(string.Empty);
        var model = new CoreSampleValidationModel
        {
            RegexValue = "ABC"
        };

        // Act
        var result = validator.Validate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CoreSampleValidationModel.RegexValue));
    }

    [Fact]
    public void NotEmptyCollection_Should_FailValidation_When_CollectionIsEmpty()
    {
        // Arrange
        var validator = new InlineValidator<CoreSampleValidationModel>();
        validator.RuleFor(x => x.Items).NotEmptyCollection();
        var model = new CoreSampleValidationModel
        {
            Items = []
        };

        // Act
        var result = validator.Validate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CoreSampleValidationModel.Items));
    }

    [Fact]
    public void ValidEnum_Should_FailValidation_When_EnumValueIsUndefined()
    {
        // Arrange
        var validator = new InlineValidator<CoreSampleValidationModel>();
        validator.RuleFor(x => x.Status).ValidEnum();
        var model = new CoreSampleValidationModel
        {
            Status = (CoreSampleStatusEnum)999
        };

        // Act
        var result = validator.Validate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CoreSampleValidationModel.Status));
    }

    [Fact]
    public void ValidEnum_Should_FailValidation_When_CsvContainsUndefinedEnumValue()
    {
        // Arrange
        var validator = new InlineValidator<CoreSampleValidationModel>();
        validator.RuleFor(x => x.StatusCsv).ValidEnum<CoreSampleValidationModel, CoreSampleStatusEnum>();
        var model = new CoreSampleValidationModel
        {
            StatusCsv = "Pending,999"
        };

        // Act
        var result = validator.Validate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CoreSampleValidationModel.StatusCsv));
    }

    [Fact]
    public void NumericRules_Should_FailValidation_When_NumberIsOutsideAllowedRange()
    {
        // Arrange
        var validator = new InlineValidator<CoreSampleValidationModel>();
        validator.RuleFor(x => x.Count).GreaterThanZero().LessThanValue(5);
        var model = new CoreSampleValidationModel
        {
            Count = 5
        };

        // Act
        var result = validator.Validate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CoreSampleValidationModel.Count));
    }

    [Fact]
    public void NumericRules_Should_FailValidation_When_NumberViolatesAllNumericComparisons()
    {
        // Arrange
        var validator = new InlineValidator<CoreSampleValidationModel>();
        validator.RuleFor(x => x.Count).NotDefault();
        validator.RuleFor(x => x.OtherCount).GreaterOrEqualZero().LessThanZero();
        validator.RuleFor(x => x.Count).LessOrEqualZero();
        validator.RuleFor(x => x.Count).GreaterThanValue(10);
        validator.RuleFor(x => x.Count).GreaterOrEqualValue(10);
        validator.RuleFor(x => x.Count).LessOrEqualValue(3);
        var model = new CoreSampleValidationModel
        {
            Count = 5,
            OtherCount = 1
        };

        // Act
        var result = validator.Validate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Select(x => x.PropertyName).Should().Contain([
            nameof(CoreSampleValidationModel.Count),
            nameof(CoreSampleValidationModel.OtherCount)
        ]);
    }

    [Fact]
    public void BetweenInclusive_Should_ThrowArgumentException_When_MinIsGreaterThanMax()
    {
        // Arrange
        var validator = new InlineValidator<CoreSampleValidationModel>();

        // Act
        var act = () => validator.RuleFor(x => x.Count).BetweenInclusive(5, 1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("min must be <= max");
    }

    [Fact]
    public void BetweenRules_Should_FailValidation_When_ValueIsOutsideRange()
    {
        // Arrange
        var validator = new InlineValidator<CoreSampleValidationModel>();
        validator.RuleFor(x => x.Count).BetweenInclusive(1, 3);
        validator.RuleFor(x => x.OtherCount).BetweenExclusive(1, 3);
        var model = new CoreSampleValidationModel
        {
            Count = 4,
            OtherCount = 3
        };

        // Act
        var result = validator.Validate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Select(x => x.PropertyName).Should().Contain([
            nameof(CoreSampleValidationModel.Count),
            nameof(CoreSampleValidationModel.OtherCount)
        ]);
    }

    [Fact]
    public void BetweenExclusive_Should_ThrowArgumentException_When_MinIsGreaterThanOrEqualToMax()
    {
        // Arrange
        var validator = new InlineValidator<CoreSampleValidationModel>();

        // Act
        var act = () => validator.RuleFor(x => x.Count).BetweenExclusive(5, 5);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("min must be < max");
    }

    [Fact]
    public void PropertyComparisonRules_Should_FailValidation_When_ValuesBreakComparisons()
    {
        // Arrange
        var validator = new InlineValidator<CoreSampleValidationModel>();
        validator.RuleFor(x => x.Count).GreaterThanProp(x => x.OtherCount);
        validator.RuleFor(x => x.Count).GreaterOrEqualProp(x => x.OtherCount);
        validator.RuleFor(x => x.OtherCount).LessThanProp(x => x.Count);
        validator.RuleFor(x => x.OtherCount).LessOrEqualProp(x => x.Count);
        var model = new CoreSampleValidationModel
        {
            Count = 1,
            OtherCount = 2
        };

        // Act
        var result = validator.Validate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Select(x => x.PropertyName).Should().Contain([
            nameof(CoreSampleValidationModel.Count),
            nameof(CoreSampleValidationModel.OtherCount)
        ]);
    }

    [Fact]
    public void DateRules_Should_FailValidation_When_StartDateIsAfterEndDate()
    {
        // Arrange
        var validator = new InlineValidator<CoreSampleValidationModel>();
        validator.RuleFor(x => x.StartAt).Before(x => x.EndAt);
        validator.RuleFor(x => x.StartDate).BeforeOrEqual(x => x.EndDate);
        var model = new CoreSampleValidationModel
        {
            StartAt = new DateTime(2026, 5, 18, 0, 0, 0, DateTimeKind.Utc),
            EndAt = new DateTime(2026, 5, 17, 0, 0, 0, DateTimeKind.Utc),
            StartDate = new DateOnly(2026, 5, 18),
            EndDate = new DateOnly(2026, 5, 17)
        };

        // Act
        var result = validator.Validate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Select(x => x.PropertyName).Should().Contain([
            nameof(CoreSampleValidationModel.StartAt),
            nameof(CoreSampleValidationModel.StartDate)
        ]);
    }

    [Fact]
    public void DateRules_Should_FailValidation_When_DateComparisonsDoNotMatch()
    {
        // Arrange
        var validator = new InlineValidator<CoreSampleValidationModel>();
        validator.RuleFor(x => x.StartAt).After(x => x.EndAt);
        validator.RuleFor(x => x.StartAt).AfterOrEqual(x => x.EndAt);
        validator.RuleFor(x => x.StartAt).EqualTo(x => x.EndAt);
        validator.RuleFor(x => x.StartAt).NotEqualTo(x => x.EndAt);
        validator.RuleFor(x => x.StartDate).After(x => x.EndDate);
        validator.RuleFor(x => x.StartDate).AfterOrEqual(x => x.EndDate);
        validator.RuleFor(x => x.StartDate).EqualTo(x => x.EndDate);
        validator.RuleFor(x => x.StartDate).NotEqualTo(x => x.EndDate);
        var model = new CoreSampleValidationModel
        {
            StartAt = new DateTime(2026, 5, 17, 0, 0, 0, DateTimeKind.Utc),
            EndAt = new DateTime(2026, 5, 18, 0, 0, 0, DateTimeKind.Utc),
            StartDate = new DateOnly(2026, 5, 17),
            EndDate = new DateOnly(2026, 5, 18)
        };

        // Act
        var result = validator.Validate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Select(x => x.PropertyName).Should().Contain([
            nameof(CoreSampleValidationModel.StartAt),
            nameof(CoreSampleValidationModel.StartDate)
        ]);
    }

    [Fact]
    public void NullableDateRules_Should_FailValidation_When_NullableDateComparisonsDoNotMatch()
    {
        // Arrange
        var validator = new InlineValidator<CoreSampleValidationModel>();
        validator.RuleFor(x => x.OptionalStartAt).Before(x => x.OptionalEndAt);
        validator.RuleFor(x => x.OptionalStartAt).BeforeOrEqual(x => x.OptionalEndAt);
        validator.RuleFor(x => x.OptionalStartAt).After(x => x.OptionalEndAt);
        validator.RuleFor(x => x.OptionalStartAt).AfterOrEqual(x => x.OptionalEndAt);
        validator.RuleFor(x => x.OptionalStartAt).EqualTo(x => x.OptionalEndAt);
        validator.RuleFor(x => x.OptionalStartAt).NotEqualTo(x => x.OptionalEndAt);
        validator.RuleFor(x => x.OptionalEndAt).AfterNow();
        var model = new CoreSampleValidationModel
        {
            OptionalStartAt = null,
            OptionalEndAt = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = validator.Validate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CoreSampleValidationModel.OptionalStartAt));
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CoreSampleValidationModel.OptionalEndAt));
    }

    [Fact]
    public void CommonRules_Should_PassValidation_When_ValuesSatisfyRules()
    {
        // Arrange
        var validator = new InlineValidator<CoreSampleValidationModel>();
        validator.RuleFor(x => x.OptionalStatus).ValidEnum();
        validator.RuleFor(x => x.StatusCsv).ValidEnum<CoreSampleValidationModel, CoreSampleStatusEnum>();
        validator.RuleFor(x => x.OptionalCount).GreaterThanZero().GreaterOrEqualZero();
        validator.RuleFor(x => x.Count).GreaterThanZero().GreaterOrEqualZero();
        validator.RuleFor(x => x.StartDate).Before(x => x.EndDate);
        validator.RuleFor(x => x.StartAt).BeforeOrEqual(x => x.EndAt);
        var model = new CoreSampleValidationModel
        {
            OptionalStatus = null,
            StatusCsv = "Pending,2",
            OptionalCount = 1,
            Count = 1,
            StartDate = new DateOnly(2026, 5, 17),
            EndDate = new DateOnly(2026, 5, 18),
            StartAt = new DateTime(2026, 5, 17, 0, 0, 0, DateTimeKind.Utc),
            EndAt = new DateTime(2026, 5, 17, 0, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var result = validator.Validate(model);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidEnum_Should_FailValidation_When_CsvContainsUnknownEnumName()
    {
        // Arrange
        var validator = new InlineValidator<CoreSampleValidationModel>();
        validator.RuleFor(x => x.StatusCsv).ValidEnum<CoreSampleValidationModel, CoreSampleStatusEnum>();
        var model = new CoreSampleValidationModel
        {
            StatusCsv = "Pending,Missing"
        };

        // Act
        var result = validator.Validate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CoreSampleValidationModel.StatusCsv));
    }
}
