namespace Be.Haven.Tests.Shared.Be.Haven.Shared.Dtos;

public sealed class ResponseDtoTests
{
    [Fact]
    public void Constructor_Should_SetSuccessTrue_When_DataIsProvided()
    {
        // Act
        var result = new ResponseDto<string>("ok");

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().Be("ok");
        result.Error.Should().BeNull();
    }

    [Fact]
    public void DataSetter_Should_SetSuccessTrue_When_ErrorIsCleared()
    {
        // Arrange
        var result = new ResponseDto<string>("error_code", "failed");
        result.Error = null;

        // Act
        result.Data = "ok";

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().Be("ok");
    }

    [Fact]
    public void Constructor_Should_SetBadRequestError_When_StatusCodeIsNotProvided()
    {
        // Arrange
        var details = new List<ErrorDetailDto>
        {
            new()
            {
                Field = "email",
                Issue = "invalid"
            }
        };
        var meta = new MetaDetailDto
        {
            RequestId = "request-1"
        };

        // Act
        var result = new ResponseDto<string>("error_code", "failed", details, meta);

        // Assert
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Error.Code.Should().Be("error_code");
        result.Error.Message.Should().Be("failed");
        result.Error.StatusCode.Should().Be(AppConstants.SystemCode.BAD_REQUEST);
        result.Error.Details.Should().BeSameAs(details);
        result.Meta.Should().BeSameAs(meta);
    }

    [Fact]
    public void Constructor_Should_SetProvidedStatusCodeAndDefaultMeta_When_ErrorIsProvided()
    {
        // Act
        var result = new ResponseDto<string>("error_conflict", "conflict", StatusCodes.Status409Conflict);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        result.Meta.Should().NotBeNull();
    }
}
