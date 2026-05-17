namespace Be.Haven.Tests.Shared.Be.Haven.ApiCommon.Swagger.Examples;

public sealed class SwaggerStandardExampleFactoryTests
{
    [Fact]
    public void BuildSuccessEnvelope_Should_ReturnSuccessResponseWithStableMeta_When_DataIsProvided()
    {
        // Arrange
        var data = new SwaggerSampleDataModel { Name = "Haven" };

        // Act
        var result = SwaggerStandardExampleFactory.BuildSuccessEnvelope(data);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Name.Should().Be("Haven");
        result.Meta.RequestId.Should().Be(SwaggerExampleConstants.EXAMPLE_REQUEST_ID);
        result.Meta.CorrelationId.Should().Be(SwaggerExampleConstants.EXAMPLE_CORRELATION_ID);
    }

    [Theory]
    [InlineData(StatusCodes.Status401Unauthorized, UNAUTHORIZED, INVALID_TOKEN)]
    [InlineData(StatusCodes.Status403Forbidden, FORBIDDEN, MISSING_PERMISSION)]
    [InlineData(StatusCodes.Status404NotFound, NOT_FOUND, MSG_NOT_FOUND)]
    [InlineData(StatusCodes.Status405MethodNotAllowed, METHOD_NOT_ALLOWED, MSG_METHOD_NOT_ALLOWED)]
    [InlineData(StatusCodes.Status406NotAcceptable, NOT_ACCEPTABLE, MSG_NOT_ACCEPTABLE)]
    [InlineData(StatusCodes.Status413PayloadTooLarge, BAD_REQUEST, MSG_PAYLOAD_TOO_LARGE)]
    [InlineData(StatusCodes.Status415UnsupportedMediaType, UNSUPPORTED_MEDIA_TYPE, MSG_UNSUPPORTED_MEDIA_TYPE)]
    [InlineData(StatusCodes.Status429TooManyRequests, MANY_REQUESTS, RedisConstants.ErrorMessage.ACCOUNT_LOCKED)]
    [InlineData(StatusCodes.Status503ServiceUnavailable, SERVICE_UNAVAILABLE, SwaggerExampleConstants.SERVICE_UNAVAILABLE_MESSAGE)]
    [InlineData(StatusCodes.Status500InternalServerError, INTERNAL_SERVER, UNEXPECTED_SERVER_ERROR)]
    public void Error_Should_ReturnStatusSpecificEnvelope_When_StatusCodeIsDocumented(
        int statusCode,
        string expectedCode,
        string expectedMessage)
    {
        // Arrange
        var expected = statusCode;

        // Act
        var result = SwaggerStandardExampleFactory.Error(statusCode);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.StatusCode.Should().Be(expected);
        result.Error.Code.Should().Be(expectedCode);
        result.Error.Message.Should().Be(expectedMessage);
        result.Meta.RequestId.Should().Be(SwaggerExampleConstants.EXAMPLE_REQUEST_ID);
    }

    [Fact]
    public void ValidationError_Should_ReturnGroupedValidationDetails_When_Called()
    {
        // Arrange
        var expectedFields = new[] { SwaggerExampleConstants.VALIDATION_FIELD_EMAIL, SwaggerExampleConstants.VALIDATION_FIELD_PASSWORD };

        // Act
        var result = SwaggerStandardExampleFactory.ValidationError();

        // Assert
        result.Success.Should().BeFalse();
        result.Error.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        result.Error.Details.Select(x => x.Field).Should().BeEquivalentTo(expectedFields);
    }

    private sealed class SwaggerSampleDataModel
    {
        public string Name { get; set; }
    }
}
