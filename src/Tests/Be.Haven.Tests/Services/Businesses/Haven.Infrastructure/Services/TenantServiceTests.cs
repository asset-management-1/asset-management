using Be.Haven.Core.Interfaces.Repositories;
using Haven.Application.Dtos.Tenants.Detail;
using Haven.Application.Interfaces.Repositories;
using Haven.Application.Interfaces.Services;
using Haven.Application.Mappings.Tenants;
using Haven.Application.Models.MasterData;
using Haven.Application.Models.Parties;
using Haven.Application.Models.Rooms.QueryParameters;
using Haven.Application.Models.Tenants.Common;
using Haven.Application.Models.Tenants.Create;
using Haven.Application.Models.Tenants.Join;
using Haven.Application.Models.Tenants.QueryParameters;
using Haven.Application.Models.Tenants.Rows;
using Haven.Domain.Entities;
using Haven.Domain.Enums;
using Haven.Infrastructure.Services.Tenants;
using static Haven.Application.Constants.ApplicationConstants;

namespace Be.Haven.Tests.Services.Businesses.Haven.Infrastructure.Services;

public sealed class TenantServiceTests
{
    private const string TOKEN = "join-token";

    [Fact]
    public async Task ConfirmTenantJoinAsync_Should_CreatePendingOccupancy_AndConsumeToken()
    {
        var fixture = BuildFixture();
        Occupancy capturedOccupancy = null;
        Contract capturedContract = null;
        fixture.TenantRepository
            .Setup(repository => repository.HasOccupancyAsync(
                fixture.Room.UnitId,
                fixture.TenantParty.Id,
                It.Is<IReadOnlyCollection<long>>(ids => ids.Contains(6) && ids.Contains(7)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        fixture.TenantRepository
            .Setup(repository => repository.CountPrimaryOccupanciesAsync(
                fixture.Room.UnitId,
                It.Is<IReadOnlyCollection<long>>(ids => ids.Contains(6) && ids.Contains(7)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        fixture.TenantRepository
            .Setup(repository => repository.AddTenantOccupancyAsync(
                It.IsAny<Occupancy>(),
                It.IsAny<Contract>(),
                It.IsAny<CancellationToken>()))
            .Callback<Occupancy, Contract, CancellationToken>((occupancy, contract, _) =>
            {
                capturedOccupancy = occupancy;
                capturedContract = contract;
            })
            .Returns(Task.CompletedTask);

        var result = await fixture.Service.ConfirmTenantJoinAsync(BuildRequest(fixture.TenantParty.PublicId));

        capturedOccupancy.Should().NotBeNull();
        capturedOccupancy.StatusId.Should().Be(6);
        capturedOccupancy.ContractId.Should().BeNull();
        capturedContract.Should().BeNull();
        result.Id.Should().Be(capturedOccupancy.PublicId);
        result.Room.Id.Should().Be(fixture.Room.RoomPublicId);
        result.ContractStartDate.Should().BeNull();
        fixture.Cache.Verify(
            cache => cache.GetAsync<TenantJoinPayloadModel>(
                $"{TENANT_JOIN_CACHE_KEY_PREFIX}{TOKEN}",
                It.IsAny<CancellationToken>()),
            Times.Exactly(3));
        fixture.Cache.Verify(
            cache => cache.RemoveAsync(
                $"{TENANT_JOIN_CACHE_KEY_PREFIX}{TOKEN}",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ConfirmTenantJoinAsync_Should_Reject_WhenRoomLockCannotBeAcquired()
    {
        var fixture = BuildFixture(acquireLock: false);

        var action = () => fixture.Service.ConfirmTenantJoinAsync(BuildRequest(fixture.TenantParty.PublicId));

        await action.Should().ThrowAsync<ApiException>();
        fixture.TenantRepository.Verify(
            repository => repository.AddTenantOccupancyAsync(
                It.IsAny<Occupancy>(),
                It.IsAny<Contract>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ConfirmTenantJoinAsync_Should_RejectDuplicatePendingOrActiveOccupancy()
    {
        var fixture = BuildFixture();
        fixture.TenantRepository
            .Setup(repository => repository.HasOccupancyAsync(
                fixture.Room.UnitId,
                fixture.TenantParty.Id,
                It.IsAny<IReadOnlyCollection<long>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var action = () => fixture.Service.ConfirmTenantJoinAsync(BuildRequest(fixture.TenantParty.PublicId));

        await action.Should().ThrowAsync<ApiException>();
        fixture.TenantRepository.Verify(
            repository => repository.AddTenantOccupancyAsync(
                It.IsAny<Occupancy>(),
                It.IsAny<Contract>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        fixture.Cache.Verify(
            cache => cache.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ConfirmTenantJoinAsync_Should_Reject_WhenTokenDisappearsInsideRoomLock()
    {
        var fixture = BuildFixture();
        fixture.Cache.SetupSequence(cache => cache.GetAsync<TenantJoinPayloadModel>(
                $"{TENANT_JOIN_CACHE_KEY_PREFIX}{TOKEN}",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(fixture.Payload)
            .ReturnsAsync((TenantJoinPayloadModel)null);

        var action = () => fixture.Service.ConfirmTenantJoinAsync(BuildRequest(fixture.TenantParty.PublicId));

        await action.Should().ThrowAsync<ApiException>();
        fixture.TenantRepository.Verify(
            repository => repository.AddTenantOccupancyAsync(
                It.IsAny<Occupancy>(),
                It.IsAny<Contract>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ConfirmTenantJoinAsync_Should_CountPendingAndActivePrimaryOccupancies_ForSharedBedCapacity()
    {
        var fixture = BuildFixture();
        fixture.Room.RentalModeId = 8;
        fixture.Room.BedCount = 1;
        fixture.TenantRepository
            .Setup(repository => repository.HasOccupancyAsync(
                fixture.Room.UnitId,
                fixture.TenantParty.Id,
                It.IsAny<IReadOnlyCollection<long>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        fixture.TenantRepository
            .Setup(repository => repository.CountPrimaryOccupanciesAsync(
                fixture.Room.UnitId,
                It.Is<IReadOnlyCollection<long>>(ids => ids.Contains(6) && ids.Contains(7)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var action = () => fixture.Service.ConfirmTenantJoinAsync(BuildRequest(fixture.TenantParty.PublicId));

        await action.Should().ThrowAsync<ApiException>();
        fixture.TenantRepository.Verify(
            repository => repository.AddTenantOccupancyAsync(
                It.IsAny<Occupancy>(),
                It.IsAny<Contract>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ConfirmTenantJoinAsync_Should_RestoreOnlyRemainingTokenLifetime_WhenTransactionFails()
    {
        var fixture = BuildFixture();
        fixture.Payload.ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(2);
        fixture.TenantRepository
            .Setup(repository => repository.HasOccupancyAsync(
                fixture.Room.UnitId,
                fixture.TenantParty.Id,
                It.IsAny<IReadOnlyCollection<long>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        fixture.TenantRepository
            .Setup(repository => repository.CountPrimaryOccupanciesAsync(
                fixture.Room.UnitId,
                It.IsAny<IReadOnlyCollection<long>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        fixture.TenantRepository
            .Setup(repository => repository.AddTenantOccupancyAsync(
                It.IsAny<Occupancy>(),
                It.IsAny<Contract>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Mutation failed."));

        var action = () => fixture.Service.ConfirmTenantJoinAsync(BuildRequest(fixture.TenantParty.PublicId));

        await action.Should().ThrowAsync<InvalidOperationException>();
        fixture.Cache.Verify(
            cache => cache.SetAbsoluteAsync(
                $"{TENANT_JOIN_CACHE_KEY_PREFIX}{TOKEN}",
                fixture.Payload,
                It.Is<TimeSpan>(ttl => ttl > TimeSpan.Zero && ttl <= TimeSpan.FromMinutes(2)),
                CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task ConfirmTenantJoinAsync_Should_RestoreToken_WhenCacheRemoveFailsAfterConsumptionStarts()
    {
        var fixture = BuildFixture();
        fixture.Payload.ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(2);
        fixture.TenantRepository
            .Setup(repository => repository.HasOccupancyAsync(
                fixture.Room.UnitId,
                fixture.TenantParty.Id,
                It.IsAny<IReadOnlyCollection<long>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        fixture.TenantRepository
            .Setup(repository => repository.CountPrimaryOccupanciesAsync(
                fixture.Room.UnitId,
                It.IsAny<IReadOnlyCollection<long>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        fixture.Cache
            .Setup(cache => cache.RemoveAsync(
                $"{TENANT_JOIN_CACHE_KEY_PREFIX}{TOKEN}",
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Cache remove failed."));

        var action = () => fixture.Service.ConfirmTenantJoinAsync(BuildRequest(fixture.TenantParty.PublicId));

        await action.Should().ThrowAsync<InvalidOperationException>();
        fixture.Cache.Verify(
            cache => cache.SetAbsoluteAsync(
                $"{TENANT_JOIN_CACHE_KEY_PREFIX}{TOKEN}",
                fixture.Payload,
                It.Is<TimeSpan>(ttl => ttl > TimeSpan.Zero && ttl <= TimeSpan.FromMinutes(2)),
                CancellationToken.None),
            Times.Once);
        fixture.TenantRepository.Verify(
            repository => repository.AddTenantOccupancyAsync(
                It.IsAny<Occupancy>(),
                It.IsAny<Contract>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ConfirmTenantJoinAsync_Should_NotRestoreToken_WhenFailureHappensAfterMutationCallback()
    {
        var fixture = BuildFixture();
        fixture.Payload.ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(2);
        fixture.TenantRepository
            .Setup(repository => repository.HasOccupancyAsync(
                fixture.Room.UnitId,
                fixture.TenantParty.Id,
                It.IsAny<IReadOnlyCollection<long>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        fixture.TenantRepository
            .Setup(repository => repository.CountPrimaryOccupanciesAsync(
                fixture.Room.UnitId,
                It.IsAny<IReadOnlyCollection<long>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        fixture.TenantRepository
            .Setup(repository => repository.AddTenantOccupancyAsync(
                It.IsAny<Occupancy>(),
                It.IsAny<Contract>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        fixture.UnitOfWork
            .Setup(work => work.ExecuteInTransactionAsync(
                It.IsAny<Func<CancellationToken, Task<TenantDetailResponseDto>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task<TenantDetailResponseDto>>, CancellationToken>(async (action, token) =>
            {
                await action(token);
                throw new InvalidOperationException("Post-callback failure.");
            });

        var action = () => fixture.Service.ConfirmTenantJoinAsync(BuildRequest(fixture.TenantParty.PublicId));

        await action.Should().ThrowAsync<InvalidOperationException>();
        fixture.Cache.Verify(
            cache => cache.SetAbsoluteAsync(
                It.IsAny<string>(),
                It.IsAny<TenantJoinPayloadModel>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateTenantAsync_Should_RejectDuplicatePendingOrActiveOccupancy_InsideRoomLock()
    {
        var fixture = BuildFixture();
        var room = new Unit
        {
            Id = fixture.Room.UnitId,
            PublicId = fixture.Room.RoomPublicId,
            PropertyId = fixture.Room.PropertyId,
            RentalModeId = fixture.Room.RentalModeId,
            Property = new Property { Id = fixture.Room.PropertyId }
        };
        fixture.UnitRepository
            .Setup(repository => repository.GetRoomGraphAsync(
                It.IsAny<RoomScopedQueryParametersModel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);
        fixture.TenantRepository
            .Setup(repository => repository.GetTenantPartyAsync(
                fixture.TenantParty.PublicId,
                1,
                2,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(fixture.TenantParty);
        fixture.TenantRepository
            .Setup(repository => repository.HasOccupancyAsync(
                room.Id,
                fixture.TenantParty.Id,
                It.IsAny<IReadOnlyCollection<long>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var request = new TenantCreateRequestModel
        {
            RoomId = room.PublicId,
            TenantId = fixture.TenantParty.PublicId,
            RoleCode = TENANT_ROLE_OCCUPANT,
            CurrentParty = new CurrentPartyContextModel
            {
                PartyId = 99,
                PartyPublicId = fixture.Payload.LandlordPartyPublicId,
                PartyTypeCode = MASTER_CODE_PARTY_TYPE_LANDLORD,
                StatusCode = MASTER_CODE_ACTIVE
            }
        };

        var action = () => fixture.Service.CreateTenantAsync(request);

        await action.Should().ThrowAsync<ApiException>();
        fixture.TenantRepository.Verify(
            repository => repository.AddTenantOccupancyAsync(
                It.IsAny<Occupancy>(),
                It.IsAny<Contract>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateTenantAsync_Should_AssignBackendPartyFields_ForManualProfile()
    {
        var fixture = BuildFixture();
        var room = new Unit
        {
            Id = fixture.Room.UnitId,
            PublicId = fixture.Room.RoomPublicId,
            PropertyId = fixture.Room.PropertyId,
            RentalModeId = fixture.Room.RentalModeId,
            Property = new Property { Id = fixture.Room.PropertyId }
        };
        Party capturedParty = null;
        fixture.UnitRepository
            .Setup(repository => repository.GetRoomGraphAsync(It.IsAny<RoomScopedQueryParametersModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);
        fixture.TenantRepository
            .Setup(repository => repository.HasOccupancyAsync(
                room.Id,
                It.IsAny<long>(),
                It.IsAny<IReadOnlyCollection<long>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        fixture.TenantRepository
            .Setup(repository => repository.AddTenantOccupancyAsync(
                It.IsAny<Occupancy>(),
                It.IsAny<Contract>(),
                It.IsAny<CancellationToken>()))
            .Callback<Occupancy, Contract, CancellationToken>((occupancy, _, _) => capturedParty = occupancy.Party)
            .Returns(Task.CompletedTask);
        var request = new TenantCreateRequestModel
        {
            RoomId = room.PublicId,
            RoleCode = TENANT_ROLE_OCCUPANT,
            TenantProfile = new TenantProfileModel
            {
                FullName = "Nguyễn Văn B",
                Phone = "0909999999",
                Email = "tenant-b@haven.test"
            },
            CurrentParty = new CurrentPartyContextModel
            {
                PartyId = 99,
                PartyPublicId = fixture.Payload.LandlordPartyPublicId,
                PartyTypeCode = MASTER_CODE_PARTY_TYPE_LANDLORD,
                StatusCode = MASTER_CODE_ACTIVE
            }
        };

        await fixture.Service.CreateTenantAsync(request);

        capturedParty.Should().NotBeNull();
        capturedParty.PublicId.Should().NotBeEmpty();
        capturedParty.PartyTypeId.Should().Be(1);
        capturedParty.StatusId.Should().Be(2);
        capturedParty.DisplayName.Should().Be(request.TenantProfile.FullName);
        capturedParty.PrimaryPhone.Should().Be(request.TenantProfile.Phone);
        capturedParty.PrimaryEmail.Should().Be(request.TenantProfile.Email);
    }

    private static TenantServiceFixture BuildFixture(bool acquireLock = true)
    {
        var tenantRepository = new Mock<ITenantRepository>();
        var unitRepository = new Mock<IUnitRepository>();
        var masterDataService = new Mock<IMasterDataService>();
        var cache = new Mock<ICachingService>();
        var distributedLock = new Mock<IDistributedLockService>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var payload = new TenantJoinPayloadModel
        {
            RoomPublicId = Guid.NewGuid(),
            LandlordPartyPublicId = Guid.NewGuid(),
            RoleCode = TENANT_ROLE_PRIMARY,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(TENANT_JOIN_TOKEN_EXPIRY_MINUTES)
        };
        var room = new TenantJoinRoomRowModel
        {
            PropertyId = 10,
            PropertyPublicId = Guid.NewGuid(),
            PropertyName = "Tòa A",
            UnitId = 20,
            RoomPublicId = payload.RoomPublicId,
            RoomCode = "ROOM_1234567890",
            RoomName = "Phòng 101",
            RentalModeId = 12,
            BedCount = null
        };
        var tenantParty = new Party
        {
            Id = 30,
            PublicId = Guid.NewGuid(),
            DisplayName = "Nguyễn Văn A"
        };

        TenantJoinPayloadModel cachedPayload = payload;
        cache.Setup(service => service.GetAsync<TenantJoinPayloadModel>(
                $"{TENANT_JOIN_CACHE_KEY_PREFIX}{TOKEN}",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => cachedPayload);
        cache.Setup(service => service.RemoveAsync(
                $"{TENANT_JOIN_CACHE_KEY_PREFIX}{TOKEN}",
                It.IsAny<CancellationToken>()))
            .Callback(() => cachedPayload = null)
            .Returns(Task.CompletedTask);
        distributedLock.Setup(service => service.TryAcquireAsync(
                $"{TENANT_JOIN_ROOM_LOCK_KEY_PREFIX}{payload.RoomPublicId:N}",
                TimeSpan.FromSeconds(TENANT_JOIN_ROOM_LOCK_SECONDS),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(acquireLock ? Mock.Of<IAsyncDisposable>() : null);
        masterDataService.Setup(service => service.GetValuesAsync(
                It.IsAny<IReadOnlyCollection<MasterDataKeyModel>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildMasterData());
        unitRepository.Setup(repository => repository.GetTenantJoinRoomAsync(
                It.IsAny<TenantJoinRoomQueryParametersModel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);
        tenantRepository.Setup(repository => repository.GetTenantPartyAsync(
                tenantParty.PublicId,
                1,
                2,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenantParty);
        unitOfWork.Setup(work => work.ExecuteInTransactionAsync(
                It.IsAny<Func<CancellationToken, Task>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task>, CancellationToken>((action, token) => action(token));
        unitOfWork.Setup(work => work.ExecuteInTransactionAsync(
                It.IsAny<Func<CancellationToken, Task<TenantDetailResponseDto>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task<TenantDetailResponseDto>>, CancellationToken>(
                (action, token) => action(token));

        var service = new TenantService(
            tenantRepository.Object,
            unitRepository.Object,
            masterDataService.Object,
            cache.Object,
            distributedLock.Object,
            unitOfWork.Object,
            Mock.Of<ILogger<TenantService>>());

        return new TenantServiceFixture(
            service,
            tenantRepository,
            unitRepository,
            cache,
            unitOfWork,
            room,
            tenantParty,
            payload);
    }

    private static TenantJoinConfirmRequestModel BuildRequest(Guid tenantPartyPublicId)
    {
        return new TenantJoinConfirmRequestModel
        {
            Token = TOKEN,
            CurrentParty = new CurrentPartyContextModel
            {
                PartyPublicId = tenantPartyPublicId,
                PartyTypeCode = MASTER_CODE_PARTY_TYPE_TENANT,
                StatusCode = MASTER_CODE_ACTIVE
            }
        };
    }

    private static IReadOnlyDictionary<MasterDataKeyModel, MasterDataValueModel> BuildMasterData()
    {
        var values = new[]
        {
            Value(1, MasterDataTypeEnum.PartyType, MASTER_CODE_PARTY_TYPE_TENANT),
            Value(2, MasterDataTypeEnum.PartyStatus, MASTER_CODE_ACTIVE),
            Value(3, MasterDataTypeEnum.ContractType, MASTER_CODE_CONTRACT_TYPE_RENTAL),
            Value(4, MasterDataTypeEnum.ContractSource, MASTER_CODE_CONTRACT_SOURCE_OFFLINE_UPLOAD),
            Value(5, MasterDataTypeEnum.ContractStatus, MASTER_CODE_CONTRACT_STATUS_ACTIVE),
            Value(6, MasterDataTypeEnum.OccupancyStatus, MASTER_CODE_OCCUPANCY_STATUS_PENDING),
            Value(7, MasterDataTypeEnum.OccupancyStatus, MASTER_CODE_OCCUPANCY_STATUS_ACTIVE),
            Value(9, MasterDataTypeEnum.OccupancyStatus, MASTER_CODE_OCCUPANCY_STATUS_MOVED_OUT),
            Value(8, MasterDataTypeEnum.UnitRentalMode, MASTER_CODE_RENTAL_MODE_SHARED_BED),
            Value(10, MasterDataTypeEnum.UnitStatus, MASTER_CODE_UNIT_STATUS_OCCUPIED),
            Value(11, MasterDataTypeEnum.UnitStatus, MASTER_CODE_UNIT_STATUS_AVAILABLE)
        };

        return values.ToDictionary(
            value => new MasterDataKeyModel(Enum.Parse<MasterDataTypeEnum>(value.Type), value.Code),
            value => value);
    }

    private static MasterDataValueModel Value(long id, MasterDataTypeEnum type, string code)
    {
        return new MasterDataValueModel
        {
            Id = id,
            Type = type.ToString(),
            Code = code,
            Name = code
        };
    }

    private sealed record TenantServiceFixture(
        TenantService Service,
        Mock<ITenantRepository> TenantRepository,
        Mock<IUnitRepository> UnitRepository,
        Mock<ICachingService> Cache,
        Mock<IUnitOfWork> UnitOfWork,
        TenantJoinRoomRowModel Room,
        Party TenantParty,
        TenantJoinPayloadModel Payload);
}
