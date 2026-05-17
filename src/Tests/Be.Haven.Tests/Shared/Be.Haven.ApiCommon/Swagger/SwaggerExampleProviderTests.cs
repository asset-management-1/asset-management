namespace Be.Haven.Tests.Shared.Be.Haven.ApiCommon.Swagger;

public sealed class SwaggerExampleProviderTests
{
    [Fact]
    public void GetExample_Should_ReturnTypedExample_When_BaseProviderIsUsed()
    {
        // Arrange
        var sut = new SampleExampleProvider();

        // Act
        var result = sut.GetExample();

        // Assert
        result.Should().BeOfType<SampleExampleModel>()
            .Which.Name.Should().Be("typed-example");
    }

    [Fact]
    public void GetExample_Should_ReturnSuccessEnvelope_When_SuccessProviderIsUsed()
    {
        // Arrange
        var sut = new SampleSuccessExampleProvider();

        // Act
        var result = sut.GetExample();

        // Assert
        var response = result.Should().BeOfType<ResponseDto<SampleExampleModel>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Name.Should().Be("success-example");
        response.Meta.RequestId.Should().Be(SwaggerExampleConstants.EXAMPLE_REQUEST_ID);
    }

    private sealed class SampleExampleProvider : SwaggerExampleProvider<SampleExampleModel>
    {
        protected override SampleExampleModel BuildExample() =>
            new() { Name = "typed-example" };
    }

    private sealed class SampleSuccessExampleProvider : SwaggerSuccessExampleProvider<SampleExampleModel>
    {
        protected override SampleExampleModel BuildData() =>
            new() { Name = "success-example" };
    }

    private sealed class SampleExampleModel
    {
        public string Name { get; set; }
    }
}
