namespace Be.Haven.Tests.Shared.Be.Haven.Core.Exceptions;

public sealed class ApiExceptionTests
{
    [Fact]
    public void Constructor_Should_DefaultToBadRequest_When_CreatedWithoutMessage()
    {
        // Act
        var result = new ApiException();

        // Assert
        result.Message.Should().Be("Exception of type 'Be.Haven.Core.Exceptions.ApiException' was thrown.");
        result.ErrorCode.Should().BeNull();
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public void Constructor_Should_DefaultToBadRequest_When_OnlyMessageIsProvided()
    {
        // Arrange
        var message = "Invalid request";

        // Act
        var result = new ApiException(message);

        // Assert
        result.Message.Should().Be(message);
        result.ErrorCode.Should().BeNull();
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public void Constructor_Should_DefaultToBadRequest_When_StatusIsNotProvided()
    {
        // Arrange
        var message = "Invalid profile";

        // Act
        var result = new ApiException(message, "error_profile_invalid");

        // Assert
        result.Message.Should().Be(message);
        result.ErrorCode.Should().Be("error_profile_invalid");
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public void Constructor_Should_KeepExplicitStatusCode_When_StatusIsProvided()
    {
        // Arrange
        var statusCode = StatusCodes.Status403Forbidden;

        // Act
        var result = new ApiException("Forbidden", "error_forbidden", statusCode);

        // Assert
        result.StatusCode.Should().Be(statusCode);
        result.ErrorCode.Should().Be("error_forbidden");
    }

    [Fact]
    public void Constructor_Should_KeepPublicSafeDetails_When_DetailsAreProvided()
    {
        // Arrange
        var details = new List<ErrorDetailDto>
        {
            new()
            {
                Field = "action",
                Issue = "selection_required"
            }
        };

        // Act
        var result = new ApiException(
            "Selection is required",
            "error_selection_required",
            StatusCodes.Status400BadRequest,
            details);

        // Assert
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        result.ErrorCode.Should().Be("error_selection_required");
        result.Details.Should().BeSameAs(details);
    }

    [Fact]
    public void Constructor_Should_FormatMessage_When_ArgumentsAreProvided()
    {
        // Arrange
        var format = "User {0} is invalid";

        // Act
        var result = new ApiException(format, (object)"lockh");

        // Assert
        result.Message.Should().Be("User lockh is invalid");
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }
}
