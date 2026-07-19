using Authentication.Application.Interfaces.Repositories;
using Authentication.Domain.Entities;
using Authentication.Infrastructure.Context;
using Authentication.Infrastructure.Dependencies;
using Authentication.Infrastructure.Helpers;
using Authentication.Infrastructure.Options.Authentications;
using Authentication.Infrastructure.Repositories;
using Authentication.Infrastructure.Services;
using Be.Haven.Core.Models.ClientDevices;
using Microsoft.AspNetCore.Identity;
using Testcontainers.PostgreSql;
using AuthenticationService = Authentication.Infrastructure.Services.AuthenticationService;

namespace Be.Haven.Tests.Services.Authentications.Authentication.Infrastructure.Services;

public sealed class AuthenticationConcurrencyTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine").Build();
    private DbContextOptions<AuthenticationDbContext> _dbOptions;
    private SeededIdentity _identity;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        _dbOptions = new DbContextOptionsBuilder<AuthenticationDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;
        await using var dbContext = new AuthenticationDbContext(_dbOptions);
        await dbContext.Database.EnsureCreatedAsync();
        _identity = await SeedIdentityAsync(dbContext);
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task RefreshTokenAsync_Should_ReturnOneSuccessAndOneConflict_WhenSameTokenIsUsedConcurrently()
    {
        var results = await Task.WhenAll(
            RefreshAsync(_identity.DeviceAToken, _identity.DeviceAId),
            RefreshAsync(_identity.DeviceAToken, _identity.DeviceAId));

        results.Should().BeEquivalentTo([StatusCodes.Status200OK, StatusCodes.Status409Conflict]);
        await using var verificationContext = new AuthenticationDbContext(_dbOptions);
        var session = await verificationContext.RefreshTokens.SingleAsync(x => x.DeviceId == _identity.DeviceAId);
        session.RevokedAt.Should().BeNull();
        session.PreviousTokenHash.Should().Be(AuthSessionHelper.HashRefreshToken(_identity.DeviceAToken));
    }

    [Fact]
    public async Task RefreshTokenAsync_Should_AllowBothDevices_WhenTheirSessionsRefreshConcurrently()
    {
        var results = await Task.WhenAll(
            RefreshAsync(_identity.DeviceAToken, _identity.DeviceAId),
            RefreshAsync(_identity.DeviceBToken, _identity.DeviceBId));

        results.Should().OnlyContain(status => status == StatusCodes.Status200OK);
    }

    [Fact]
    public async Task ChangePasswordAsync_Should_AllowOnlyFirstConcurrentRequestAfterUserLock()
    {
        var results = await Task.WhenAll(
            ChangePasswordAsync(_identity.DeviceAId),
            ChangePasswordAsync(_identity.DeviceAId));

        results.Count(status => status == StatusCodes.Status200OK).Should().Be(1);
        results.Count(status => status == StatusCodes.Status400BadRequest).Should().Be(1);
    }

    /// <summary>
    /// Ensures forgot-password mutation can lock and load the identity by normalized email on PostgreSQL.
    /// </summary>
    [Fact]
    public async Task GetPasswordIdentityByEmailForUpdateAsync_Should_LoadSeededUser_WhenEmailMatches()
    {
        // Arrange
        await using var dbContext = new AuthenticationDbContext(_dbOptions);
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        var sut = new UserRepository(
            dbContext,
            CreateDapperService(),
            Mock.Of<IJsonSerializerService>());

        // Act
        var result = await sut.GetPasswordIdentityByEmailForUpdateAsync("concurrency@example.com");

        // Assert
        result.Should().NotBeNull();
        result.PublicId.Should().Be(_identity.UserPublicId);
    }

    [Fact]
    public async Task GetPasswordIdentityByEmailForUpdateAsync_Should_RequireActiveTransaction_WhenCalledForRowLock()
    {
        // Arrange
        await using var dbContext = new AuthenticationDbContext(_dbOptions);
        var sut = new UserRepository(
            dbContext,
            CreateDapperService(),
            Mock.Of<IJsonSerializerService>());

        // Act
        var action = () => sut.GetPasswordIdentityByEmailForUpdateAsync("concurrency@example.com");

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetForRefreshForUpdateAsync_Should_RequireActiveTransaction_WhenCalledForRowLock()
    {
        // Arrange
        await using var dbContext = new AuthenticationDbContext(_dbOptions);
        var sut = new RefreshTokenRepository(dbContext, CreateDapperService());

        // Act
        var action = () => sut.GetForRefreshForUpdateAsync(
            AuthSessionHelper.HashRefreshToken(_identity.DeviceAToken));

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    private async Task<int> RefreshAsync(
        string rawToken,
        string deviceId)
    {
        await using var dbContext = new AuthenticationDbContext(_dbOptions);
        var sut = CreateSut(dbContext, deviceId);
        try
        {
            await sut.RefreshTokenAsync(
                new global::Authentication.Application.Dtos.Authentications.Sessions.RefreshTokenRequestDto
                {
                    RefreshToken = rawToken
                });
            return StatusCodes.Status200OK;
        }
        catch (ApiException exception)
        {
            return exception.StatusCode;
        }
    }

    private async Task<int> ChangePasswordAsync(string deviceId)
    {
        await using var dbContext = new AuthenticationDbContext(_dbOptions);
        var sut = CreateSut(dbContext, deviceId);
        try
        {
            await sut.ChangePasswordAsync(
                _identity.UserPublicId,
                _identity.DeviceASessionId,
                new global::Authentication.Application.Dtos.Authentications.Passwords.ChangePasswordRequestDto
                {
                    CurrentPassword = "Old@123a",
                    NewPassword = "New@123a"
                });
            return StatusCodes.Status200OK;
        }
        catch (ApiException exception)
        {
            return exception.StatusCode;
        }
    }

    private AuthenticationService CreateSut(
        AuthenticationDbContext dbContext,
        string deviceId)
    {
        var dapperService = CreateDapperService();
        var refreshTokenRepository = new RefreshTokenRepository(dbContext, dapperService);
        var userRepository = new UserRepository(
            dbContext,
            dapperService,
            Mock.Of<IJsonSerializerService>());
        var userPartyRepository = new Mock<IUserPartyRepository>();
        userPartyRepository.Setup(x => x.ResolveSessionPartyIdAsync(
                _identity.UserId,
                It.IsAny<long?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_identity.PartyId);
        var repositories = new AuthenticationRepositoryDependencies(
            userRepository,
            refreshTokenRepository,
            Mock.Of<IMasterDataValueRepository>(),
            Mock.Of<IPartyRepository>(),
            userPartyRepository.Object,
            Mock.Of<IExternalLoginRepository>());
        var unitOfWork = new UnitOfWork<AuthenticationDbContext>(
            dbContext,
            Mock.Of<IAuthService>(),
            Mock.Of<ILogger<UnitOfWork<AuthenticationDbContext>>>());
        var cache = new Mock<ICachingService>();
        cache.Setup(x => x.SetAbsoluteAsync(
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var authOptions = OptionsFactory.Create(new AuthOptions
        {
            Issuer = "Authentication",
            Audiences = ["Haven.Mobile"],
            SecretKey = new string('s', 64),
            AccessTokenMinutes = 15,
            RefreshTokenDays = 30
        });
        var support = new AuthenticationServiceSupportDependencies(
            unitOfWork,
            Mock.Of<IEmailService>(),
            new PasswordHasher<User>(),
            cache.Object,
            authOptions,
            OptionsFactory.Create(new EmailOptions()));
        var deviceAccessor = new Mock<IClientDeviceContextAccessor>();
        deviceAccessor.Setup(x => x.GetCurrent()).Returns(new ClientDeviceContextModel
        {
            DeviceId = deviceId,
            DeviceName = deviceId,
            DeviceType = "mobile"
        });
        return new AuthenticationService(
            repositories,
            support,
            deviceAccessor.Object,
            Mock.Of<ILogger<AuthenticationService>>());
    }

    private static IDapperService CreateDapperService()
    {
        // Locking queries borrow the EF transaction, so this factory must never be used by the test flow.
        return new DapperService(
            Mock.Of<IDbConnectionFactory>(),
            Options.Create(new GcpOptions
            {
                DatabaseSettings = new DatabaseOptions
                {
                    Provider = SqlProvider.PostgreSql
                }
            }));
    }

    private static async Task<SeededIdentity> SeedIdentityAsync(AuthenticationDbContext dbContext)
    {
        var masterType = new MasterDataType { Code = "TEST", Name = "Test" };
        var active = new MasterDataValue { MasterDataType = masterType, Code = "ACTIVE", Name = "Active", IsActive = true };
        var tenant = new MasterDataValue { MasterDataType = masterType, Code = "TENANT", Name = "Tenant", IsActive = true };
        var user = new User
        {
            PublicId = Guid.NewGuid(),
            UserName = "concurrency-user",
            Email = "concurrency@example.com",
            EmailConfirmed = true,
            FullName = "Concurrency User",
            Status = active
        };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, "Old@123a");
        var party = new Party
        {
            DisplayName = user.FullName,
            PrimaryEmail = user.Email,
            PartyType = tenant,
            Status = active
        };
        var userParty = new UserParty { User = user, Party = party };
        var deviceAToken = "device-a-refresh";
        var deviceBToken = "device-b-refresh";
        var deviceASessionId = Guid.NewGuid();
        dbContext.AddRange(
            masterType,
            active,
            tenant,
            user,
            party,
            userParty,
            new RefreshToken
            {
                User = user,
                CurrentUserParty = userParty,
                CurrentPartyId = party.Id,
                SessionPublicId = deviceASessionId,
                TokenHash = AuthSessionHelper.HashRefreshToken(deviceAToken),
                JwtId = Guid.NewGuid().ToString("N"),
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                DeviceId = "device-a"
            },
            new RefreshToken
            {
                User = user,
                CurrentUserParty = userParty,
                CurrentPartyId = party.Id,
                SessionPublicId = Guid.NewGuid(),
                TokenHash = AuthSessionHelper.HashRefreshToken(deviceBToken),
                JwtId = Guid.NewGuid().ToString("N"),
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                DeviceId = "device-b"
            });
        await dbContext.SaveChangesAsync();
        return new SeededIdentity(
            user.Id,
            user.PublicId,
            party.Id,
            deviceASessionId,
            "device-a",
            "device-b",
            deviceAToken,
            deviceBToken);
    }

    private sealed record SeededIdentity(
        long UserId,
        Guid UserPublicId,
        long PartyId,
        Guid DeviceASessionId,
        string DeviceAId,
        string DeviceBId,
        string DeviceAToken,
        string DeviceBToken);
}
