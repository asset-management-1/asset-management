namespace Be.Haven.Tests.Shared.Be.Haven.Core.Exceptions;

public sealed class ValidationExceptionTests
{
    [Fact]
    public void Constructor_Should_NormalizeFluentValidationFailures_When_FailuresAreProvided()
    {
        // Arrange
        var failures = new[]
        {
            new ValidationFailure("Email", "Email is invalid."),
            new ValidationFailure("Password", "Password is required.")
        };

        // Act
        var result = new ValidationException(failures, "error_validation");

        // Assert
        result.Message.Should().Be("Email is invalid. | Password is required.");
        result.ErrorCode.Should().Be("error_validation");
        result.Errors.Should().HaveCount(2);
        result.Errors[0].Field.Should().Be("Email");
        result.Errors[0].Issue.Should().BeEquivalentTo(new[] { "Email is invalid." });
    }

    [Fact]
    public void Constructor_Should_UseDefaultMessage_When_FluentValidationFailuresAreEmpty()
    {
        // Arrange
        var failures = Array.Empty<ValidationFailure>();

        // Act
        var result = new ValidationException(failures);

        // Assert
        result.Message.Should().Be("One or more validation failures have occurred.");
        result.ErrorCode.Should().Be("error_bad_request");
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_Should_NormalizeDataAnnotationFailures_When_MemberNamesAreProvided()
    {
        // Arrange
        var failures = new[]
        {
            new System.ComponentModel.DataAnnotations.ValidationResult(
                "Name is required.",
                new[] { "Name" })
        };

        // Act
        var result = new ValidationException(failures, string.Empty);

        // Assert
        result.Message.Should().Be("One or more validation failures have occurred.");
        result.ErrorCode.Should().Be("error_bad_request");
        result.Errors.Should().ContainSingle();
        result.Errors[0].Field.Should().Be("Name");
        result.Errors[0].Issue.Should().BeEquivalentTo(new[] { "Name is required." });
    }
}
