using Haven.Api.Controllers.v1;
using Haven.Application.Dtos.Meters.Detail;
using Haven.Application.Dtos.Properties.Detail;
using Haven.Application.Dtos.RoomPackages.Detail;
using Haven.Application.Dtos.Rooms.Detail;
using Haven.Application.Dtos.Tenants.Detail;
using Haven.Application.Dtos.Vehicles.Detail;

namespace Be.Haven.Tests.Services.Businesses.Haven.Api.Swagger;

public sealed class HavenSwaggerExamplesTests
{
    [Theory]
    [InlineData("PropertySwaggerExamples+DetailResponse", typeof(ResponseDto<PropertyDetailResponseDto>))]
    [InlineData("RoomSwaggerExamples+DetailResponse", typeof(ResponseDto<RoomDetailResponseDto>))]
    [InlineData("RoomPackageSwaggerExamples+DetailResponse", typeof(ResponseDto<RoomPackageDetailResponseDto>))]
    [InlineData("VehicleSwaggerExamples+DetailResponse", typeof(ResponseDto<VehicleDetailResponseDto>))]
    [InlineData("TenantSwaggerExamples+DetailResponse", typeof(ResponseDto<TenantDetailResponseDto>))]
    [InlineData("MeterSwaggerExamples+PeriodResponse", typeof(ResponseDto<MeterPeriodDetailResponseDto>))]
    public void SuccessProvider_Should_ReturnProductionEnvelope_When_FeatureExampleIsBuilt(
        string nestedProviderName,
        Type expectedEnvelopeType)
    {
        // Arrange
        var providerType = typeof(PropertiesController).Assembly.GetType(
            $"Haven.Api.Swagger.Examples.{nestedProviderName}",
            throwOnError: true)!;
        var provider = (ISwaggerExampleProvider)Activator.CreateInstance(providerType, nonPublic: true)!;

        // Act
        var example = provider.GetExample();

        // Assert
        example.Should().BeOfType(expectedEnvelopeType);
    }
}
