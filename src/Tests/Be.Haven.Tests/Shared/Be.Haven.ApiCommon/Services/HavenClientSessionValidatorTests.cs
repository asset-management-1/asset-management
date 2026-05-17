namespace Be.Haven.Tests.Shared.Be.Haven.ApiCommon.Services;

public sealed class HavenClientSessionValidatorTests
{
    [Fact]
    public async Task IsSessionActiveAsync_Should_ReturnTrue_When_DatabaseFindsActiveSession()
    {
        // Arrange
        var userPublicId = Guid.NewGuid();
        var sessionPublicId = Guid.NewGuid();
        var dapperService = new Mock<IDapperService>();
        dapperService
            .Setup(x => x.QueryFirstOrDefaultAsync<ClientSessionValidationReadModel>(
                HavenClientSessionConstants.GET_ACTIVE_SESSION_BY_USER_AND_PUBLIC_ID_QUERY,
                It.IsAny<object>(),
                It.IsAny<DapperCommandOptions>()))
            .ReturnsAsync(new ClientSessionValidationReadModel { SessionPublicId = sessionPublicId });
        var sut = new HavenClientSessionValidator(dapperService.Object, Mock.Of<ILogger<HavenClientSessionValidator>>());

        // Act
        var result = await sut.IsSessionActiveAsync(userPublicId, sessionPublicId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsSessionActiveAsync_Should_ReturnFalse_When_DatabaseFindsNoSession()
    {
        // Arrange
        var dapperService = new Mock<IDapperService>();
        dapperService
            .Setup(x => x.QueryFirstOrDefaultAsync<ClientSessionValidationReadModel>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<DapperCommandOptions>()))
            .ReturnsAsync((ClientSessionValidationReadModel)null);
        var sut = new HavenClientSessionValidator(dapperService.Object, Mock.Of<ILogger<HavenClientSessionValidator>>());

        // Act
        var result = await sut.IsSessionActiveAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsSessionActiveAsync_Should_ThrowServiceUnavailable_When_DatabaseLookupFails()
    {
        // Arrange
        var dapperService = new Mock<IDapperService>();
        dapperService
            .Setup(x => x.QueryFirstOrDefaultAsync<ClientSessionValidationReadModel>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<DapperCommandOptions>()))
            .ThrowsAsync(new InvalidOperationException("db offline"));
        var sut = new HavenClientSessionValidator(dapperService.Object, Mock.Of<ILogger<HavenClientSessionValidator>>());

        // Act
        var action = () => sut.IsSessionActiveAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        var exception = await action.Should().ThrowAsync<HttpStatusCodeException>();
        exception.Subject.Single().StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
        exception.Subject.Single().ErrorCode.Should().Be(SERVICE_UNAVAILABLE);
    }
}
