using System.Reflection;
using Microsoft.OpenApi;

namespace Be.Haven.Tests.Shared.Be.Haven.ApiCommon.Filters;

public sealed class SwaggerExampleOperationFilterTests
{
    [Fact]
    public void Apply_Should_SetRequestResponseAndFieldExamples_When_AttributesAreDeclared()
    {
        // Arrange
        var operation = CreateOperation();
        var method = GetExampleMethod(nameof(OperationWithExamples));
        var sut = new SwaggerExampleOperationFilter();

        // Act
        sut.Apply(operation, CreateOperationContext(method));

        // Assert
        operation.RequestBody.Content[TEXT_JSON].Example.ToJsonString().Should().Contain("request-name");
        operation.Responses["200"].Content[TEXT_JSON].Example.ToJsonString().Should().Contain("response-name");
        operation.Responses["400"].Content[TEXT_JSON].Example.ToJsonString().Should().Contain(VALIDATION_ERROR);
        operation.Parameters.Single(x => x.Name == "vehicle-public-id").Example.ToJsonString().Should().Contain("vehicle-123");
        ((OpenApiSchema)operation.RequestBody.Content[TEXT_JSON].Schema.Properties["fullName"]).Example.ToJsonString().Should().Contain("Nguyen Van A");
    }

    [Fact]
    public void Apply_Should_ThrowInvalidOperationException_When_ProviderDoesNotImplementContract()
    {
        // Arrange
        var operation = CreateOperation();
        var method = GetExampleMethod(nameof(OperationWithInvalidProvider));
        var sut = new SwaggerExampleOperationFilter();

        // Act
        var action = () => sut.Apply(operation, CreateOperationContext(method));

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*ISwaggerExampleProvider*");
    }

    [Fact]
    public void Apply_Should_NotThrow_When_OperationHasNoMatchingRequestOrResponseTargets()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            Responses = []
        };
        var method = GetExampleMethod(nameof(OperationWithValueExampleOnly));
        var sut = new SwaggerExampleOperationFilter();

        // Act
        var action = () => sut.Apply(operation, CreateOperationContext(method));

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Apply_Should_NotApplyObjectValueExamples_When_ProviderReturnsScalarWithoutFieldName()
    {
        // Arrange
        var operation = CreateOperation();
        var method = GetExampleMethod(nameof(OperationWithScalarObjectValueExample));
        var sut = new SwaggerExampleOperationFilter();

        // Act
        sut.Apply(operation, CreateOperationContext(method));

        // Assert
        operation.Parameters.Single(x => x.Name == "vehicle-public-id").Example.Should().BeNull();
        ((OpenApiSchema)operation.RequestBody.Content[TEXT_JSON].Schema.Properties["fullName"]).Example.Should().BeNull();
    }

    [Fact]
    public void Apply_Should_NotThrow_When_ResponseTargetHasNoContent()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses
            {
                [StatusCodes.Status200OK.ToString(CultureInfo.InvariantCulture)] = new OpenApiResponse()
            }
        };
        var method = GetExampleMethod(nameof(OperationWithResponseExampleOnly));
        var sut = new SwaggerExampleOperationFilter();

        // Act
        var action = () => sut.Apply(operation, CreateOperationContext(method));

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Apply_Should_ApplyFieldExample_When_FieldNameMatchesExactly()
    {
        // Arrange
        var operation = CreateOperation();
        var method = GetExampleMethod(nameof(OperationWithExactValueExample));
        var sut = new SwaggerExampleOperationFilter();

        // Act
        sut.Apply(operation, CreateOperationContext(method));

        // Assert
        operation.Parameters.Single(x => x.Name == "vehicle-public-id").Example.ToJsonString()
            .Should().Contain("vehicle-123");
    }

    [Fact]
    public void Apply_Should_NotThrow_When_ResponseExampleStatusIsNotDocumented()
    {
        // Arrange
        var operation = CreateOperation();
        var method = GetExampleMethod(nameof(OperationWithUndocumentedResponseExample));
        var sut = new SwaggerExampleOperationFilter();

        // Act
        var action = () => sut.Apply(operation, CreateOperationContext(method));

        // Assert
        action.Should().NotThrow();
        operation.Responses.Should().NotContainKey("201");
    }

    [Fact]
    public void Apply_Should_IgnoreNonNumericStandardErrorResponseKeys_When_ResponsesContainDefault()
    {
        // Arrange
        var operation = CreateOperation();
        var defaultResponse = new OpenApiResponse();
        defaultResponse.Content ??= new Dictionary<string, OpenApiMediaType>();
        defaultResponse.Content[TEXT_JSON] = new OpenApiMediaType();
        operation.Responses["default"] = defaultResponse;
        var method = GetExampleMethod(nameof(OperationWithoutExamples));
        var sut = new SwaggerExampleOperationFilter();

        // Act
        var action = () => sut.Apply(operation, CreateOperationContext(method));

        // Assert
        action.Should().NotThrow();
        operation.Responses["default"].Content[TEXT_JSON].Example.Should().BeNull();
    }

    [Fact]
    public void Apply_Should_NotThrow_When_RequestExampleHasNoRequestBody()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            Responses = []
        };
        var method = GetExampleMethod(nameof(OperationWithRequestExampleOnly));
        var sut = new SwaggerExampleOperationFilter();

        // Act
        var action = () => sut.Apply(operation, CreateOperationContext(method));

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Apply_Should_AddNamedResponseExamples_When_StatusHasMultipleValidShapes()
    {
        // Arrange
        var operation = CreateOperation();
        var method = GetExampleMethod(nameof(OperationWithNamedResponseExamples));
        var sut = new SwaggerExampleOperationFilter();

        // Act
        sut.Apply(operation, CreateOperationContext(method));

        // Assert
        var examples = operation.Responses["200"].Content[TEXT_JSON].Examples;
        examples.Should().ContainKeys("existingAccount", "registrationRequired");
        examples["existingAccount"].Value.ToJsonString().Should().Contain("response-name");
        examples["existingAccount"].Summary.Should().Be("Existing account");
        examples["registrationRequired"].Value.ToJsonString().Should().Contain("request-name");
        examples["registrationRequired"].Summary.Should().Be("Registration required");
    }

    [Fact]
    public void Apply_Should_NotApplyFieldExample_When_RequestSchemaHasNoProperties()
    {
        // Arrange
        var operation = CreateOperation();
        operation.RequestBody.Content[TEXT_JSON].Schema = new OpenApiSchema();
        var method = GetExampleMethod(nameof(OperationWithValueExampleOnly));
        var sut = new SwaggerExampleOperationFilter();

        // Act
        var action = () => sut.Apply(operation, CreateOperationContext(method));

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Apply_Should_ThrowInvalidOperationException_When_ProviderCannotBeCreated()
    {
        // Arrange
        var operation = CreateOperation();
        var method = GetExampleMethod(nameof(OperationWithProviderWithoutDefaultConstructor));
        var sut = new SwaggerExampleOperationFilter();

        // Act
        var action = () => sut.Apply(operation, CreateOperationContext(method));

        // Assert
        action.Should().Throw<MissingMethodException>();
    }

    [SwaggerRequestExample<RequestExampleProvider>]
    [SwaggerResponseExample<ResponseExampleProvider>(StatusCodes.Status200OK)]
    [SwaggerValueExample<VehiclePublicIdExampleProvider>("vehiclePublicId")]
    [SwaggerValueExample<FormFieldExamplesProvider>]
    private static void OperationWithExamples()
    {
    }

    [SwaggerRequestExample(typeof(object))]
    private static void OperationWithInvalidProvider()
    {
    }

    [SwaggerValueExample<VehiclePublicIdExampleProvider>("vehiclePublicId")]
    private static void OperationWithValueExampleOnly()
    {
    }

    [SwaggerValueExample<VehiclePublicIdExampleProvider>("vehicle-public-id")]
    private static void OperationWithExactValueExample()
    {
    }

    [SwaggerValueExample<VehiclePublicIdExampleProvider>]
    private static void OperationWithScalarObjectValueExample()
    {
    }

    [SwaggerResponseExample<ResponseExampleProvider>(StatusCodes.Status200OK)]
    private static void OperationWithResponseExampleOnly()
    {
    }

    [SwaggerResponseExample<ResponseExampleProvider>(StatusCodes.Status201Created)]
    private static void OperationWithUndocumentedResponseExample()
    {
    }

    [SwaggerResponseExample<ResponseExampleProvider>(
        StatusCodes.Status200OK,
        Name = "existingAccount",
        Summary = "Existing account")]
    [SwaggerResponseExample<RequestExampleProvider>(
        StatusCodes.Status200OK,
        Name = "registrationRequired",
        Summary = "Registration required")]
    private static void OperationWithNamedResponseExamples()
    {
    }

    [SwaggerRequestExample<RequestExampleProvider>]
    private static void OperationWithRequestExampleOnly()
    {
    }

    private static void OperationWithoutExamples()
    {
    }

    [SwaggerRequestExample<ProviderWithoutDefaultConstructor>]
    private static void OperationWithProviderWithoutDefaultConstructor()
    {
    }

    private static OpenApiOperation CreateOperation()
    {
        var schema = new OpenApiSchema();
        schema.Properties = new Dictionary<string, IOpenApiSchema>
        {
            ["fullName"] = new OpenApiSchema()
        };
        var requestBody = new OpenApiRequestBody();
        requestBody.Content ??= new Dictionary<string, OpenApiMediaType>();
        requestBody.Content[TEXT_JSON] = new OpenApiMediaType
        {
            Schema = schema
        };
        var okResponse = new OpenApiResponse();
        okResponse.Content ??= new Dictionary<string, OpenApiMediaType>();
        okResponse.Content[TEXT_JSON] = new OpenApiMediaType();
        var badRequestResponse = new OpenApiResponse();
        badRequestResponse.Content ??= new Dictionary<string, OpenApiMediaType>();
        badRequestResponse.Content[TEXT_JSON] = new OpenApiMediaType();

        return new OpenApiOperation
        {
            Parameters =
            [
                new OpenApiParameter { Name = "vehicle-public-id", In = ParameterLocation.Path }
            ],
            RequestBody = requestBody,
            Responses = new OpenApiResponses
            {
                [StatusCodes.Status200OK.ToString(CultureInfo.InvariantCulture)] = okResponse,
                [StatusCodes.Status400BadRequest.ToString(CultureInfo.InvariantCulture)] = badRequestResponse
            }
        };
    }

    private static OperationFilterContext CreateOperationContext(MethodInfo method)
    {
        return new OperationFilterContext(
            new ApiDescription(),
            Mock.Of<ISchemaGenerator>(),
            new SchemaRepository(),
            new OpenApiDocument(),
            method);
    }

    private static MethodInfo GetExampleMethod(string methodName)
    {
        return typeof(SwaggerExampleOperationFilterTests).GetMethod(
            methodName,
            BindingFlags.Static | BindingFlags.NonPublic);
    }

    private sealed class RequestExampleProvider : ISwaggerExampleProvider
    {
        public object GetExample() => new { Name = "request-name" };
    }

    private sealed class ResponseExampleProvider : ISwaggerExampleProvider
    {
        public object GetExample() => new { Name = "response-name" };
    }

    private sealed class VehiclePublicIdExampleProvider : ISwaggerExampleProvider
    {
        public object GetExample() => "vehicle-123";
    }

    private sealed class FormFieldExamplesProvider : ISwaggerExampleProvider
    {
        public object GetExample() => new { FullName = "Nguyen Van A" };
    }

    private sealed class ProviderWithoutDefaultConstructor : ISwaggerExampleProvider
    {
        public ProviderWithoutDefaultConstructor(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public object GetExample() => new { Value };
    }
}
