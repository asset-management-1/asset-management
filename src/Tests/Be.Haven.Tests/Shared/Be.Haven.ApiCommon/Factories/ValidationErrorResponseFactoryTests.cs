namespace Be.Haven.Tests.Shared.Be.Haven.ApiCommon.Factories;

public sealed class ValidationErrorResponseFactoryTests
{
    [Fact]
    public void Create_Should_ReturnStandardValidationEnvelope_When_ModelStateHasErrors()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.TraceIdentifier = "request-1";
        httpContext.Items[X_CORRELATION_ID] = "correlation-1";
        httpContext.Items[REQUEST_TIMESTAMP] = new DateTime(2026, 5, 17, 1, 2, 3, DateTimeKind.Utc);
        httpContext.RequestServices = new ServiceCollection().BuildServiceProvider();
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Email", "Email is required.");
        var context = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), modelState);

        // Act
        var result = ValidationErrorResponseFactory.Create(context);

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        objectResult.ContentTypes.Should().Contain(TEXT_JSON);
        var response = objectResult.Value.Should().BeOfType<ResponseDto<string>>().Subject;
        response.Success.Should().BeFalse();
        response.Meta.RequestId.Should().Be("request-1");
        response.Meta.CorrelationId.Should().Be("correlation-1");
        response.Error.Code.Should().Be(BAD_REQUEST);
        response.Error.Details.Should().ContainSingle(x =>
            x.Field == "Email" && ((string[])x.Issue).Contains("Email is required."));
    }

    [Fact]
    public void Create_Should_IncludeApiVersion_When_RequestFeatureHasVersion()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.TraceIdentifier = "request-1";
        httpContext.Items[X_CORRELATION_ID] = "correlation-1";
        httpContext.Items[REQUEST_TIMESTAMP] = new DateTime(2026, 5, 17, 1, 2, 3, DateTimeKind.Utc);
        httpContext.RequestServices = new ServiceCollection().BuildServiceProvider();
        httpContext.Features.Set(CreateVersioningFeature(new ApiVersion(2, 1)));
        var modelState = new ModelStateDictionary();
        var metadata = new EmptyModelMetadataProvider()
            .GetMetadataForType(typeof(string));
        modelState.AddModelError("Email", new InvalidOperationException("Email exception."), metadata);
        var context = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), modelState);

        // Act
        var result = ValidationErrorResponseFactory.Create(context);

        // Assert
        var response = ((ObjectResult)result).Value.Should().BeOfType<ResponseDto<string>>().Subject;
        response.Meta.Version.Should().Be("2.1");
        response.Error.Details.Should().ContainSingle(x =>
            x.Field == "Email" && ((string[])x.Issue).Contains("Email exception."));
    }

    private static IApiVersioningFeature CreateVersioningFeature(ApiVersion version)
    {
        var feature = new Mock<IApiVersioningFeature>();
        feature.SetupGet(x => x.RequestedApiVersion).Returns(version);

        return feature.Object;
    }
}
