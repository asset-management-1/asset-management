using Microsoft.OpenApi;

namespace Be.Haven.Tests.Shared.Be.Haven.ApiCommon.Filters;

public sealed class KebabCaseSwaggerFilterTests
{
    [Fact]
    public void Apply_Should_RemoveCamelCasePathParameter_When_KebabCaseParameterExists()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            Parameters =
            [
                new OpenApiParameter { Name = "vehiclePublicId", In = ParameterLocation.Path },
                new OpenApiParameter { Name = "vehicle-public-id", In = ParameterLocation.Path },
                new OpenApiParameter { Name = "searchText", In = ParameterLocation.Query }
            ]
        };
        var sut = new KebabCaseSwaggerFilter();

        // Act
        sut.Apply(operation, CreateOperationContext());

        // Assert
        operation.Parameters.Select(x => x.Name).Should().BeEquivalentTo(
        [
            "vehicle-public-id",
            "searchText"
        ]);
    }

    [Fact]
    public void Apply_Should_KeepCamelCasePathParameter_When_KebabCaseParameterDoesNotExist()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            Parameters =
            [
                new OpenApiParameter { Name = "vehiclePublicId", In = ParameterLocation.Path }
            ]
        };
        var sut = new KebabCaseSwaggerFilter();

        // Act
        sut.Apply(operation, CreateOperationContext());

        // Assert
        operation.Parameters.Should().ContainSingle(x => x.Name == "vehiclePublicId");
    }

    [Fact]
    public void Apply_Should_ReturnWithoutChanges_When_OperationHasNoParameters()
    {
        // Arrange
        var operation = new OpenApiOperation();
        var sut = new KebabCaseSwaggerFilter();

        // Act
        sut.Apply(operation, CreateOperationContext());

        // Assert
        operation.Parameters.Should().BeNull();
    }

    [Fact]
    public void Apply_Should_KeepUnnamedPathParameter_When_NameIsBlank()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            Parameters =
            [
                new OpenApiParameter { Name = " ", In = ParameterLocation.Path },
                new OpenApiParameter { Name = "vehicle-public-id", In = ParameterLocation.Path }
            ]
        };
        var sut = new KebabCaseSwaggerFilter();

        // Act
        sut.Apply(operation, CreateOperationContext());

        // Assert
        operation.Parameters.Should().Contain(parameter => parameter.Name == " ");
    }

    private static OperationFilterContext CreateOperationContext()
    {
        var method = typeof(KebabCaseSwaggerFilterTests).GetMethod(nameof(Apply_Should_KeepCamelCasePathParameter_When_KebabCaseParameterDoesNotExist));

        return new OperationFilterContext(
            new ApiDescription(),
            Mock.Of<ISchemaGenerator>(),
            new SchemaRepository(),
            new OpenApiDocument(),
            method);
    }
}
