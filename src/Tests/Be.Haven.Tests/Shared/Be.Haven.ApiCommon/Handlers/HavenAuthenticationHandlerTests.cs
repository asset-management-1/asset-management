using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Builder;
using Microsoft.IdentityModel.Tokens;
using System.Text.Encodings.Web;
using System.Reflection;

#pragma warning disable ASPDEPR004, ASPDEPR008

namespace Be.Haven.Tests.Shared.Be.Haven.ApiCommon.Handlers;

public sealed class HavenAuthenticationHandlerTests
{
    private const string Scheme = "Haven";
    private const string Issuer = "haven-tests";
    private const string Audience = "haven-api";
    private const string Secret = "0123456789abcdef0123456789abcdef";

    [Fact]
    public async Task HandleAuthenticateAsync_Should_ReturnOk_When_TokenSessionAndResetAreValid()
    {
        // Arrange
        var userPublicId = Guid.NewGuid();
        var sessionPublicId = Guid.NewGuid();
        var sessionValidator = new Mock<IClientSessionValidator>();
        sessionValidator
            .Setup(x => x.IsSessionActiveAsync(userPublicId, sessionPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var authResetValidator = new Mock<IAuthResetValidator>();
        authResetValidator
            .Setup(x => x.IsTokenValidAsync(userPublicId, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        using var server = CreateServer(authResetValidator.Object, sessionValidator.Object);
        var client = server.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            BEARER,
            CreateToken(userPublicId, sessionPublicId));

        // Act
        var result = await client.GetAsync("/protected");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        (await result.Content.ReadAsStringAsync()).Should().Be("ok");
    }

    [Fact]
    public async Task HandleChallengeAsync_Should_WriteUnauthorizedEnvelope_When_ProtectedEndpointHasNoBearerToken()
    {
        // Arrange
        using var server = CreateServer(
            Mock.Of<IAuthResetValidator>(),
            Mock.Of<IClientSessionValidator>());
        var client = server.CreateClient();

        // Act
        var result = await client.GetAsync("/protected");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var body = await result.Content.ReadAsStringAsync();
        body.Should().Contain(UNAUTHORIZED);
        body.Should().Contain(AUTHENTICATION_FAIL);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_Should_SkipAuthentication_When_RequestTargetsHealthEndpoint()
    {
        // Arrange
        var sessionValidator = new Mock<IClientSessionValidator>();
        var authResetValidator = new Mock<IAuthResetValidator>();
        using var server = CreateServer(authResetValidator.Object, sessionValidator.Object);
        var client = server.CreateClient();

        // Act
        var result = await client.GetAsync("/health");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        (await result.Content.ReadAsStringAsync()).Should().Be("healthy");
        sessionValidator.Verify(
            x => x.IsSessionActiveAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        authResetValidator.Verify(
            x => x.IsTokenValidAsync(It.IsAny<Guid>(), It.IsAny<long>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_Should_ReturnUnauthorized_When_ClientSessionIsInactive()
    {
        // Arrange
        var userPublicId = Guid.NewGuid();
        var sessionPublicId = Guid.NewGuid();
        var sessionValidator = new Mock<IClientSessionValidator>();
        sessionValidator
            .Setup(x => x.IsSessionActiveAsync(userPublicId, sessionPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        using var server = CreateServer(
            Mock.Of<IAuthResetValidator>(),
            sessionValidator.Object);
        var client = server.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            BEARER,
            CreateToken(userPublicId, sessionPublicId));

        // Act
        var result = await client.GetAsync("/protected");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var body = await result.Content.ReadAsStringAsync();
        body.Should().Contain(UNAUTHORIZED);
        sessionValidator.Verify(
            x => x.IsSessionActiveAsync(userPublicId, sessionPublicId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_Should_ReturnUnauthorized_When_TokenHasNoSessionClaim()
    {
        // Arrange
        var userPublicId = Guid.NewGuid();
        using var server = CreateServer(
            Mock.Of<IAuthResetValidator>(),
            Mock.Of<IClientSessionValidator>());
        var client = server.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            BEARER,
            CreateToken(userPublicId, Guid.NewGuid(), includeSession: false));

        // Act
        var result = await client.GetAsync("/protected");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_Should_ReturnUnauthorized_When_TokenHasNoIssuedAtClaim()
    {
        // Arrange
        var userPublicId = Guid.NewGuid();
        var sessionPublicId = Guid.NewGuid();
        var sessionValidator = new Mock<IClientSessionValidator>();
        sessionValidator
            .Setup(x => x.IsSessionActiveAsync(userPublicId, sessionPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        using var server = CreateServer(
            Mock.Of<IAuthResetValidator>(),
            sessionValidator.Object);
        var client = server.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            BEARER,
            CreateToken(userPublicId, sessionPublicId, includeIssuedAt: false));

        // Act
        var result = await client.GetAsync("/protected");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_Should_ReturnUnauthorized_When_TokenSubjectIsNotGuid()
    {
        // Arrange
        using var server = CreateServer(
            Mock.Of<IAuthResetValidator>(),
            Mock.Of<IClientSessionValidator>());
        var client = server.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            BEARER,
            CreateToken(
                Guid.NewGuid(),
                Guid.NewGuid(),
                useJwtClaimNames: true,
                subjectOverride: "not-a-guid"));

        // Act
        var result = await client.GetAsync("/protected");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_Should_ReturnUnauthorized_When_AuthResetDependencyFails()
    {
        // Arrange
        var userPublicId = Guid.NewGuid();
        var sessionPublicId = Guid.NewGuid();
        var sessionValidator = new Mock<IClientSessionValidator>();
        sessionValidator
            .Setup(x => x.IsSessionActiveAsync(userPublicId, sessionPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var authResetValidator = new Mock<IAuthResetValidator>();
        authResetValidator
            .Setup(x => x.IsTokenValidAsync(userPublicId, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpStatusCodeException(
                "Auth reset cache is unavailable.",
                "error_auth_reset_unavailable",
                StatusCodes.Status503ServiceUnavailable));
        using var server = CreateServer(authResetValidator.Object, sessionValidator.Object);
        var client = server.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            BEARER,
            CreateToken(userPublicId, sessionPublicId));

        // Act
        var result = await client.GetAsync("/protected");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var body = await result.Content.ReadAsStringAsync();
        body.Should().Contain(UNAUTHORIZED);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_Should_ReturnUnauthorized_When_AuthResetRejectsToken()
    {
        // Arrange
        var userPublicId = Guid.NewGuid();
        var sessionPublicId = Guid.NewGuid();
        var sessionValidator = new Mock<IClientSessionValidator>();
        sessionValidator
            .Setup(x => x.IsSessionActiveAsync(userPublicId, sessionPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var authResetValidator = new Mock<IAuthResetValidator>();
        authResetValidator
            .Setup(x => x.IsTokenValidAsync(userPublicId, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        using var server = CreateServer(authResetValidator.Object, sessionValidator.Object);
        var client = server.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            BEARER,
            CreateToken(userPublicId, sessionPublicId));

        // Act
        var result = await client.GetAsync("/protected");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        authResetValidator.Verify(
            x => x.IsTokenValidAsync(userPublicId, It.IsAny<long>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_Should_ReturnUnauthorized_When_TokenIsExpired()
    {
        // Arrange
        using var server = CreateServer(
            Mock.Of<IAuthResetValidator>(),
            Mock.Of<IClientSessionValidator>());
        var client = server.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            BEARER,
            CreateToken(Guid.NewGuid(), Guid.NewGuid(), expires: DateTime.UtcNow.AddMinutes(-5)));

        // Act
        var result = await client.GetAsync("/protected");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await result.Content.ReadAsStringAsync()).Should().Contain(UNAUTHORIZED);
    }

    [Fact]
    public async Task HandleForbiddenAsync_Should_WriteForbiddenEnvelope_When_UserLacksRequiredRole()
    {
        // Arrange
        var userPublicId = Guid.NewGuid();
        var sessionPublicId = Guid.NewGuid();
        var sessionValidator = new Mock<IClientSessionValidator>();
        sessionValidator
            .Setup(x => x.IsSessionActiveAsync(userPublicId, sessionPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var authResetValidator = new Mock<IAuthResetValidator>();
        authResetValidator
            .Setup(x => x.IsTokenValidAsync(userPublicId, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        using var server = CreateServer(authResetValidator.Object, sessionValidator.Object);
        var client = server.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            BEARER,
            CreateToken(userPublicId, sessionPublicId));

        // Act
        var result = await client.GetAsync("/admin");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var body = await result.Content.ReadAsStringAsync();
        body.Should().Contain(FORBIDDEN);
        body.Should().Contain(MISSING_PERMISSION);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_Should_NormalizeFallbackClaims_When_TokenUsesJwtClaimNames()
    {
        // Arrange
        var userPublicId = Guid.NewGuid();
        var sessionPublicId = Guid.NewGuid();
        var sessionValidator = new Mock<IClientSessionValidator>();
        sessionValidator
            .Setup(x => x.IsSessionActiveAsync(userPublicId, sessionPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var authResetValidator = new Mock<IAuthResetValidator>();
        authResetValidator
            .Setup(x => x.IsTokenValidAsync(userPublicId, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        using var server = CreateServer(authResetValidator.Object, sessionValidator.Object);
        var client = server.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            BEARER,
            CreateToken(userPublicId, sessionPublicId, useJwtClaimNames: true, roles: "Tenant;Landlord"));

        // Act
        var result = await client.GetAsync("/claims");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await result.Content.ReadAsStringAsync();
        body.Should().Contain(userPublicId.ToString());
        body.Should().Contain("Haven Tester");
        body.Should().Contain("tester@haven.test");
        body.Should().Contain("Tenant");
        body.Should().Contain("Landlord");
    }

    [Fact]
    public async Task HandleAuthenticateAsync_Should_NormalizeSingleRoleClaim_When_TokenUsesRoleClaimName()
    {
        // Arrange
        var userPublicId = Guid.NewGuid();
        var sessionPublicId = Guid.NewGuid();
        var sessionValidator = new Mock<IClientSessionValidator>();
        sessionValidator
            .Setup(x => x.IsSessionActiveAsync(userPublicId, sessionPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var authResetValidator = new Mock<IAuthResetValidator>();
        authResetValidator
            .Setup(x => x.IsTokenValidAsync(userPublicId, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        using var server = CreateServer(authResetValidator.Object, sessionValidator.Object);
        var client = server.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            BEARER,
            CreateToken(
                userPublicId,
                sessionPublicId,
                useJwtClaimNames: true,
                role: "Tenant"));

        // Act
        var result = await client.GetAsync("/claims");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        (await result.Content.ReadAsStringAsync()).Should().Contain("Tenant");
    }

    [Fact]
    public async Task HandleChallengeAsync_Should_PreserveDependencyFailurePayload_When_AuthenticateFeatureHasStatusException()
    {
        // Arrange
        using var server = CreateServer(
            Mock.Of<IAuthResetValidator>(),
            Mock.Of<IClientSessionValidator>());
        var client = server.CreateClient();

        // Act
        var result = await client.GetAsync("/challenge-dependency");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        var body = await result.Content.ReadAsStringAsync();
        body.Should().Contain("error_auth_dependency");
        body.Should().Contain("Auth dependency is unavailable.");
        body.Should().Contain("1.2");
    }

    [Fact]
    public void NormalizePrincipal_Should_AddRoleClaims_When_PrincipalUsesRoleAliases()
    {
        // Arrange
        var userPublicId = Guid.NewGuid();
        var identity = new ClaimsIdentity(
        [
            new Claim(TokenClaimTypes.SUBJECT, userPublicId.ToString()),
            new Claim(TokenClaimTypes.ROLE, "Tenant"),
            new Claim(TokenClaimTypes.ROLES, "Landlord Manager")
        ], Scheme);
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = InvokeNormalizePrincipal(principal);

        // Assert
        result.FindAll(ClaimTypes.Role).Select(x => x.Value)
            .Should().Contain(["Tenant", "Landlord", "Manager"]);
    }

    [Fact]
    public void NormalizePrincipal_Should_ThrowSecurityTokenException_When_IdentityIsAnonymous()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var action = () => InvokeNormalizePrincipal(principal);

        // Assert
        var exception = action.Should().Throw<TargetInvocationException>().Which;
        exception.InnerException.Should().BeOfType<SecurityTokenException>()
            .Which.Message.Should().Be(AUTHENTICATION_FAIL);
    }

    [Fact]
    public async Task ValidateSessionAsync_Should_ThrowSecurityTokenException_When_NormalizedUserClaimIsInvalid()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "not-a-guid")
        ], Scheme));
        var sut = CreateHandler();

        // Act
        var action = async () => await InvokePrivateTaskAsync(sut, "ValidateSessionAsync", principal);

        // Assert
        await action.Should().ThrowAsync<SecurityTokenException>()
            .WithMessage(AUTHENTICATION_FAIL);
    }

    [Fact]
    public async Task ValidateAuthResetAsync_Should_ThrowSecurityTokenException_When_NormalizedUserClaimIsInvalid()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "not-a-guid")
        ], Scheme));
        var sut = CreateHandler();

        // Act
        var action = async () => await InvokePrivateTaskAsync(sut, "ValidateAuthResetAsync", principal);

        // Assert
        await action.Should().ThrowAsync<SecurityTokenException>()
            .WithMessage(AUTHENTICATION_FAIL);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_Should_NotAuthenticate_When_EndpointAllowsAnonymous()
    {
        // Arrange
        using var server = CreateServer(
            Mock.Of<IAuthResetValidator>(),
            Mock.Of<IClientSessionValidator>());
        var client = server.CreateClient();

        // Act
        var result = await client.GetAsync("/anonymous");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        (await result.Content.ReadAsStringAsync()).Should().Be("anonymous");
    }

    private static TestServer CreateServer(
        IAuthResetValidator authResetValidator,
        IClientSessionValidator sessionValidator)
    {
        return new TestServer(new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddRouting();
                services.AddAuthorization();
                services.AddSingleton(Options.Create(new AuthenticationTokenValidationOptions
                {
                    Issuer = Issuer,
                    Audiences = [Audience],
                    SecretKey = Secret
                }));
                services.AddSingleton(authResetValidator);
                services.AddSingleton(sessionValidator);
                services.AddAuthentication(Scheme)
                    .AddScheme<AuthenticationSchemeOptions, HavenAuthenticationHandler>(Scheme, _ => { });
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseAuthentication();
                app.UseAuthorization();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapGet("/health", context => context.Response.WriteAsync("healthy"));
                    endpoints.MapGet("/protected", context => context.Response.WriteAsync("ok"))
                        .RequireAuthorization();
                    endpoints.MapGet("/admin", context => context.Response.WriteAsync("admin"))
                        .RequireAuthorization(policy => policy.RequireRole("Admin"));
                    endpoints.MapGet("/claims", context =>
                    {
                        var roles = string.Join(",", context.User.FindAll(ClaimTypes.Role).Select(x => x.Value));

                        return context.Response.WriteAsync(string.Join(
                            "|",
                            context.User.FindFirstValue(ClaimTypes.NameIdentifier),
                            context.User.FindFirstValue(ClaimTypes.Name),
                            context.User.FindFirstValue(ClaimTypes.Email),
                            roles));
                    }).RequireAuthorization();
                    endpoints.MapGet("/challenge-dependency", async context =>
                    {
                        context.Features.Set<IAuthenticateResultFeature>(new AuthenticationResultFeatureStub(
                            AuthenticateResult.Fail(new HttpStatusCodeException(
                                "Auth dependency is unavailable.",
                                "error_auth_dependency",
                                StatusCodes.Status503ServiceUnavailable))));
                        var feature = new Mock<IApiVersioningFeature>();
                        feature.SetupGet(x => x.RequestedApiVersion).Returns(new ApiVersion(1, 2));
                        context.Features.Set(feature.Object);

                        await context.ChallengeAsync(Scheme);
                    });
                    endpoints.MapGet("/anonymous", context => context.Response.WriteAsync("anonymous"))
                        .AllowAnonymous();
                });
            }));
    }

    private static ClaimsPrincipal InvokeNormalizePrincipal(ClaimsPrincipal principal)
    {
        var method = typeof(HavenAuthenticationHandler).GetMethod(
            "NormalizePrincipal",
            BindingFlags.Static | BindingFlags.NonPublic);

        return (ClaimsPrincipal)method.Invoke(null, [principal]);
    }

    private static async Task InvokePrivateTaskAsync(
        HavenAuthenticationHandler handler,
        string methodName,
        ClaimsPrincipal principal)
    {
        var method = typeof(HavenAuthenticationHandler).GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.NonPublic);
        var task = (Task)method.Invoke(handler, [principal, CancellationToken.None]);

        await task;
    }

    private static HavenAuthenticationHandler CreateHandler()
    {
        var schemeOptions = new Mock<IOptionsMonitor<AuthenticationSchemeOptions>>();
        schemeOptions
            .Setup(x => x.Get(It.IsAny<string>()))
            .Returns(new AuthenticationSchemeOptions());
        schemeOptions
            .SetupGet(x => x.CurrentValue)
            .Returns(new AuthenticationSchemeOptions());

        return new HavenAuthenticationHandler(
            schemeOptions.Object,
            LoggerFactory.Create(_ => { }),
            UrlEncoder.Default,
            Options.Create(new AuthenticationTokenValidationOptions
            {
                Issuer = Issuer,
                Audiences = [Audience],
                SecretKey = Secret
            }),
            Mock.Of<IAuthResetValidator>(),
            Mock.Of<IClientSessionValidator>());
    }

    private static string CreateToken(
        Guid userPublicId,
        Guid sessionPublicId,
        DateTime? expires = null,
        bool includeSession = true,
        bool useJwtClaimNames = false,
        string roles = null,
        string role = null,
        bool includeIssuedAt = true,
        string subjectOverride = null)
    {
        var now = DateTime.UtcNow;
        var claims = useJwtClaimNames
            ?
            [
                new(TokenClaimTypes.SUBJECT, subjectOverride ?? userPublicId.ToString()),
                new(TokenClaimTypes.NAME, "Haven Tester"),
                new(TokenClaimTypes.EMAIL, "tester@haven.test")
            ]
            : new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userPublicId.ToString()),
                new(ClaimTypes.Name, "Haven Tester"),
                new(ClaimTypes.Email, "tester@haven.test")
            };
        if (includeSession)
        {
            claims.Add(new Claim(TokenClaimTypes.SESSION_ID, sessionPublicId.ToString()));
        }

        if (!string.IsNullOrWhiteSpace(roles))
        {
            claims.Add(new Claim(TokenClaimTypes.ROLES, roles));
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            claims.Add(new Claim(TokenClaimTypes.ROLE, role));
        }

        if (includeIssuedAt)
        {
            claims.Add(new Claim(
                TokenClaimTypes.HAVEN_ISSUED_AT_MS,
                new DateTimeOffset(now).ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture)));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
        var expiresAt = expires ?? now.AddMinutes(5);
        var notBefore = expiresAt <= now
            ? expiresAt.AddMinutes(-5)
            : now.AddMinutes(-1);
        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            notBefore: notBefore,
            expires: expiresAt,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed class AuthenticationResultFeatureStub : IAuthenticateResultFeature
    {
        public AuthenticationResultFeatureStub(AuthenticateResult authenticateResult)
        {
            AuthenticateResult = authenticateResult;
        }

        public AuthenticateResult AuthenticateResult { get; set; }
    }
}

#pragma warning restore ASPDEPR004, ASPDEPR008
