namespace Be.Haven.Tests.Shared.Be.Haven.ApiCommon.Services;

public sealed class HavenAuthResetValidatorTests
{
    [Fact]
    public async Task IsTokenValidAsync_Should_ReturnCachedDecision_When_AuthResetMarkerExistsInCache()
    {
        // Arrange
        var userPublicId = Guid.NewGuid();
        var issuedAtMs = 2000L;
        var cachingService = new Mock<ICachingService>();
        cachingService
            .Setup(x => x.GetAsync<long?>(
                TokenHelper.BuildAuthResetCacheKey(userPublicId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1000L);
        var dapperService = new Mock<IDapperService>();
        var sut = CreateSut(cachingService.Object, dapperService.Object);

        // Act
        var result = await sut.IsTokenValidAsync(userPublicId, issuedAtMs);

        // Assert
        result.Should().BeTrue();
        dapperService.Verify(
            x => x.QueryFirstOrDefaultAsync<AuthResetReadModel>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<DapperCommandOptions>()),
            Times.Never);
    }

    [Fact]
    public async Task IsTokenValidAsync_Should_ReturnFalse_When_CachedAuthResetIsAfterTokenIssue()
    {
        // Arrange
        var userPublicId = Guid.NewGuid();
        var cachingService = new Mock<ICachingService>();
        cachingService
            .Setup(x => x.GetAsync<long?>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2000L);
        var sut = CreateSut(cachingService.Object, Mock.Of<IDapperService>());

        // Act
        var result = await sut.IsTokenValidAsync(userPublicId, issuedAtMs: 1000L);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsTokenValidAsync_Should_LoadDatabaseAndSeedCache_When_CacheMisses()
    {
        // Arrange
        var userPublicId = Guid.NewGuid();
        var resetAt = new DateTime(2026, 5, 17, 1, 2, 3, DateTimeKind.Utc);
        var issuedAtMs = TokenHelper.ToAuthResetUnixMilliseconds(resetAt.AddMinutes(1));
        var cachingService = new Mock<ICachingService>();
        cachingService
            .Setup(x => x.GetAsync<long?>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((long?)null);
        var dapperService = new Mock<IDapperService>();
        dapperService
            .Setup(x => x.QueryFirstOrDefaultAsync<AuthResetReadModel>(
                HavenAuthResetConstants.GET_AUTH_RESET_AT_BY_PUBLIC_ID_QUERY,
                It.IsAny<object>(),
                It.IsAny<DapperCommandOptions>()))
            .ReturnsAsync(new AuthResetReadModel { AuthResetAt = resetAt });
        var sut = CreateSut(cachingService.Object, dapperService.Object);

        // Act
        var result = await sut.IsTokenValidAsync(userPublicId, issuedAtMs);

        // Assert
        result.Should().BeTrue();
        cachingService.Verify(
            x => x.SetAbsoluteAsync(
                TokenHelper.BuildAuthResetCacheKey(userPublicId),
                TokenHelper.ToAuthResetUnixMilliseconds(resetAt),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task IsTokenValidAsync_Should_LoadDatabase_When_CacheReadFails()
    {
        // Arrange
        var userPublicId = Guid.NewGuid();
        var cachingService = new Mock<ICachingService>();
        cachingService
            .Setup(x => x.GetAsync<long?>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("cache unavailable"));
        var dapperService = new Mock<IDapperService>();
        dapperService
            .Setup(x => x.QueryFirstOrDefaultAsync<AuthResetReadModel>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<DapperCommandOptions>()))
            .ReturnsAsync(new AuthResetReadModel());
        var sut = CreateSut(cachingService.Object, dapperService.Object);

        // Act
        var result = await sut.IsTokenValidAsync(userPublicId, issuedAtMs: 3000L);

        // Assert
        result.Should().BeTrue();
        dapperService.Verify(
            x => x.QueryFirstOrDefaultAsync<AuthResetReadModel>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<DapperCommandOptions>()),
            Times.Once);
    }

    [Fact]
    public async Task IsTokenValidAsync_Should_ReturnDatabaseDecision_When_CacheSeedFails()
    {
        // Arrange
        var userPublicId = Guid.NewGuid();
        var cachingService = new Mock<ICachingService>();
        cachingService
            .Setup(x => x.GetAsync<long?>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((long?)null);
        cachingService
            .Setup(x => x.SetAbsoluteAsync(
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("cache seed failed"));
        var dapperService = new Mock<IDapperService>();
        dapperService
            .Setup(x => x.QueryFirstOrDefaultAsync<AuthResetReadModel>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<DapperCommandOptions>()))
            .ReturnsAsync(new AuthResetReadModel());
        var sut = CreateSut(cachingService.Object, dapperService.Object);

        // Act
        var result = await sut.IsTokenValidAsync(userPublicId, issuedAtMs: 3000L);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsTokenValidAsync_Should_ReturnFalse_When_DatabaseUserIsMissing()
    {
        // Arrange
        var cachingService = new Mock<ICachingService>();
        cachingService
            .Setup(x => x.GetAsync<long?>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((long?)null);
        var dapperService = new Mock<IDapperService>();
        dapperService
            .Setup(x => x.QueryFirstOrDefaultAsync<AuthResetReadModel>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<DapperCommandOptions>()))
            .ReturnsAsync((AuthResetReadModel)null);
        var sut = CreateSut(cachingService.Object, dapperService.Object);

        // Act
        var result = await sut.IsTokenValidAsync(Guid.NewGuid(), issuedAtMs: 3000L);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsTokenValidAsync_Should_ThrowServiceUnavailable_When_DatabaseLookupFails()
    {
        // Arrange
        var cachingService = new Mock<ICachingService>();
        cachingService
            .Setup(x => x.GetAsync<long?>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((long?)null);
        var dapperService = new Mock<IDapperService>();
        dapperService
            .Setup(x => x.QueryFirstOrDefaultAsync<AuthResetReadModel>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<DapperCommandOptions>()))
            .ThrowsAsync(new InvalidOperationException("db offline"));
        var sut = CreateSut(cachingService.Object, dapperService.Object);

        // Act
        var action = () => sut.IsTokenValidAsync(Guid.NewGuid(), issuedAtMs: 3000L);

        // Assert
        var exception = await action.Should().ThrowAsync<HttpStatusCodeException>();
        exception.Subject.Single().StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
        exception.Subject.Single().ErrorCode.Should().Be(SERVICE_UNAVAILABLE);
    }

    private static HavenAuthResetValidator CreateSut(
        ICachingService cachingService,
        IDapperService dapperService)
    {
        return new HavenAuthResetValidator(
            cachingService,
            dapperService,
            Options.Create(new AuthenticationTokenValidationOptions { RefreshTokenDays = 7 }),
            Mock.Of<ILogger<HavenAuthResetValidator>>());
    }
}
