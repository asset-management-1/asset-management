namespace Be.Haven.Tests.Shared.Be.Haven.Core.Extensions.Objects;

public sealed class StringExtensionTests
{
    [Theory]
    [MemberData(nameof(OptionalTextValues))]
    public void NormalizeOptional_Should_ReturnTrimmedValueOrNull_When_TextIsOptional(
        string value,
        string expected)
    {
        // Act
        var result = value.NormalizeOptional();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void NormalizeEmail_Should_TrimAndLowercase_When_EmailHasWhitespaceAndMixedCase()
    {
        // Arrange
        var email = "  LockH@Example.COM ";

        // Act
        var result = email.NormalizeEmail();

        // Assert
        result.Should().Be("lockh@example.com");
    }

    [Fact]
    public void NormalizeAlphanumericCode_Should_KeepOnlyLettersAndDigits_When_ValueHasSeparators()
    {
        // Arrange
        var value = " ab-12_cd ";

        // Act
        var result = value.NormalizeAlphanumericCode();

        // Assert
        result.Should().Be("AB12CD");
    }

    [Fact]
    public void RemoveEndWith_Should_RemoveRepeatedTrailingCharacters_When_SourceEndsWithValue()
    {
        // Arrange
        var source = "avatars///";

        // Act
        var result = source.RemoveEndWith("/");

        // Assert
        result.Should().Be("avatars");
    }

    [Fact]
    public void RemoveStartWith_Should_RemoveRepeatedLeadingCharacters_When_SourceStartsWithValue()
    {
        // Arrange
        var source = "///avatars";

        // Act
        var result = source.RemoveStartWith("/");

        // Assert
        result.Should().Be("avatars");
    }

    [Theory]
    [InlineData("Haven", "haven")]
    [InlineData("h", "h")]
    [InlineData("", "")]
    public void FirstCharToLowerCase_Should_LowercaseOnlyFirstCharacter_When_FirstCharacterIsUppercase(
        string source,
        string expected)
    {
        // Act
        var result = source.FirstCharToLowerCase();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("haven", "Haven")]
    [InlineData("H", "H")]
    [InlineData("", "")]
    public void FirstCharToUpperCase_Should_UppercaseOnlyFirstCharacter_When_FirstCharacterIsLowercase(
        string source,
        string expected)
    {
        // Act
        var result = source.FirstCharToUpperCase();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ToNonUnicode_Should_RemoveVietnameseAccents_When_TextContainsUnicodeCharacters()
    {
        // Arrange
        var source = "\u0110\u1eb7ng V\u0103n B";

        // Act
        var result = source.ToNonUnicode();

        // Assert
        result.Should().Be("Dang Van B");
    }

    [Fact]
    public void RemoveCharacters_Should_RemoveEveryConfiguredCharacter_When_SourceContainsThem()
    {
        // Arrange
        var source = "59-S2 123.45";

        // Act
        var result = source.RemoveCharacters('-', ' ', '.');

        // Assert
        result.Should().Be("59S212345");
    }

    [Theory]
    [MemberData(nameof(RemoveCharactersNoOpValues))]
    public void RemoveCharacters_Should_ReturnSource_When_SourceOrCharactersAreMissing(
        string source,
        char[] charsToRemove,
        string expected)
    {
        // Act
        var result = source.RemoveCharacters(charsToRemove);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetSecretValue_Should_ReturnSegmentAtIndex_When_UrlContainsPathSegments()
    {
        // Arrange
        var source = "https://storage.googleapis.com/bucket/folder/file.jpg";

        // Act
        var result = source.GetSecretValue(2);

        // Assert
        result.Should().Be("folder");
    }

    [Fact]
    public void GetSecretValue_Should_ReturnSource_When_SourceIsEmpty()
    {
        // Act
        var result = string.Empty.GetSecretValue(0);

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("abc123", "^[a-z]+[0-9]+$", true)]
    [InlineData("abc123", "[", false)]
    [InlineData("", "^[a-z]+$", false)]
    public void IsMatchPattern_Should_ReturnMatchResultOrFalse_When_InputOrPatternIsInvalid(
        string source,
        string pattern,
        bool expected)
    {
        // Act
        var result = source.IsMatchPattern(pattern);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GeneratePrefixExtension_Should_JoinServiceNameAndCode_WithUnderscore()
    {
        // Act
        var result = StringExtension.GeneratePrefixExtension("auth", "login");

        // Assert
        result.Should().Be("auth_login");
    }

    public static IEnumerable<object[]> OptionalTextValues()
    {
        yield return [null, null];
        yield return ["", null];
        yield return ["   ", null];
        yield return ["  Haven  ", "Haven"];
    }

    public static IEnumerable<object[]> RemoveCharactersNoOpValues()
    {
        yield return [null, new[] { '-' }, null];
        yield return ["", new[] { '-' }, ""];
        yield return ["abc", null, "abc"];
        yield return ["abc", Array.Empty<char>(), "abc"];
    }
}
