using Haven.Application.Commands.CreateProperty;
using Haven.Application.Dtos.Properties.Common;
using Haven.Application.Dtos.Properties.Create;
using Haven.Application.Dtos.Properties.Detail;
using Haven.Application.Dtos.Properties.List;
using Haven.Application.Interfaces.Repositories;
using Haven.Application.Interfaces.Services;
using Haven.Application.Mappings;
using Haven.Application.Models.Locations;
using Haven.Application.Models.MasterData;
using Haven.Application.Models.Parties;
using Haven.Application.Models.Properties.Create;
using Haven.Application.Models.Properties.Detail;
using Haven.Application.Models.Properties.List;
using Haven.Application.Models.Properties.QueryParameters;
using Haven.Application.Models.Properties.Rows;
using Haven.Application.Queries.GetProperties;
using Haven.Application.Queries.GetPropertyDetail;
using Haven.Domain.Entities;
using Haven.Domain.Enums;
using Haven.Infrastructure.Dependencies;
using Haven.Infrastructure.Services;
using Be.Haven.Core.Interfaces.Repositories;
using Mapster;
using Microsoft.AspNetCore.Http;
using static Haven.Application.Constants.ApplicationConstants;

namespace Be.Haven.Tests.Services.Businesses.Haven.Infrastructure.Services;

public sealed class PropertyServiceTests
{
    static PropertyServiceTests()
    {
        new PropertyMapping().Register(TypeAdapterConfig.GlobalSettings);
    }

    [Fact]
    public async Task CreatePropertyAsync_Should_CreatePropertyUnitsPartyAndPolicies_When_RequestIsValid()
    {
        var currentParty = new CurrentPartyContextModel
        {
            PartyId = 123,
            PartyPublicId = Guid.NewGuid(),
            PartyTypeCode = MASTER_CODE_PARTY_TYPE_LANDLORD,
            StatusCode = MASTER_CODE_ACTIVE
        };
        var masterDataService = new Mock<IMasterDataService>();
        var locationService = new Mock<ILocationService>();
        var propertyRepository = new Mock<IPropertyRepository>();
        var unitRepository = new Mock<IUnitRepository>();
        var unitPackageRepository = new Mock<IUnitPackageRepository>();
        var unitPackageItemRepository = new Mock<IUnitPackageItemRepository>();
        var propertyPartyRepository = new Mock<IPropertyPartyRepository>();
        var rentalChargePolicyRepository = new Mock<IRentalChargePolicyRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var logger = new Mock<ILogger<PropertyService>>();

        Property capturedProperty = null;
        IReadOnlyList<Unit> capturedUnits = [];
        IReadOnlyList<UnitPackage> capturedUnitPackages = [];
        IReadOnlyList<UnitPackageItem> capturedUnitPackageItems = [];
        PropertyParty capturedPropertyParty = null;
        IReadOnlyList<RentalChargePolicy> capturedPolicies = [];

        SetupPropertyCreationContext(masterDataService, locationService);
        propertyRepository
            .SetupSequence(x => x.PropertyCodeExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);
        propertyRepository
            .Setup(x => x.AddAsync(It.IsAny<Property>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Property entity, CancellationToken _) =>
            {
                capturedProperty = entity;
                return entity;
            });
        unitRepository
            .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<Unit>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<Unit>, CancellationToken>((entities, _) => capturedUnits = entities.ToList())
            .Returns(Task.CompletedTask);
        unitPackageRepository
            .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<UnitPackage>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<UnitPackage>, CancellationToken>((entities, _) => capturedUnitPackages = entities.ToList())
            .Returns(Task.CompletedTask);
        unitPackageItemRepository
            .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<UnitPackageItem>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<UnitPackageItem>, CancellationToken>((entities, _) => capturedUnitPackageItems = entities.ToList())
            .Returns(Task.CompletedTask);
        propertyPartyRepository
            .Setup(x => x.AddAsync(It.IsAny<PropertyParty>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PropertyParty entity, CancellationToken _) =>
            {
                capturedPropertyParty = entity;
                return entity;
            });
        rentalChargePolicyRepository
            .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<RentalChargePolicy>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<RentalChargePolicy>, CancellationToken>((entities, _) => capturedPolicies = entities.ToList())
            .Returns(Task.CompletedTask);
        unitOfWork
            .Setup(x => x.ExecuteInTransactionAsync(
                It.IsAny<Func<CancellationToken, Task<CreatedPropertyResponseDto>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task<CreatedPropertyResponseDto>>, CancellationToken>(
                (action, token) => action(token));

        var service = new PropertyService(
            new PropertyRepositoryDependencies(
                propertyRepository.Object,
                unitRepository.Object,
                unitPackageRepository.Object,
                unitPackageItemRepository.Object,
                propertyPartyRepository.Object,
                rentalChargePolicyRepository.Object),
            masterDataService.Object,
            locationService.Object,
            unitOfWork.Object,
            logger.Object);
        var request = BuildValidCommand().Adapt<PropertyCreationRequestModel>();
        request.CurrentParty = currentParty;

        var result = await service.CreatePropertyAsync(request);

        capturedProperty.Should().NotBeNull();
        capturedProperty.PropertyCode.Should().StartWith($"{PROPERTY_CODE_PREFIX}_");
        capturedProperty.PropertyTypeId.Should().Be(100);
        capturedProperty.StatusId.Should().Be(101);
        capturedProperty.ProvinceId.Should().Be(10);
        capturedProperty.DistrictId.Should().Be(20);
        capturedProperty.WardId.Should().Be(30);
        capturedProperty.IsPublished.Should().BeFalse();
        capturedProperty.TotalFloors.Should().Be(2);
        capturedProperty.TotalUnits.Should().Be(4);

        capturedUnits.Should().HaveCount(4);
        capturedUnits.Select(x => x.UnitCode).Should().BeEquivalentTo(["101", "102", "201", "202"]);
        capturedUnits.Should().OnlyContain(x => x.Property == capturedProperty);
        capturedUnits.Should().OnlyContain(x => x.IsPublished == false);
        capturedUnits.Should().OnlyContain(x => x.StatusId == 104);
        capturedUnits.Select(x => x.FloorNumber).Should().BeEquivalentTo([1, 1, 2, 2]);

        capturedUnitPackages.Should().HaveCount(4);
        capturedUnitPackages.Should().OnlyContain(x => x.PackageCode == MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE);
        capturedUnitPackages.Should().OnlyContain(x => x.PackageName == MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE);
        capturedUnitPackages.Should().OnlyContain(x => x.PriceAdjustment == 0 && x.StatusId == 109);
        capturedUnitPackages.Select(x => x.Unit).Should().BeEquivalentTo(capturedUnits);
        capturedUnitPackageItems.Should().BeEmpty();
        unitPackageItemRepository.Verify(
            x => x.AddRangeAsync(It.IsAny<IEnumerable<UnitPackageItem>>(), It.IsAny<CancellationToken>()),
            Times.Never);

        capturedPropertyParty.Should().NotBeNull();
        capturedPropertyParty.Property.Should().Be(capturedProperty);
        capturedPropertyParty.PartyId.Should().Be(currentParty.PartyId);
        capturedPropertyParty.RelationshipTypeId.Should().Be(105);
        capturedPropertyParty.StartDate.Should().NotBeNull();

        capturedPolicies.Should().ContainSingle();
        capturedPolicies[0].Property.Should().Be(capturedProperty);
        capturedPolicies[0].LineTypeId.Should().Be(107);
        capturedPolicies[0].StatusId.Should().Be(106);
        capturedPolicies[0].IsUsageBased.Should().BeTrue();
        capturedPolicies[0].Amount.Should().Be(3500);

        result.Id.Should().Be(capturedProperty.PublicId);
        result.TotalFloors.Should().Be(2);
        result.TotalRooms.Should().Be(4);
        result.Floors.Should().HaveCount(2);
        propertyRepository.Verify(
            x => x.PropertyCodeExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
        masterDataService.Verify(
            x => x.GetValuesAsync(It.IsAny<IReadOnlyCollection<MasterDataKeyModel>>(), It.IsAny<CancellationToken>()),
            Times.Once);
        locationService.Verify(
            x => x.GetLocationsAsync(It.IsAny<LocationKeyModel>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreatePropertyAsync_Should_CreateExplicitRooms_When_RequestSuppliesFloors()
    {
        var currentParty = new CurrentPartyContextModel
        {
            PartyId = 123,
            PartyPublicId = Guid.NewGuid(),
            PartyTypeCode = MASTER_CODE_PARTY_TYPE_LANDLORD,
            StatusCode = MASTER_CODE_ACTIVE
        };
        var masterDataService = new Mock<IMasterDataService>();
        var locationService = new Mock<ILocationService>();
        var propertyRepository = new Mock<IPropertyRepository>();
        var unitRepository = new Mock<IUnitRepository>();
        var unitPackageRepository = new Mock<IUnitPackageRepository>();
        var unitPackageItemRepository = new Mock<IUnitPackageItemRepository>();
        var propertyPartyRepository = new Mock<IPropertyPartyRepository>();
        var rentalChargePolicyRepository = new Mock<IRentalChargePolicyRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        Property capturedProperty = null;
        IReadOnlyList<Unit> capturedUnits = [];
        IReadOnlyList<UnitPackage> capturedUnitPackages = [];

        SetupPropertyCreationContext(masterDataService, locationService);
        propertyRepository
            .Setup(x => x.PropertyCodeExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        propertyRepository
            .Setup(x => x.AddAsync(It.IsAny<Property>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Property entity, CancellationToken _) =>
            {
                capturedProperty = entity;
                return entity;
            });
        unitRepository
            .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<Unit>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<Unit>, CancellationToken>((entities, _) => capturedUnits = entities.ToList())
            .Returns(Task.CompletedTask);
        unitPackageRepository
            .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<UnitPackage>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<UnitPackage>, CancellationToken>((entities, _) => capturedUnitPackages = entities.ToList())
            .Returns(Task.CompletedTask);
        propertyPartyRepository
            .Setup(x => x.AddAsync(It.IsAny<PropertyParty>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PropertyParty entity, CancellationToken _) => entity);
        rentalChargePolicyRepository
            .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<RentalChargePolicy>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        unitOfWork
            .Setup(x => x.ExecuteInTransactionAsync(
                It.IsAny<Func<CancellationToken, Task<CreatedPropertyResponseDto>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task<CreatedPropertyResponseDto>>, CancellationToken>(
                (action, token) => action(token));

        var service = new PropertyService(
            new PropertyRepositoryDependencies(
                propertyRepository.Object,
                unitRepository.Object,
                unitPackageRepository.Object,
                unitPackageItemRepository.Object,
                propertyPartyRepository.Object,
                rentalChargePolicyRepository.Object),
            masterDataService.Object,
            locationService.Object,
            unitOfWork.Object,
            Mock.Of<ILogger<PropertyService>>());
        var request = BuildValidCommand().Adapt<PropertyCreationRequestModel>();
        request.CurrentParty = currentParty;
        request.StructureSetup = new CreatePropertyStructureRequestDto
        {
            Floors =
            [
                new CreatePropertyFloorRequestDto
                {
                    FloorNumber = 1,
                    Rooms =
                    [
                        new CreatePropertyRoomRequestDto
                        {
                            Code = "A101",
                            Name = "Phòng A101",
                            TypeCode = MASTER_CODE_UNIT_TYPE_ROOM,
                            RentalModeCode = MASTER_CODE_RENTAL_MODE_WHOLE_UNIT,
                            BaseRentAmount = 6_000_000,
                            DefaultDepositAmount = 6_000_000,
                            IsPetAllowed = true
                        }
                    ]
                },
                new CreatePropertyFloorRequestDto
                {
                    FloorNumber = 2,
                    Rooms =
                    [
                        new CreatePropertyRoomRequestDto
                        {
                            Code = "A201",
                            Name = "Phòng A201",
                            TypeCode = MASTER_CODE_UNIT_TYPE_ROOM,
                            RentalModeCode = MASTER_CODE_RENTAL_MODE_WHOLE_UNIT,
                            BaseRentAmount = 7_000_000
                        }
                    ]
                }
            ]
        };

        var result = await service.CreatePropertyAsync(request);

        capturedProperty.TotalFloors.Should().Be(2);
        capturedProperty.TotalUnits.Should().Be(2);
        capturedUnits.Should().HaveCount(2);
        capturedUnits.Select(unit => unit.UnitCode).Should().Equal("A101", "A201");
        capturedUnits[0].UnitName.Should().Be("Phòng A101");
        capturedUnits[0].FloorNumber.Should().Be(1);
        capturedUnits[0].DefaultDepositAmount.Should().Be(6_000_000);
        capturedUnits[0].IsPetAllowed.Should().BeTrue();
        capturedUnits[1].FloorNumber.Should().Be(2);
        capturedUnits[1].BaseRentAmount.Should().Be(7_000_000);
        capturedUnitPackages.Should().HaveCount(2);
        capturedUnitPackages.Should().OnlyContain(package => package.PackageCode == MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE);
        result.TotalFloors.Should().Be(2);
        result.TotalRooms.Should().Be(2);
        result.Floors.SelectMany(floor => floor.RoomCodes).Should().Equal("A101", "A201");
    }

    [Fact]
    public async Task CreatePropertyAsync_Should_CreateCustomPackagesAndItems_When_RequestSuppliesPackages()
    {
        var currentParty = new CurrentPartyContextModel
        {
            PartyId = 123,
            PartyPublicId = Guid.NewGuid(),
            PartyTypeCode = MASTER_CODE_PARTY_TYPE_LANDLORD,
            StatusCode = MASTER_CODE_ACTIVE
        };
        var masterDataService = new Mock<IMasterDataService>();
        var locationService = new Mock<ILocationService>();
        var propertyRepository = new Mock<IPropertyRepository>();
        var unitRepository = new Mock<IUnitRepository>();
        var unitPackageRepository = new Mock<IUnitPackageRepository>();
        var unitPackageItemRepository = new Mock<IUnitPackageItemRepository>();
        var propertyPartyRepository = new Mock<IPropertyPartyRepository>();
        var rentalChargePolicyRepository = new Mock<IRentalChargePolicyRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        IReadOnlyList<UnitPackage> capturedUnitPackages = [];
        IReadOnlyList<UnitPackageItem> capturedUnitPackageItems = [];

        SetupPropertyCreationContext(masterDataService, locationService);
        propertyRepository
            .Setup(x => x.PropertyCodeExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        propertyRepository
            .Setup(x => x.AddAsync(It.IsAny<Property>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Property entity, CancellationToken _) => entity);
        unitRepository
            .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<Unit>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        unitPackageRepository
            .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<UnitPackage>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<UnitPackage>, CancellationToken>((entities, _) => capturedUnitPackages = entities.ToList())
            .Returns(Task.CompletedTask);
        unitPackageItemRepository
            .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<UnitPackageItem>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<UnitPackageItem>, CancellationToken>((entities, _) => capturedUnitPackageItems = entities.ToList())
            .Returns(Task.CompletedTask);
        propertyPartyRepository
            .Setup(x => x.AddAsync(It.IsAny<PropertyParty>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PropertyParty entity, CancellationToken _) => entity);
        rentalChargePolicyRepository
            .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<RentalChargePolicy>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        unitOfWork
            .Setup(x => x.ExecuteInTransactionAsync(
                It.IsAny<Func<CancellationToken, Task<CreatedPropertyResponseDto>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task<CreatedPropertyResponseDto>>, CancellationToken>(
                (action, token) => action(token));

        var service = new PropertyService(
            new PropertyRepositoryDependencies(
                propertyRepository.Object,
                unitRepository.Object,
                unitPackageRepository.Object,
                unitPackageItemRepository.Object,
                propertyPartyRepository.Object,
                rentalChargePolicyRepository.Object),
            masterDataService.Object,
            locationService.Object,
            unitOfWork.Object,
            Mock.Of<ILogger<PropertyService>>());
        var request = BuildValidCommand().Adapt<PropertyCreationRequestModel>();
        request.CurrentParty = currentParty;
        request.Packages =
        [
            new CreatePropertyPackageRequestDto
            {
                Name = "Gói Full Nội thất",
                PriceAdjustment = 1_500_000,
                Items =
                [
                    new CreatePropertyPackageItemRequestDto { Name = "Giường ngủ King Size" },
                    new CreatePropertyPackageItemRequestDto { Name = "Tủ lạnh 200L" }
                ]
            }
        ];

        var result = await service.CreatePropertyAsync(request);

        capturedUnitPackages.Should().HaveCount(8);
        capturedUnitPackages.Count(package => package.PackageCode == MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE).Should().Be(4);
        capturedUnitPackages.Count(package => package.PackageCode == "PKG-001").Should().Be(4);
        capturedUnitPackages.Where(package => package.PackageCode == "PKG-001")
            .Should().OnlyContain(package => package.PackageName == "Gói Full Nội thất" && package.PriceAdjustment == 1_500_000);
        capturedUnitPackageItems.Should().HaveCount(8);
        capturedUnitPackageItems.Should().OnlyContain(item => item.UnitPackage.PackageCode == "PKG-001");
        result.Floors.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreatePropertyAsync_Should_ThrowApiException_When_PropertyCodeGenerationIsExhausted()
    {
        var currentParty = new CurrentPartyContextModel
        {
            PartyId = 123,
            PartyPublicId = Guid.NewGuid(),
            PartyTypeCode = MASTER_CODE_PARTY_TYPE_LANDLORD,
            StatusCode = MASTER_CODE_ACTIVE
        };
        var masterDataService = new Mock<IMasterDataService>();
        var locationService = new Mock<ILocationService>();
        var propertyRepository = new Mock<IPropertyRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var service = new PropertyService(
            new PropertyRepositoryDependencies(
                propertyRepository.Object,
                Mock.Of<IUnitRepository>(),
                Mock.Of<IUnitPackageRepository>(),
                Mock.Of<IUnitPackageItemRepository>(),
                Mock.Of<IPropertyPartyRepository>(),
                Mock.Of<IRentalChargePolicyRepository>()),
            masterDataService.Object,
            locationService.Object,
            unitOfWork.Object,
            Mock.Of<ILogger<PropertyService>>());
        var request = BuildValidCommand().Adapt<PropertyCreationRequestModel>();
        request.CurrentParty = currentParty;

        SetupPropertyCreationContext(masterDataService, locationService);
        propertyRepository
            .Setup(x => x.PropertyCodeExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = () => service.CreatePropertyAsync(request);

        await act.Should()
            .ThrowAsync<ApiException>()
            .Where(exception =>
                exception.Message == ERROR_PROPERTY_CODE_GENERATION_FAILED
                && exception.ErrorCode == INTERNAL_SERVER
                && exception.StatusCode == StatusCodes.Status500InternalServerError);
        var expectedAttempts = (32 - PROPERTY_CODE_RANDOM_LENGTH + 1) * PROPERTY_CODE_MAX_ATTEMPTS;
        propertyRepository.Verify(
            x => x.PropertyCodeExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(expectedAttempts));
        unitOfWork.Verify(
            x => x.ExecuteInTransactionAsync(
                It.IsAny<Func<CancellationToken, Task<CreatedPropertyResponseDto>>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetPropertiesAsync_Should_ReturnNestedFloorsAndRooms_When_RowsExist()
    {
        var currentParty = new CurrentPartyContextModel
        {
            PartyId = 123,
            PartyPublicId = Guid.NewGuid()
        };
        var propertyPublicId = Guid.NewGuid();
        var unitPublicId = Guid.NewGuid();
        var propertyRepository = new Mock<IPropertyRepository>();
        var service = CreateService(propertyRepository);

        propertyRepository
            .Setup(x => x.GetPropertyPageAsync(
                It.Is<PropertyListQueryParametersModel>(parameters =>
                    parameters.CurrentPartyId == currentParty.PartyId
                    && parameters.Search == "toa"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([BuildPropertyRow(propertyPublicId, totalCount: 1)]);
        propertyRepository
            .Setup(x => x.GetFloorsAsync(
                It.Is<PropertyChildRowsQueryParametersModel>(parameters =>
                    parameters.PropertyPublicIds.Contains(propertyPublicId)
                    && parameters.Search == "toa"
                    && parameters.FloorNumber == 1
                    && parameters.RoomStatusCode == MASTER_CODE_UNIT_STATUS_AVAILABLE
                    && parameters.PaymentStatusCode == "PAID"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new PropertyFloorRowModel
                {
                    PropertyPublicId = propertyPublicId,
                    FloorNumber = 1,
                    UnitCount = 1,
                    AvailableUnitCount = 1
                }
            ]);
        propertyRepository
            .Setup(x => x.GetRoomsAsync(
                It.Is<PropertyChildRowsQueryParametersModel>(parameters =>
                    parameters.PropertyPublicIds.Contains(propertyPublicId)
                    && parameters.Search == "toa"
                    && parameters.FloorNumber == 1
                    && parameters.RoomStatusCode == MASTER_CODE_UNIT_STATUS_AVAILABLE
                    && parameters.PaymentStatusCode == "PAID"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new PropertyRoomRowModel
                {
                    PropertyPublicId = propertyPublicId,
                    UnitPublicId = unitPublicId,
                    UnitCode = "101",
                    UnitName = "Phòng 101",
                    FloorNumber = 1,
                    BaseRentAmount = 5_500_000,
                    TotalRentAmount = 6_000_000,
                    UnitTypeCode = MASTER_CODE_UNIT_TYPE_ROOM,
                    UnitTypeName = "Phòng",
                    RentalModeCode = MASTER_CODE_RENTAL_MODE_WHOLE_UNIT,
                    RentalModeName = "Cho thuê nguyên căn",
                    StatusCode = MASTER_CODE_UNIT_STATUS_AVAILABLE,
                    StatusName = "Còn trống"
                }
            ]);
        propertyRepository
            .Setup(x => x.GetRoomTenantsAsync(
                It.Is<PropertyChildRowsQueryParametersModel>(parameters =>
                    parameters.PropertyPublicIds.Contains(propertyPublicId)
                    && parameters.Search == "toa"
                    && parameters.FloorNumber == 1
                    && parameters.RoomStatusCode == MASTER_CODE_UNIT_STATUS_AVAILABLE
                    && parameters.PaymentStatusCode == "PAID"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var request = new PropertyListRequestModel
        {
            CurrentParty = currentParty,
            Search = " toa ",
            FloorNumber = 1,
            RoomStatusCode = $" {MASTER_CODE_UNIT_STATUS_AVAILABLE} ",
            PaymentStatusCode = " PAID ",
            PageNumber = 1,
            PageSize = 20
        };

        var result = await service.GetPropertiesAsync(request);

        result.Total.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items[0].Id.Should().Be(propertyPublicId);
        result.Items[0].Floors.Should().ContainSingle();
        result.Items[0].Floors[0].Number.Should().Be(1);
        result.Items[0].Floors[0].Rooms.Should().ContainSingle();
        result.Items[0].Floors[0].Rooms[0].Id.Should().Be(unitPublicId);
        result.Items[0].Floors[0].Rooms[0].RentalModeCode.Should().Be(MASTER_CODE_RENTAL_MODE_WHOLE_UNIT);
        result.Items[0].Floors[0].Rooms[0].TotalRentAmount.Should().Be(6_000_000);
    }

    [Fact]
    public async Task GetPropertiesAsync_Should_AlwaysLoadRoomsAndTenants_ForPropertyList()
    {
        var currentParty = new CurrentPartyContextModel
        {
            PartyId = 123,
            PartyPublicId = Guid.NewGuid()
        };
        var propertyPublicId = Guid.NewGuid();
        var unitPublicId = Guid.NewGuid();
        var propertyRepository = new Mock<IPropertyRepository>();
        var service = CreateService(propertyRepository);

        propertyRepository
            .Setup(x => x.GetPropertyPageAsync(It.IsAny<PropertyListQueryParametersModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([BuildPropertyRow(propertyPublicId, totalCount: 5)]);
        propertyRepository
            .Setup(x => x.GetFloorsAsync(It.IsAny<PropertyChildRowsQueryParametersModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new PropertyFloorRowModel
                {
                    PropertyPublicId = propertyPublicId,
                    FloorNumber = 1,
                    UnitCount = 1,
                    AvailableUnitCount = 1
                }
            ]);
        propertyRepository
            .Setup(x => x.GetRoomsAsync(It.IsAny<PropertyChildRowsQueryParametersModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new PropertyRoomRowModel
                {
                    PropertyPublicId = propertyPublicId,
                    UnitPublicId = unitPublicId,
                    UnitCode = "101",
                    UnitName = "Phòng 101",
                    FloorNumber = 1,
                    BaseRentAmount = 5_500_000,
                    TotalRentAmount = 5_500_000,
                    RentalModeCode = MASTER_CODE_RENTAL_MODE_WHOLE_UNIT,
                    RentalModeName = "Cho thuê nguyên phòng",
                    StatusCode = MASTER_CODE_UNIT_STATUS_AVAILABLE,
                    StatusName = "Còn trống"
                }
            ]);
        propertyRepository
            .Setup(x => x.GetRoomTenantsAsync(It.IsAny<PropertyChildRowsQueryParametersModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var request = new PropertyListRequestModel
        {
            CurrentParty = currentParty,
            PageNumber = 1,
            PageSize = 20
        };

        var result = await service.GetPropertiesAsync(request);

        result.Total.Should().Be(5);
        result.Items.Should().ContainSingle();
        result.Items[0].Floors.Should().ContainSingle();
        result.Items[0].Floors[0].Rooms.Should().ContainSingle();
        propertyRepository.Verify(
            x => x.GetRoomsAsync(It.IsAny<PropertyChildRowsQueryParametersModel>(), It.IsAny<CancellationToken>()),
            Times.Once);
        propertyRepository.Verify(
            x => x.GetRoomTenantsAsync(It.IsAny<PropertyChildRowsQueryParametersModel>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetPropertyDetailAsync_Should_ThrowNotFound_When_PropertyIsOutsideCurrentPartyScope()
    {
        var currentParty = new CurrentPartyContextModel
        {
            PartyId = 123,
            PartyPublicId = Guid.NewGuid()
        };
        var propertyRepository = new Mock<IPropertyRepository>();
        var service = CreateService(propertyRepository);

        propertyRepository
            .Setup(x => x.GetPropertyDetailHeaderAsync(
                It.Is<PropertyScopedQueryParametersModel>(parameters => parameters.CurrentPartyId == currentParty.PartyId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PropertyRowModel)null);

        var act = () => service.GetPropertyDetailAsync(
            new PropertyDetailRequestModel
            {
                CurrentParty = currentParty,
                PropertyPublicId = Guid.NewGuid()
            });

        await act.Should()
            .ThrowAsync<ApiException>()
            .Where(exception => exception.StatusCode == StatusCodes.Status404NotFound);
    }

    private static CreatePropertyCommand BuildValidCommand()
    {
        return new CreatePropertyCommand
        {
            Name = "Tòa nhà A",
            PropertyTypeCode = "BUILDING",
            ProvinceCode = "HCM",
            DistrictCode = "D1",
            WardCode = "W1",
            StreetAddress = "123 Nguyễn Văn Linh",
            FormattedAddress = "123 Nguyễn Văn Linh, Quận 1, TP.HCM",
            Latitude = 10.1m,
            Longitude = 106.7m,
            StructureSetup = new CreatePropertyStructureRequestDto
            {
                TotalFloors = 2,
                RoomsPerFloor = 2,
                RoomNumberingPattern = DEFAULT_ROOM_NUMBERING_PATTERN,
                DefaultUnitTypeCode = MASTER_CODE_UNIT_TYPE_ROOM,
                DefaultRentalModeCode = MASTER_CODE_RENTAL_MODE_WHOLE_UNIT,
                DefaultBaseRentAmount = 5_500_000,
                DefaultDepositAmount = 5_500_000
            },
            ChargePolicies =
            [
                new CreatePropertyChargePolicyRequestDto
                {
                    Code = "ELECTRIC",
                    Name = "Tiền điện",
                    Amount = 3500,
                    CalculationMethodCode = CALCULATION_METHOD_METER_READING
                }
            ]
        };
    }

    private static IReadOnlyDictionary<MasterDataKeyModel, MasterDataValueModel> ResolveMasterData(
        IReadOnlyCollection<MasterDataKeyModel> keys,
        CancellationToken _)
    {
        var ids = new Dictionary<(string Type, string Code), long>
        {
            [(MASTER_TYPE_PROPERTY_TYPE, "BUILDING")] = 100,
            [(MASTER_TYPE_PROPERTY_STATUS, MASTER_CODE_PROPERTY_STATUS_DRAFT)] = 101,
            [(MASTER_TYPE_UNIT_TYPE, MASTER_CODE_UNIT_TYPE_ROOM)] = 102,
            [(MASTER_TYPE_UNIT_RENTAL_MODE, MASTER_CODE_RENTAL_MODE_WHOLE_UNIT)] = 103,
            [(MASTER_TYPE_UNIT_STATUS, MASTER_CODE_UNIT_STATUS_AVAILABLE)] = 104,
            [(MASTER_TYPE_PROPERTY_RELATIONSHIP_TYPE, PropertyRelationshipTypeEnum.Landlord.ToMasterDataCode())] = 105,
            [(MASTER_TYPE_COMMON_STATUS, MASTER_CODE_ACTIVE)] = 106,
            [(MASTER_TYPE_INVOICE_LINE_TYPE, "ELECTRIC")] = 107,
            [(MASTER_TYPE_UNIT_PACKAGE_TYPE, MASTER_CODE_UNIT_PACKAGE_TYPE_CUSTOM)] = 110,
            [(MASTER_TYPE_UNIT_PACKAGE_TYPE, MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE)] = 108,
            [(MASTER_TYPE_UNIT_PACKAGE_STATUS, MASTER_CODE_UNIT_PACKAGE_STATUS_ACTIVE)] = 109
        };

        return keys.ToDictionary(
            key => key,
            key => new MasterDataValueModel
            {
                Id = ids[(key.Type, key.Code)],
                Type = key.Type,
                Code = key.Code,
                Name = key.Code,
                Description = key.Type == MASTER_TYPE_UNIT_PACKAGE_TYPE ? "Default zero-price package." : null
            });
    }

    private static void SetupPropertyCreationContext(
        Mock<IMasterDataService> masterDataService,
        Mock<ILocationService> locationService)
    {
        masterDataService
            .Setup(x => x.GetValuesAsync(
                It.IsAny<IReadOnlyCollection<MasterDataKeyModel>>(),
                It.IsAny<CancellationToken>()))
            .Returns<IReadOnlyCollection<MasterDataKeyModel>, CancellationToken>(
                (keys, token) => Task.FromResult(ResolveMasterData(keys, token)));
        locationService
            .Setup(x => x.GetLocationsAsync(
                It.Is<LocationKeyModel>(key =>
                    key.ProvinceCode == "HCM"
                    && key.DistrictCode == "D1"
                    && key.WardCode == "W1"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PropertyLocationContextModel
            {
                Province = new LocationLookupModel { Id = 10, Code = "HCM", Name = "TP.HCM" },
                District = new LocationLookupModel { Id = 20, Code = "D1", Name = "Quận 1" },
                Ward = new LocationLookupModel { Id = 30, Code = "W1", Name = "Phường 1" }
            });
    }

    private static PropertyService CreateService(Mock<IPropertyRepository> propertyRepository)
    {
        return new PropertyService(
            new PropertyRepositoryDependencies(
                propertyRepository.Object,
                Mock.Of<IUnitRepository>(),
                Mock.Of<IUnitPackageRepository>(),
                Mock.Of<IUnitPackageItemRepository>(),
                Mock.Of<IPropertyPartyRepository>(),
                Mock.Of<IRentalChargePolicyRepository>()),
            Mock.Of<IMasterDataService>(),
            Mock.Of<ILocationService>(),
            Mock.Of<IUnitOfWork>(),
            Mock.Of<ILogger<PropertyService>>());
    }

    private static PropertyRowModel BuildPropertyRow(Guid propertyPublicId, int totalCount = 1)
    {
        return new PropertyRowModel
        {
            TotalCount = totalCount,
            PropertyId = 10,
            PropertyPublicId = propertyPublicId,
            PropertyCode = "PROP-0001",
            Name = "Tòa nhà A",
            PropertyTypeCode = "BUILDING",
            PropertyTypeName = "Tòa nhà",
            StatusCode = MASTER_CODE_PROPERTY_STATUS_DRAFT,
            StatusName = "Bản nháp",
            ProvinceCode = "HCM",
            ProvinceName = "TP.HCM",
            DistrictCode = "D1",
            DistrictName = "Quận 1",
            WardCode = "W1",
            WardName = "Phường 1",
            TotalFloors = 1,
            TotalUnits = 1,
            AvailableUnitCount = 1
        };
    }
}
