namespace Be.Haven.Tests.Shared.Be.Haven.Core.Helpers;

public sealed class CodeGenerationHelperTests
{
    [Fact]
    public void GenerateCode_Should_PreservePrefixCase_When_DefaultOptionsAreUsed()
    {
        // Act
        var result = CodeGenerationHelper.GenerateCode("prop", 10);

        // Assert
        result.Should().MatchRegex("^prop_[A-F0-9]{10}$");
    }

    [Fact]
    public void GenerateCode_Should_UseCustomSeparator_When_SeparatorIsProvided()
    {
        // Act
        var result = CodeGenerationHelper.GenerateCode(
            "unit",
            4,
            separator: ":");

        // Assert
        result.Should().MatchRegex("^unit:[A-F0-9]{4}$");
    }

    [Fact]
    public async Task GenerateUniqueCodeAsync_Should_RetryAndReturnFirstAvailableCode_When_CollisionOccurs()
    {
        // Arrange
        var checkedCodes = new List<string>();
        var attempts = 0;

        // Act
        var result = await CodeGenerationHelper.GenerateUniqueCodeAsync(
            new UniqueCodeGenerationOptions
            {
                Prefix = "P",
                RandomLength = 4,
                MaxAttempts = 3,
                ExistsAsync = (code, _) =>
                {
                    checkedCodes.Add(code);
                    attempts++;
                    return Task.FromResult(attempts == 1);
                }
            });

        // Assert
        result.Should().MatchRegex("^P_[A-F0-9]{4}$");
        checkedCodes.Should().HaveCount(2);
        checkedCodes[1].Should().Be(result);
    }

    [Fact]
    public async Task GenerateUniqueCodeAsync_Should_IncreaseRandomLength_When_CurrentLengthKeepsColliding()
    {
        // Arrange
        var checkedCodes = new List<string>();

        // Act
        var result = await CodeGenerationHelper.GenerateUniqueCodeAsync(
            new UniqueCodeGenerationOptions
            {
                Prefix = "P",
                RandomLength = 2,
                MaxAttempts = 2,
                ExistsAsync = (code, _) =>
                {
                    checkedCodes.Add(code);
                    return Task.FromResult(checkedCodes.Count <= 2);
                }
            });

        // Assert
        checkedCodes.Should().HaveCount(3);
        checkedCodes.Take(2).Should().OnlyContain(code => code.Split('_')[1].Length == 2);
        result.Should().MatchRegex("^P_[A-F0-9]{3}$");
    }

    [Fact]
    public async Task GenerateUniqueCodeAsync_Should_ThrowCustomException_When_AllLengthsCollide()
    {
        // Arrange
        Func<Task> act = () => CodeGenerationHelper.GenerateUniqueCodeAsync(
            new UniqueCodeGenerationOptions
            {
                Prefix = "P",
                RandomLength = 32,
                MaxAttempts = 2,
                ExistsAsync = (_, _) => Task.FromResult(true),
                ExhaustionExceptionFactory = () => new InvalidOperationException("Custom exhaustion")
            });

        // Act and Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Custom exhaustion");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GenerateCode_Should_ThrowArgumentException_When_PrefixIsMissing(string prefix)
    {
        // Act
        var act = () => CodeGenerationHelper.GenerateCode(prefix, 4);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GenerateCode_Should_ThrowArgumentOutOfRangeException_When_RandomLengthIsInvalid()
    {
        // Act
        var act = () => CodeGenerationHelper.GenerateCode("P", 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GenerateCode_Should_ThrowArgumentOutOfRangeException_When_RandomLengthExceedsGuidHexLength()
    {
        // Act
        var act = () => CodeGenerationHelper.GenerateCode("P", 33);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task GenerateUniqueCodeAsync_Should_ThrowArgumentOutOfRangeException_When_MaxAttemptsIsInvalid()
    {
        // Act
        Func<Task> act = () => CodeGenerationHelper.GenerateUniqueCodeAsync(
            new UniqueCodeGenerationOptions
            {
                Prefix = "P",
                RandomLength = 4,
                MaxAttempts = 0,
                ExistsAsync = (_, _) => Task.FromResult(false)
            });

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task GenerateUniqueCodeAsync_Should_ThrowArgumentNullException_When_OptionsAreMissing()
    {
        // Act
        Func<Task> act = () => CodeGenerationHelper.GenerateUniqueCodeAsync(null);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GenerateUniqueCodeAsync_Should_ThrowArgumentNullException_When_ExistsCallbackIsMissing()
    {
        // Act
        Func<Task> act = () => CodeGenerationHelper.GenerateUniqueCodeAsync(
            new UniqueCodeGenerationOptions
            {
                Prefix = "P",
                RandomLength = 4,
                MaxAttempts = 1,
                ExistsAsync = null
            });

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
