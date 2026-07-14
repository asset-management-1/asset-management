using Haven.Application.Constants;
using Haven.Application.Interfaces.Repositories;
using Haven.Application.Interfaces.Services;
using Haven.Application.Mappings.Vehicles;
using Haven.Application.Models.MasterData;
using Haven.Application.Models.Parties;
using Haven.Application.Models.Vehicles.Create;
using Haven.Application.Models.Vehicles.Detail;
using Haven.Application.Models.Vehicles.List;
using Haven.Application.Models.Vehicles.QueryParameters;
using Haven.Application.Models.Vehicles.Rows;
using Haven.Infrastructure.Services.Vehicles;
using Be.Haven.Core.Interfaces.Repositories;

namespace Be.Haven.Tests.Services.Businesses.Haven.Infrastructure.Services.Vehicles;

public sealed class VehicleServiceTests
{
    [Fact]
    public async Task GetVehiclesAsync_Should_MapEveryCompactRoomRowWithoutImageFields()
    {
        // Arrange
        var vehiclePublicId = Guid.NewGuid();
        var roomPublicId = Guid.NewGuid();
        var repository = new Mock<IVehicleRepository>();
        repository
            .Setup(value => value.GetByRoomAsync(
                It.Is<VehicleListQueryParametersModel>(parameters => parameters.RoomId == roomPublicId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new VehicleListRowModel
                {
                    VehiclePublicId = vehiclePublicId,
                    PayerTenantPublicId = Guid.NewGuid(),
                    PayerTenantName = "Nguyễn Văn An",
                    VehicleTypeCode = "MOTORBIKE",
                    VehicleTypeName = "Xe máy",
                    VehicleName = "Honda Vision",
                    LicensePlate = "59-X1 123.45"
                }
            ]);
        var sut = CreateSut(repository.Object);

        // Act
        var result = await sut.GetVehiclesAsync(new VehicleListRequestModel
        {
            CurrentParty = new CurrentPartyContextModel { PartyId = 100, PartyPublicId = Guid.NewGuid() },
            RoomId = roomPublicId
        });

        // Assert
        result.Should().ContainSingle();
        result.Single().Id.Should().Be(vehiclePublicId);
        result.Single().Payer.Tenant.Should().Be("Nguyễn Văn An");
    }

    [Fact]
    public async Task GetDetailAsync_Should_MapScopedProjection_When_VehicleExists()
    {
        // Arrange
        var vehiclePublicId = Guid.NewGuid();
        var roomPublicId = Guid.NewGuid();
        var currentPartyId = 100L;
        var repository = new Mock<IVehicleRepository>();
        repository
            .Setup(value => value.GetDetailAsync(
                It.Is<VehicleScopeQueryParametersModel>(parameters =>
                    parameters.VehiclePublicId == vehiclePublicId
                    && parameters.RoomPublicId == roomPublicId
                    && parameters.CurrentPartyId == currentPartyId
                    && parameters.RelationshipCodes.SequenceEqual(PROPERTY_ACCESS_RELATIONSHIP_CODES)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VehicleDetailRowModel
            {
                VehiclePublicId = vehiclePublicId,
                VehicleName = "Honda SH 150i",
                RoomPublicId = roomPublicId,
                RoomName = "Phòng 101",
                PayerTenantPublicId = Guid.NewGuid(),
                PayerTenantName = "Nguyễn Văn A"
            });
        var sut = CreateSut(repository.Object);

        // Act
        var result = await sut.GetDetailAsync(new VehicleDetailRequestModel
        {
            RoomPublicId = roomPublicId,
            VehiclePublicId = vehiclePublicId,
            CurrentParty = new CurrentPartyContextModel { PartyId = currentPartyId }
        });

        // Assert
        result.Id.Should().Be(vehiclePublicId);
        result.Name.Should().Be("Honda SH 150i");
        result.Room.Name.Should().Be("Phòng 101");
        result.Payer.Tenant.Should().Be("Nguyễn Văn A");
    }

    [Fact]
    public async Task GetDetailAsync_Should_ThrowNotFound_When_ScopedProjectionDoesNotExist()
    {
        // Arrange
        var repository = new Mock<IVehicleRepository>();
        repository
            .Setup(value => value.GetDetailAsync(
                It.IsAny<VehicleScopeQueryParametersModel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((VehicleDetailRowModel)null);
        var sut = CreateSut(repository.Object);

        // Act
        var action = () => sut.GetDetailAsync(new VehicleDetailRequestModel
        {
            RoomPublicId = Guid.NewGuid(),
            VehiclePublicId = Guid.NewGuid(),
            CurrentParty = new CurrentPartyContextModel { PartyId = 100 }
        });

        // Assert
        await action.Should().ThrowAsync<ApiException>()
            .Where(exception => exception.StatusCode == StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task CreateAsync_Should_Reject_When_PayerIsNotActivePrimaryTenantSigner()
    {
        var vehicleRepository = new Mock<IVehicleRepository>();
        vehicleRepository
            .Setup(repository => repository.GetPrimaryPayerAsync(
                It.IsAny<VehiclePayerEligibilityQueryParametersModel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((VehiclePayerRowModel)null);
        var sut = CreateSut(vehicleRepository.Object);
        var request = new VehicleCreateRequestModel
        {
            CurrentParty = new CurrentPartyContextModel { PartyId = 100, PartyPublicId = Guid.NewGuid() },
            RoomId = Guid.NewGuid(),
            PayerTenantId = Guid.NewGuid(),
            VehicleTypeCode = "MOTORBIKE",
            Name = "Honda SH 150i"
        };

        var action = () => sut.CreateAsync(request);

        await action.Should().ThrowAsync<ApiException>()
            .Where(exception => exception.Message == global::Haven.Application.Constants.ApplicationErrorConstants.VehicleErrors.ERROR_VEHICLE_PRIMARY_PAYER_REQUIRED);
    }

    private static VehicleService CreateSut(IVehicleRepository vehicleRepository)
    {
        var masterData = new Mock<IMasterDataService>();
        masterData.Setup(service => service.GetValuesAsync(It.IsAny<IReadOnlyCollection<MasterDataKeyModel>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateMasterData());

        return new VehicleService(
            vehicleRepository,
            masterData.Object,
            Mock.Of<IObjectStorageService>(),
            Mock.Of<IImageOptimizationService>(),
            OptionsFactory.Create(new R2StorageOptions()),
            Mock.Of<IUnitOfWork>(),
            Mock.Of<ILogger<VehicleService>>());
    }

    private static IReadOnlyDictionary<MasterDataKeyModel, MasterDataValueModel> CreateMasterData()
    {
        return new Dictionary<MasterDataKeyModel, MasterDataValueModel>
        {
            [new(MasterDataTypeEnum.VehicleType, "MOTORBIKE")] = new MasterDataValueModel { Id = 1, Code = "MOTORBIKE" },
            [new(MasterDataTypeEnum.OccupancyStatus, MASTER_CODE_OCCUPANCY_STATUS_ACTIVE)] = new MasterDataValueModel { Id = 2, Code = MASTER_CODE_OCCUPANCY_STATUS_ACTIVE },
            [new(MasterDataTypeEnum.ContractStatus, MASTER_CODE_CONTRACT_STATUS_ACTIVE)] = new MasterDataValueModel { Id = 3, Code = MASTER_CODE_CONTRACT_STATUS_ACTIVE }
        };
    }
}
