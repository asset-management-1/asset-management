using Be.Haven.Core.Interfaces.Repositories;
using Haven.Application.Interfaces.Repositories;
using Haven.Application.Interfaces.Services;
using Haven.Application.Models.MasterData;
using Haven.Application.Models.Meters.Mutation;
using Haven.Application.Models.Meters.QueryParameters;
using Haven.Application.Models.Meters.Rows;
using Haven.Application.Models.Parties;
using Haven.Domain.Entities;
using Haven.Infrastructure.Dependencies;
using Haven.Infrastructure.Models.Meters;
using Haven.Infrastructure.Services.Meters;

namespace Be.Haven.Tests.Services.Businesses.Haven.Infrastructure.Services.Meters;

public sealed class RoomMeterServiceTests
{
    [Fact]
    public async Task SaveConfirmedPeriodAsync_Should_ThrowBusinessError_WhenLockedInvoiceHasPayment()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var state = CreateMutationState(roomId);
        var invoice = new Invoice
        {
            Id = 50,
            PaidAmount = 1,
            StatusId = 1
        };
        var (sut, invoiceRepository, _, _) = CreateSut(state, invoice);
        var request = CreateRequest(roomId);

        // Act
        Func<Task> action = () => sut.SaveConfirmedPeriodAsync(
            request,
            MeterMutationModeEnum.Create);

        // Assert
        await action.Should().ThrowAsync<ApiException>()
            .Where(exception => exception.ErrorCode == BAD_REQUEST);
        invoiceRepository.Verify(repository => repository.GetInvoiceImpactAsync(
            It.IsAny<long>(),
            It.IsAny<DateOnly>(),
            It.IsAny<DateOnly>(),
            It.IsAny<CancellationToken>()), Times.Never);
        invoiceRepository.Verify(repository => repository.ReplaceUtilityLinesAsync(
            It.IsAny<Invoice>(),
            It.IsAny<IReadOnlyCollection<Meter>>(),
            It.IsAny<long>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SaveConfirmedPeriodAsync_Should_RejectCreate_WhenPeriodAlreadyExists()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var state = CreateMutationState(roomId);
        state.Electric.ExistingMeter = new Meter();
        var (sut, _, _, _) = CreateSut(state);
        var request = CreateRequest(roomId);

        // Act
        Func<Task> action = () => sut.SaveConfirmedPeriodAsync(
            request,
            MeterMutationModeEnum.Create);

        // Assert
        await action.Should().ThrowAsync<ApiException>()
            .Where(exception => exception.Message == global::Haven.Application.Constants.ApplicationErrorConstants.MeterErrors.ERROR_METER_ALREADY_EXISTS
                                && exception.StatusCode == StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task SaveConfirmedPeriodAsync_Should_RejectUpdate_WhenPeriodDoesNotExist()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var state = CreateMutationState(roomId);
        var (sut, _, _, _) = CreateSut(state);
        var request = CreateRequest(roomId);

        // Act
        Func<Task> action = () => sut.SaveConfirmedPeriodAsync(
            request,
            MeterMutationModeEnum.Update);

        // Assert
        await action.Should().ThrowAsync<ApiException>()
            .Where(exception => exception.Message == global::Haven.Application.Constants.ApplicationErrorConstants.MeterErrors.ERROR_METER_NOT_FOUND
                                && exception.StatusCode == StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task SaveConfirmedPeriodAsync_Should_CleanupUploadsWithIndependentToken_WhenRequestIsCancelled()
    {
        // Arrange
        using var cancellationSource = new CancellationTokenSource();
        var roomId = Guid.NewGuid();
        var state = CreateMutationState(roomId);
        var (sut, _, objectStorage, distributedLock) = CreateSut(state);
        var cleanupTokenWasCancelled = true;
        objectStorage
            .Setup(service => service.UploadAsync(
                It.IsAny<ObjectUploadRequestModel>(),
                It.IsAny<CancellationToken>()))
            .Callback(() => cancellationSource.Cancel())
            .ReturnsAsync(new ObjectUploadResponseModel
            {
                BucketName = "test",
                ObjectKey = "meters/test/electric.jpg",
                ContentType = JPEG_IMAGE_CONTENT_TYPE,
                FileSize = 1,
                Checksum = "checksum"
            });
        objectStorage
            .Setup(service => service.DeleteAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Callback((string _, CancellationToken token) => cleanupTokenWasCancelled = token.IsCancellationRequested)
            .ReturnsAsync(true);
        distributedLock
            .Setup(service => service.TryAcquireAsync(
                It.IsAny<string>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException(cancellationSource.Token));

        var request = CreateRequest(roomId);
        request.ElectricImages =
        [
            new FormFile(
                new MemoryStream([1]),
                0,
                1,
                "electricImages",
                "electric.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = JPEG_IMAGE_CONTENT_TYPE
            }
        ];

        // Act
        Func<Task> action = () => sut.SaveConfirmedPeriodAsync(
            request,
            MeterMutationModeEnum.Create,
            cancellationSource.Token);

        // Assert
        await action.Should().ThrowAsync<OperationCanceledException>();
        cleanupTokenWasCancelled.Should().BeFalse();
        objectStorage.Verify(service => service.DeleteAsync(
            "meters/test/electric.jpg",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SaveConfirmedPeriodAsync_Should_UseServerBaselineAndReplaceDraftInvoice_WhenCreateSucceeds()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var state = CreateMutationState(roomId, electricPreviousConfirmed: 110, waterPreviousConfirmed: 22);
        var lookups = MeterMutationLookupModel.Create(CreateMasterData(), false);
        var invoice = new Invoice
        {
            Id = 50,
            PaidAmount = 0,
            StatusId = lookups.DraftInvoiceStatus.Id
        };
        var (sut, invoiceRepository, _, _) = CreateSut(state, invoice);
        var request = CreateRequest(roomId);

        // Act
        var result = await sut.SaveConfirmedPeriodAsync(
            request,
            MeterMutationModeEnum.Create);

        // Assert
        var meters = state.GetMeters();
        meters.Should().HaveCount(2);
        meters.Single(meter => meter.LineTypeId == lookups.ElectricLineType.Id)
            .Should().Match<Meter>(meter => meter.PreviousReading == 110
                                           && meter.CurrentReading == 120
                                           && meter.UsageQuantity == 10);
        meters.Single(meter => meter.LineTypeId == lookups.WaterLineType.Id)
            .Should().Match<Meter>(meter => meter.PreviousReading == 22
                                           && meter.CurrentReading == 25
                                           && meter.UsageQuantity == 3);
        invoiceRepository.Verify(repository => repository.ReplaceUtilityLinesAsync(
            invoice,
            It.Is<IReadOnlyCollection<Meter>>(items => items.Count == 2),
            lookups.MeterReadingChargeMode.Id,
            It.IsAny<CancellationToken>()), Times.Once);
        result.Should().NotBeNull();
    }

    private static MeterMutationRequestModel CreateRequest(Guid roomId)
    {
        return new MeterMutationRequestModel
        {
            RoomId = roomId,
            BillingDate = new DateTime(2026, 7, 5),
            ElectricPrevious = 100,
            ElectricCurrent = 120,
            WaterPrevious = 20,
            WaterCurrent = 25,
            CurrentParty = new CurrentPartyContextModel { PartyId = 30 }
        };
    }

    private static MeterMutationStateModel CreateMutationState(
        Guid roomId,
        decimal? electricPreviousConfirmed = null,
        decimal? waterPreviousConfirmed = null)
    {
        var lookups = MeterMutationLookupModel.Create(CreateMasterData(), false);

        return new MeterMutationStateModel
        {
            Scope = new MeterScopeModel
            {
                PropertyId = 10,
                PropertyPublicId = Guid.NewGuid(),
                UnitId = 20,
                RoomPublicId = roomId
            },
            Electric = new MeterUtilityMutationStateModel
            {
                LineTypeId = lookups.ElectricLineType.Id,
                EffectivePolicy = new MeterPolicyModel { Id = 40, Amount = 3_500 },
                PreviousConfirmedValue = electricPreviousConfirmed
            },
            Water = new MeterUtilityMutationStateModel
            {
                LineTypeId = lookups.WaterLineType.Id,
                EffectivePolicy = new MeterPolicyModel { Id = 41, Amount = 25_000 },
                PreviousConfirmedValue = waterPreviousConfirmed
            }
        };
    }

    private static (
        RoomMeterService Sut,
        Mock<IInvoiceUtilityRepository> InvoiceRepository,
        Mock<IObjectStorageService> ObjectStorage,
        Mock<IDistributedLockService> DistributedLock) CreateSut(
        MeterMutationStateModel state,
        Invoice invoice = null)
    {
        var meterRepository = new Mock<IMeterRepository>();
        meterRepository
            .Setup(repository => repository.GetScopeAsync(
                state.Scope.RoomPublicId,
                30,
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(state.Scope);
        meterRepository
            .Setup(repository => repository.GetMutationStateAsync(
                It.IsAny<MeterMutationStateRequestModel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);
        meterRepository
            .Setup(repository => repository.AddAsync(It.IsAny<Meter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Meter meter, CancellationToken _) => meter);
        meterRepository
            .Setup(repository => repository.GetPeriodRowsAsync(
                It.IsAny<MeterPeriodQueryParametersModel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        meterRepository
            .Setup(repository => repository.GetEvidenceAsync(
                It.IsAny<IReadOnlyCollection<long>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var invoiceRepository = new Mock<IInvoiceUtilityRepository>();
        invoiceRepository
            .Setup(repository => repository.GetInvoiceGraphAsync(
                state.Scope.UnitId,
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);
        invoiceRepository
            .Setup(repository => repository.GetInvoiceImpactAsync(
                state.Scope.UnitId,
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((MeterInvoiceImpactRowModel)null);

        var masterDataService = new Mock<IMasterDataService>();
        masterDataService
            .Setup(service => service.GetValuesAsync(
                It.IsAny<IReadOnlyCollection<MasterDataKeyModel>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateMasterData());

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork
            .Setup(value => value.ExecuteInTransactionAsync(
                It.IsAny<Func<CancellationToken, Task>>(),
                It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task> action, CancellationToken token) => action(token));
        unitOfWork
            .Setup(value => value.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var distributedLock = new Mock<IDistributedLockService>();
        distributedLock
            .Setup(service => service.TryAcquireAsync(
                It.IsAny<string>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<IAsyncDisposable>());

        var objectStorage = new Mock<IObjectStorageService>();
        var sut = new RoomMeterService(
            new MeterRepositoryDependencies(meterRepository.Object, invoiceRepository.Object),
            masterDataService.Object,
            unitOfWork.Object,
            distributedLock.Object,
            objectStorage.Object,
            OptionsFactory.Create(new R2StorageOptions()),
            Mock.Of<ILogger<RoomMeterService>>());

        return (sut, invoiceRepository, objectStorage, distributedLock);
    }

    private static IReadOnlyDictionary<MasterDataKeyModel, MasterDataValueModel> CreateMasterData()
    {
        long id = 1;

        return MeterMutationLookupModel.GetRequiredKeys(false)
            .ToDictionary(
                key => key,
                key => new MasterDataValueModel
                {
                    Id = id++,
                    Type = key.Type,
                    Code = key.Code,
                    Name = key.Code
                });
    }
}
