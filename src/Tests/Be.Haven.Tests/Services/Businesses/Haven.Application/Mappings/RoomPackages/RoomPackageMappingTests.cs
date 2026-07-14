using Haven.Application.Mappings.RoomPackages;
using Haven.Application.Dtos.RoomPackages.Common;
using Haven.Application.Models.RoomPackages.Create;
using Haven.Application.Models.RoomPackages.Update;
using Haven.Domain.Entities;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Mappings.RoomPackages;

public sealed class RoomPackageMappingTests
{
    [Fact]
    public void Adapt_Should_MapOnlySubmittedDisplayFields_WhenCreatingPackage()
    {
        // Arrange
        var config = new TypeAdapterConfig();
        new RoomPackageMapping().Register(config);
        var request = new RoomPackageCreateRequestModel
        {
            Name = "Gói Cao cấp",
            PriceAdjustment = 1_800_000,
            RoomPublicId = Guid.NewGuid(),
            Items = [new RoomPackageItemRequestDto { Name = "Máy lạnh" }]
        };

        // Act
        var package = request.Adapt<UnitPackage>(config);

        // Assert
        package.PackageName.Should().Be("Gói Cao cấp");
        package.PriceAdjustment.Should().Be(1_800_000);
        package.PublicId.Should().BeEmpty();
        package.PackageCode.Should().BeNull();
        package.PackageTypeId.Should().Be(0);
        package.StatusId.Should().Be(0);
        package.Items.Should().BeEmpty();
    }

    [Fact]
    public void Adapt_Should_KeepPersistedFields_WhenPartialValuesAreOmitted()
    {
        // Arrange
        var config = new TypeAdapterConfig();
        new RoomPackageMapping().Register(config);
        var package = new UnitPackage
        {
            PackageName = "Gói Cũ",
            PriceAdjustment = 800_000,
            PackageCode = "PKG_123456"
        };
        var request = new RoomPackageUpdateRequestModel();

        // Act
        request.Adapt(package, config);

        // Assert
        package.PackageName.Should().Be("Gói Cũ");
        package.PriceAdjustment.Should().Be(800_000);
        package.PackageCode.Should().Be("PKG_123456");
    }

    [Fact]
    public void Adapt_Should_UpdateOnlySubmittedFlatFields()
    {
        // Arrange
        var config = new TypeAdapterConfig();
        new RoomPackageMapping().Register(config);
        var package = new UnitPackage
        {
            PackageName = "Gói Cũ",
            PriceAdjustment = 800_000,
            PackageCode = "PKG_123456"
        };
        var request = new RoomPackageUpdateRequestModel
        {
            Name = "Gói Mới",
            PriceAdjustment = 1_200_000
        };

        // Act
        request.Adapt(package, config);

        // Assert
        package.PackageName.Should().Be("Gói Mới");
        package.PriceAdjustment.Should().Be(1_200_000);
        package.PackageCode.Should().Be("PKG_123456");
    }
}
