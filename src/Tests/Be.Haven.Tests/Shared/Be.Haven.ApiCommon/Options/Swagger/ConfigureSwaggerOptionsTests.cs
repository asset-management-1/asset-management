using Be.Haven.ApiCommon.Options.Swagger;

namespace Be.Haven.Tests.Shared.Be.Haven.ApiCommon.Swagger;

public sealed class ConfigureSwaggerOptionsTests
{
    [Fact]
    public void Configure_Should_RegisterSwaggerDocumentForEachApiVersion_When_VersionsAreProvided()
    {
        // Arrange
        var provider = new Mock<IApiVersionDescriptionProvider>();
        provider.SetupGet(x => x.ApiVersionDescriptions)
            .Returns([
                new ApiVersionDescription(new ApiVersion(1, 0), "v1", deprecated: false),
                new ApiVersionDescription(new ApiVersion(2, 0), "v2", deprecated: true)
            ]);
        var options = new SwaggerGenOptions();
        var sut = new ConfigureSwaggerOptions(provider.Object);

        // Act
        sut.Configure(options);

        // Assert
        options.SwaggerGeneratorOptions.SwaggerDocs.Should().ContainKeys("v1", "v2");
        options.SwaggerGeneratorOptions.SwaggerDocs["v1"].Version.Should().Be("1.0");
        options.SwaggerGeneratorOptions.SwaggerDocs["v2"].Version.Should().Be("2.0");
    }
}
