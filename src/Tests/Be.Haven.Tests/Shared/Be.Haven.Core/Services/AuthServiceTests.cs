namespace Be.Haven.Tests.Shared.Be.Haven.Core.Services;

public sealed class AuthServiceTests
{
    [Fact]
    public void SchemeAndRawToken_Should_ReadAuthorizationHeader_When_HeaderIsBearer()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers.Authorization = "Bearer access-token";
        var sut = CreateSut(context);

        // Act
        var scheme = sut.Scheme();
        var token = sut.RawToken();

        // Assert
        scheme.Should().Be("Bearer");
        token.Should().Be("access-token");
    }

    [Fact]
    public void TryGetAndGet_Should_ConvertClaimValues_When_ClaimsExist()
    {
        // Arrange
        var context = CreateHttpContext(new Claim("age", "42"));
        var sut = CreateSut(context);

        // Act
        var success = sut.TryGet<int>("age", out var result);
        var missing = sut.Get("missing", 99);

        // Assert
        success.Should().BeTrue();
        result.Should().Be(42);
        missing.Should().Be(99);
    }

    [Fact]
    public void Get_Should_ReturnDefaultValue_When_ClaimValueCannotBeConverted()
    {
        // Arrange
        var context = CreateHttpContext(new Claim("age", "not-a-number"));
        var sut = CreateSut(context);

        // Act
        var result = sut.Get("age", 99);

        // Assert
        result.Should().Be(99);
    }

    [Fact]
    public void GetAll_Should_ReturnEmptyCollection_When_HttpContextIsMissing()
    {
        // Arrange
        var sut = new AuthService(new HttpContextAccessor());

        // Act
        var result = sut.GetAll(ClaimTypes.Role);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void UserIdAndSessionId_Should_ReturnGuids_When_UserIsAuthenticated()
    {
        // Arrange
        var userPublicId = Guid.Parse("fc07838a-daad-46b8-ba8b-eab01715dd65");
        var sessionPublicId = Guid.Parse("14a552b6-af23-4f42-82ec-3e3fc7f07d7a");
        var context = CreateHttpContext(
            new Claim(ClaimTypes.NameIdentifier, userPublicId.ToString()),
            new Claim(TokenClaimTypes.SESSION_ID, sessionPublicId.ToString()));
        var sut = CreateSut(context);

        // Act
        var userId = sut.UserId();
        var sessionId = sut.SessionId();

        // Assert
        userId.Should().Be(userPublicId);
        sessionId.Should().Be(sessionPublicId);
    }

    [Fact]
    public void UserIdAndSessionId_Should_ReturnNull_When_UserIsAnonymous()
    {
        // Arrange
        var context = CreateHttpContext(authenticationType: null);
        var sut = CreateSut(context);

        // Act
        var userId = sut.UserId();
        var sessionId = sut.SessionId();

        // Assert
        userId.Should().BeNull();
        sessionId.Should().BeNull();
    }

    [Fact]
    public void AccountUserNameAndEmail_Should_UseFallbackClaims_When_PrimaryClaimsAreMissing()
    {
        // Arrange
        var context = CreateHttpContext(
            new Claim(TokenClaimTypes.ACCOUNT_ID, "account-1"),
            new Claim(TokenClaimTypes.NAME, "tenant.test"),
            new Claim(TokenClaimTypes.EMAIL, "tenant@test.com"));
        var sut = CreateSut(context);

        // Act
        var accountId = sut.AccountId();
        var userName = sut.UserName();
        var email = sut.Email();

        // Assert
        accountId.Should().Be("account-1");
        userName.Should().Be("tenant.test");
        email.Should().Be("tenant@test.com");
    }

    [Fact]
    public void RolesAndScopes_Should_ReturnDistinctValues_When_ClaimsContainDuplicates()
    {
        // Arrange
        var context = CreateHttpContext(
            new Claim(TokenClaimTypes.ROLES, "Admin Tenant"),
            new Claim(ClaimTypes.Role, "admin"),
            new Claim(TokenClaimTypes.SCOPE, "read write read"));
        var sut = CreateSut(context);

        // Act
        var roles = sut.Roles();
        var scopes = sut.Scopes();

        // Assert
        roles.Should().BeEquivalentTo(["Admin", "Tenant"]);
        sut.HasRole("admin").Should().BeTrue();
        scopes.Should().Equal("read", "write");
        sut.HasScope("write").Should().BeTrue();
    }

    [Fact]
    public void IssuerExpUnixAndNbfUnix_Should_ReadJwtMetadata_When_RawTokenIsJwt()
    {
        // Arrange
        var exp = new DateTimeOffset(2026, 5, 17, 12, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds();
        var nbf = new DateTimeOffset(2026, 5, 17, 11, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds();
        var token = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            issuer: "haven-issuer",
            claims:
            [
                new Claim("exp", exp.ToString(CultureInfo.InvariantCulture)),
                new Claim("nbf", nbf.ToString(CultureInfo.InvariantCulture))
            ]));
        var context = CreateHttpContext();
        context.Request.Headers.Authorization = $"Bearer {token}";
        var sut = CreateSut(context);

        // Act
        var issuer = sut.Issuer();
        var expUnix = sut.ExpUnix();
        var nbfUnix = sut.NbfUnix();

        // Assert
        issuer.Should().Be("haven-issuer");
        expUnix.Should().Be(exp);
        nbfUnix.Should().Be(nbf);
    }

    [Fact]
    public void Issuer_Should_ReturnNull_When_RawTokenIsMalformedJwt()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers.Authorization = "Bearer not-a-jwt";
        var sut = CreateSut(context);

        // Act
        var result = sut.Issuer();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void EnsureAuthenticated_Should_ThrowUnauthorizedAccessException_When_UserIsAnonymous()
    {
        // Arrange
        var context = CreateHttpContext(authenticationType: null);
        var sut = CreateSut(context);

        // Act
        var act = () => sut.EnsureAuthenticated();

        // Assert
        act.Should().Throw<UnauthorizedAccessException>();
    }

    [Fact]
    public void EnsureAuthenticated_Should_NotThrow_When_UserIsAuthenticated()
    {
        // Arrange
        var context = CreateHttpContext(new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()));
        var sut = CreateSut(context);

        // Act
        var action = () => sut.EnsureAuthenticated();

        // Assert
        action.Should().NotThrow();
    }

    private static AuthService CreateSut(HttpContext context)
    {
        return new AuthService(new HttpContextAccessor
        {
            HttpContext = context
        });
    }

    private static DefaultHttpContext CreateHttpContext(
        params Claim[] claims)
    {
        return CreateHttpContext("Test", claims);
    }

    private static DefaultHttpContext CreateHttpContext(
        string authenticationType,
        params Claim[] claims)
    {
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType));

        return context;
    }
}
