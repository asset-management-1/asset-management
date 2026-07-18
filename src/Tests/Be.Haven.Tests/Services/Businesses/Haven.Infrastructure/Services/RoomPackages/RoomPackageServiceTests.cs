using Haven.Application.Interfaces.Repositories;
using Haven.Application.Interfaces.Services;
using Haven.Application.Models.MasterData;
using Haven.Application.Models.Parties;
using Haven.Application.Models.RoomPackages.List;
using Haven.Application.Models.RoomPackages.Detail;
using Haven.Application.Models.RoomPackages.QueryParameters;
using Haven.Application.Models.RoomPackages.Update;
using Haven.Application.Models.RoomPackages.Delete;
using Haven.Application.Models.Rooms.QueryParameters;
using Haven.Application.Models.Rooms.Rows;
using Haven.Domain.Entities;
using Haven.Infrastructure.Services.RoomPackages;
using Be.Haven.Core.Interfaces.Repositories;

namespace Be.Haven.Tests.Services.Businesses.Haven.Infrastructure.Services.RoomPackages;

public sealed class RoomPackageServiceTests
{
    [Fact]
    public async Task GetListAsync_Should_ReturnCommonPackages_WhenRoomHasNoOverride()
    {
        // Arrange
        var roomPublicId = Guid.NewGuid();
        var packagePublicId = Guid.NewGuid();
        var unitRepository = new Mock<IUnitRepository>();
        unitRepository
            .Setup(repository => repository.GetRoomDetailAsync(
                It.Is<RoomScopedQueryParametersModel>(value => value.RoomPublicId == roomPublicId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RoomDetailRowModel
            {
                UnitPublicId = roomPublicId,
                UnitName = "Phòng 101",
                UsesCommonPackages = true
            });
        unitRepository
            .Setup(repository => repository.GetEffectivePackageTemplatesAsync(
                It.Is<RoomPackageQueryParametersModel>(value =>
                    value.RoomPublicId == roomPublicId
                    && value.NoFurniturePackageTypeCode == MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new RoomPackageTemplateRowModel
                {
                    PackagePublicId = packagePublicId,
                    PackageName = "Gói Full Nội thất",
                    PriceAdjustment = 1_500_000,
                    ItemName = "Giường ngủ",
                    DisplayOrder = 0
                },
                new RoomPackageTemplateRowModel
                {
                    PackagePublicId = packagePublicId,
                    PackageName = "Gói Full Nội thất",
                    PriceAdjustment = 1_500_000,
                    ItemName = "Máy lạnh",
                    DisplayOrder = 1
                }
            ]);
        var sut = CreateSut(unitRepository.Object);

        // Act
        var result = await sut.GetListAsync(new RoomPackageListRequestModel
        {
            RoomPublicId = roomPublicId,
            CurrentParty = new CurrentPartyContextModel { PartyId = 10, PartyPublicId = Guid.NewGuid() }
        });

        // Assert
        result.Room.Id.Should().Be(roomPublicId);
        result.UsesCommonPackages.Should().BeTrue();
        result.Packages.Should().ContainSingle();
        result.Packages.Single().Items.Select(item => item.Name)
            .Should().ContainInOrder("Giường ngủ", "Máy lạnh");
    }

    [Fact]
    public async Task GetDetailAsync_Should_ThrowNotFound_WhenPackageIsOutsideEffectiveSet()
    {
        // Arrange
        var roomPublicId = Guid.NewGuid();
        var unitRepository = new Mock<IUnitRepository>();
        unitRepository
            .Setup(repository => repository.GetRoomDetailAsync(
                It.IsAny<RoomScopedQueryParametersModel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RoomDetailRowModel
            {
                UnitPublicId = roomPublicId,
                UnitName = "Phòng 101",
                UsesCommonPackages = false
            });
        unitRepository
            .Setup(repository => repository.GetEffectivePackageTemplatesAsync(
                It.IsAny<RoomPackageQueryParametersModel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var sut = CreateSut(unitRepository.Object);

        // Act
        var action = () => sut.GetDetailAsync(new RoomPackageDetailRequestModel
        {
            RoomPublicId = roomPublicId,
            PackagePublicId = Guid.NewGuid(),
            CurrentParty = new CurrentPartyContextModel { PartyId = 10 }
        });

        // Assert
        await action.Should().ThrowAsync<ApiException>()
            .Where(exception => exception.StatusCode == StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task UpdateAsync_Should_RejectNoFurnitureFallback_BeforeMaterializingRoomPackages()
    {
        // Arrange
        var roomPublicId = Guid.NewGuid();
        var fallbackPackage = new UnitPackage
        {
            Id = 11,
            PublicId = Guid.NewGuid(),
            PackageTypeId = 101,
            PackageCode = MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE
        };
        var property = new Property();
        property.UnitPackages.Add(fallbackPackage);

        var room = new Unit
        {
            Id = 10,
            PublicId = roomPublicId,
            Property = property
        };
        var unitRepository = new Mock<IUnitRepository>();
        unitRepository
            .Setup(repository => repository.GetRoomPackageGraphForMutationAsync(
                It.IsAny<RoomScopedQueryParametersModel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);
        var packageRepository = new Mock<IUnitPackageRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork
            .Setup(value => value.ExecuteInTransactionAsync(
                It.IsAny<Func<CancellationToken, Task<Guid>>>(),
                It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task<Guid>> action, CancellationToken token) => action(token));
        var masterDataService = new Mock<IMasterDataService>();
        masterDataService
            .Setup(service => service.GetValuesAsync(
                It.IsAny<IReadOnlyCollection<MasterDataKeyModel>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<MasterDataKeyModel, MasterDataValueModel>
            {
                [new(MasterDataTypeEnum.UnitPackageType, MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE)] = new()
                {
                    Id = fallbackPackage.PackageTypeId
                },
                [new(MasterDataTypeEnum.UnitPackageStatus, MASTER_CODE_UNIT_PACKAGE_STATUS_ACTIVE)] = new()
                {
                    Id = 201
                }
            });
        var sut = new RoomPackageService(
            unitRepository.Object,
            packageRepository.Object,
            Mock.Of<IUnitPackageItemRepository>(),
            masterDataService.Object,
            unitOfWork.Object,
            Mock.Of<ILogger<RoomPackageService>>());

        var request = new RoomPackageUpdateRequestModel
        {
            RoomPublicId = roomPublicId,
            PackagePublicId = fallbackPackage.PublicId,
            CurrentParty = new CurrentPartyContextModel { PartyId = 10 }
        };

        // Act
        var action = () => sut.UpdateAsync(request);

        // Assert
        await action.Should().ThrowAsync<ApiException>()
            .Where(exception => exception.StatusCode == StatusCodes.Status404NotFound);
        packageRepository.Verify(
            repository => repository.AddAsync(It.IsAny<UnitPackage>(), It.IsAny<CancellationToken>()),
            Times.Never);
        packageRepository.Verify(
            repository => repository.HasContractReferenceAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_Should_UseExistingRoomPackage_WithoutMaterializingCommonPackages()
    {
        // Arrange
        var fallbackTypeId = 101L;
        var roomPackage = new UnitPackage
        {
            Id = 12,
            PublicId = Guid.NewGuid(),
            PackageTypeId = 102,
            PackageCode = "PKG_123456"
        };
        var roomFallback = new UnitPackage
        {
            Id = 13,
            PublicId = Guid.NewGuid(),
            PackageTypeId = fallbackTypeId,
            PackageCode = MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE
        };
        var room = new Unit
        {
            Id = 10,
            PublicId = Guid.NewGuid(),
            Property = new Property()
        };
        room.UnitPackages.Add(roomPackage);
        room.UnitPackages.Add(roomFallback);

        var unitRepository = new Mock<IUnitRepository>();
        unitRepository
            .Setup(repository => repository.GetRoomPackageGraphForMutationAsync(
                It.IsAny<RoomScopedQueryParametersModel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);
        var packageRepository = new Mock<IUnitPackageRepository>();
        packageRepository
            .Setup(repository => repository.HasContractReferenceAsync(
                roomPackage.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var masterDataService = new Mock<IMasterDataService>();
        masterDataService
            .Setup(service => service.GetValuesAsync(
                It.IsAny<IReadOnlyCollection<MasterDataKeyModel>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<MasterDataKeyModel, MasterDataValueModel>
            {
                [new(MasterDataTypeEnum.UnitPackageType, MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE)] = new()
                {
                    Id = fallbackTypeId
                },
                [new(MasterDataTypeEnum.UnitPackageStatus, MASTER_CODE_UNIT_PACKAGE_STATUS_ACTIVE)] = new()
                {
                    Id = 201
                }
            });
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork
            .Setup(value => value.ExecuteInTransactionAsync(
                It.IsAny<Func<CancellationToken, Task>>(),
                It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task> action, CancellationToken token) => action(token));
        var sut = new RoomPackageService(
            unitRepository.Object,
            packageRepository.Object,
            Mock.Of<IUnitPackageItemRepository>(),
            masterDataService.Object,
            unitOfWork.Object,
            Mock.Of<ILogger<RoomPackageService>>());

        // Act
        await sut.DeleteAsync(new RoomPackageDeleteRequestModel
        {
            RoomPublicId = room.PublicId,
            PackagePublicId = roomPackage.PublicId,
            CurrentParty = new CurrentPartyContextModel { PartyId = 10 }
        });

        // Assert
        packageRepository.Verify(repository => repository.HasContractReferenceAsync(
            roomPackage.Id,
            It.IsAny<CancellationToken>()), Times.Once);
        packageRepository.Verify(repository => repository.DeletePackagesWithItems(
            It.Is<IReadOnlyCollection<UnitPackage>>(packages => packages.Single() == roomPackage)), Times.Once);
        packageRepository.Verify(repository => repository.AddAsync(
            It.IsAny<UnitPackage>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_Should_MaterializeCommonPackageBeforeCheckingRoomSpecificContractReference()
    {
        // Arrange
        var roomPublicId = Guid.NewGuid();
        var fallbackTypeId = 101L;
        var commonPackage = new UnitPackage
        {
            Id = 12,
            PublicId = Guid.NewGuid(),
            PackageTypeId = 102,
            PackageCode = "PKG_123456",
            PackageName = "Gói Full Nội thất",
            StatusId = 201
        };
        var fallbackPackage = new UnitPackage
        {
            Id = 11,
            PublicId = Guid.NewGuid(),
            PackageTypeId = fallbackTypeId,
            PackageCode = MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE,
            StatusId = 201
        };
        var property = new Property();
        property.UnitPackages.Add(fallbackPackage);
        property.UnitPackages.Add(commonPackage);
        var roomFallbackPackage = new UnitPackage
        {
            Id = 13,
            PublicId = Guid.NewGuid(),
            PackageTypeId = fallbackTypeId,
            PackageCode = MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE,
            StatusId = 201
        };

        var room = new Unit
        {
            Id = 10,
            PublicId = roomPublicId,
            Property = property
        };
        room.UnitPackages.Add(roomFallbackPackage);
        var clonedPackageId = 100L;
        var unitRepository = new Mock<IUnitRepository>();
        unitRepository
            .Setup(repository => repository.GetRoomPackageGraphForMutationAsync(
                It.IsAny<RoomScopedQueryParametersModel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);
        var packageRepository = new Mock<IUnitPackageRepository>();
        packageRepository
            .Setup(repository => repository.ExistsPackageCodeAsync(
                room.Id,
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        packageRepository
            .Setup(repository => repository.AddAsync(
                It.IsAny<UnitPackage>(),
                It.IsAny<CancellationToken>()))
            .Callback<UnitPackage, CancellationToken>((package, _) =>
            {
                if (package.PackageTypeId != fallbackTypeId)
                {
                    package.Id = clonedPackageId;
                }
            })
            .Returns((UnitPackage package, CancellationToken _) => Task.FromResult(package));
        packageRepository
            .Setup(repository => repository.HasContractReferenceAsync(
                commonPackage.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        packageRepository
            .Setup(repository => repository.HasContractReferenceAsync(
                clonedPackageId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork
            .Setup(value => value.ExecuteInTransactionAsync(
                It.IsAny<Func<CancellationToken, Task>>(),
                It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task> action, CancellationToken token) => action(token));
        var masterDataService = new Mock<IMasterDataService>();
        masterDataService
            .Setup(service => service.GetValuesAsync(
                It.IsAny<IReadOnlyCollection<MasterDataKeyModel>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<MasterDataKeyModel, MasterDataValueModel>
            {
                [new(MasterDataTypeEnum.UnitPackageType, MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE)] = new()
                {
                    Id = fallbackTypeId
                },
                [new(MasterDataTypeEnum.UnitPackageStatus, MASTER_CODE_UNIT_PACKAGE_STATUS_ACTIVE)] = new()
                {
                    Id = 201
                }
            });
        var sut = new RoomPackageService(
            unitRepository.Object,
            packageRepository.Object,
            Mock.Of<IUnitPackageItemRepository>(),
            masterDataService.Object,
            unitOfWork.Object,
            Mock.Of<ILogger<RoomPackageService>>());

        // Act
        var result = await sut.DeleteAsync(new RoomPackageDeleteRequestModel
        {
            RoomPublicId = roomPublicId,
            PackagePublicId = commonPackage.PublicId,
            CurrentParty = new CurrentPartyContextModel { PartyId = 10 }
        });

        // Assert
        result.Should().NotBeNull();
        packageRepository.Verify(repository => repository.HasContractReferenceAsync(
            clonedPackageId,
            It.IsAny<CancellationToken>()), Times.Once);
        packageRepository.Verify(repository => repository.HasContractReferenceAsync(
            commonPackage.Id,
            It.IsAny<CancellationToken>()), Times.Never);
        packageRepository.Verify(repository => repository.DeletePackagesWithItems(
            It.Is<IReadOnlyCollection<UnitPackage>>(packages => packages.Single().Id == clonedPackageId)), Times.Once);
        packageRepository.Verify(repository => repository.AddAsync(
            It.Is<UnitPackage>(package => package.PackageTypeId != fallbackTypeId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private static RoomPackageService CreateSut(IUnitRepository unitRepository)
    {
        return new RoomPackageService(
            unitRepository,
            Mock.Of<IUnitPackageRepository>(),
            Mock.Of<IUnitPackageItemRepository>(),
            Mock.Of<IMasterDataService>(),
            Mock.Of<IUnitOfWork>(),
            Mock.Of<ILogger<RoomPackageService>>());
    }
}
