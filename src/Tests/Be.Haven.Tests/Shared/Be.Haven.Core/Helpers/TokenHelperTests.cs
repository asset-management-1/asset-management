namespace Be.Haven.Tests.Shared.Be.Haven.Core.Helpers;

public sealed class TokenHelperTests
{
    [Fact]
    public void BuildAuthResetCacheKey_Should_UseAuthResetKeyPattern_When_UserPublicIdIsProvided()
    {
        // Arrange
        var userPublicId = Guid.Parse("fc07838a-daad-46b8-ba8b-eab01715dd65");

        // Act
        var result = TokenHelper.BuildAuthResetCacheKey(userPublicId);

        // Assert
        result.Should().Be("auth:auth-reset-at:fc07838a-daad-46b8-ba8b-eab01715dd65");
    }

    [Fact]
    public void ToUnixTimeMilliseconds_Should_TreatUnspecifiedDateTimeAsUtc_When_KindIsUnspecified()
    {
        // Arrange
        var utc = new DateTime(2026, 5, 17, 0, 0, 0, DateTimeKind.Utc);
        var unspecified = DateTime.SpecifyKind(utc, DateTimeKind.Unspecified);

        // Act
        var result = TokenHelper.ToUnixTimeMilliseconds(unspecified);

        // Assert
        result.Should().Be(new DateTimeOffset(utc).ToUnixTimeMilliseconds());
    }

    [Fact]
    public void ToUnixTimeMilliseconds_Should_NormalizeLocalDateTime_When_KindIsLocal()
    {
        // Arrange
        var local = DateTime.SpecifyKind(new DateTime(2026, 5, 17, 10, 0, 0), DateTimeKind.Local);

        // Act
        var result = TokenHelper.ToUnixTimeMilliseconds(local);

        // Assert
        result.Should().Be(new DateTimeOffset(local.ToUniversalTime()).ToUnixTimeMilliseconds());
    }

    [Fact]
    public void ToAuthResetUnixMilliseconds_Should_ReturnZero_When_ResetTimestampIsMissing()
    {
        // Act
        var result = TokenHelper.ToAuthResetUnixMilliseconds(null);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void ToAuthResetUnixMilliseconds_Should_ReturnUnixMilliseconds_When_ResetTimestampExists()
    {
        // Arrange
        var resetAt = new DateTime(2026, 5, 17, 10, 0, 0, DateTimeKind.Utc);

        // Act
        var result = TokenHelper.ToAuthResetUnixMilliseconds(resetAt);

        // Assert
        result.Should().Be(new DateTimeOffset(resetAt).ToUnixTimeMilliseconds());
    }

    [Fact]
    public void GetAuthResetCacheTtl_Should_AddPaddingDay_ToRefreshTokenLifetime()
    {
        // Act
        var result = TokenHelper.GetAuthResetCacheTtl(7);

        // Assert
        result.Should().Be(TimeSpan.FromDays(8));
    }

    [Fact]
    public void GetExpirationDateTimeNowSpan_Should_ReturnSafeFutureSpan_When_TokenExpirationIsInFuture()
    {
        // Arrange
        var tokenExpiration = DateTime.Now.AddMinutes(5);

        // Act
        var result = TokenHelper.GetExpirationDateTimeNowSpan(tokenExpiration);

        // Assert
        result.Should().BeGreaterThan(TimeSpan.FromMinutes(3));
        result.Should().BeLessThan(TimeSpan.FromMinutes(5));
    }

    [Fact]
    public void GetExpirationDateTimeNowSpan_Should_ReturnPadding_When_TokenExpirationIsExpired()
    {
        // Arrange
        var tokenExpiration = DateTime.Now.AddMinutes(-1);

        // Act
        var result = TokenHelper.GetExpirationDateTimeNowSpan(tokenExpiration);

        // Assert
        result.Should().Be(TimeSpan.FromSeconds(60));
    }

    [Theory]
    [InlineData(null, 60)]
    [InlineData(30L, 60)]
    [InlineData(120L, 60)]
    [InlineData(180L, 120)]
    public void GetSafeTtlSpan_Should_SubtractPaddingOrReturnMinimumPadding_When_RemainingSecondsVary(
        long? remainingSeconds,
        int expectedSeconds)
    {
        // Act
        var result = TokenHelper.GetSafeTtlSpan(remainingSeconds);

        // Assert
        result.Should().Be(TimeSpan.FromSeconds(expectedSeconds));
    }

    [Fact]
    public void GetSafeTime_Should_SubtractPadding_When_DateTimeIsProvided()
    {
        // Arrange
        var dateTime = new DateTime(2026, 5, 17, 10, 0, 0, DateTimeKind.Utc);

        // Act
        var result = TokenHelper.GetSafeTime(dateTime);

        // Assert
        result.Should().Be(dateTime.AddSeconds(-60));
    }

    [Fact]
    public void TryGetUnixTimeUtc_Should_ParseSecondsClaim_When_ClaimContainsUnixSeconds()
    {
        // Arrange
        var expected = new DateTimeOffset(2026, 5, 17, 0, 0, 0, TimeSpan.Zero);
        var jwt = new JwtSecurityToken(claims: [new Claim("exp", expected.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture))]);

        // Act
        var success = TokenHelper.TryGetUnixTimeUtc(jwt, "exp", out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().Be(expected.UtcDateTime);
    }

    [Fact]
    public void TryGetUnixTimeUtc_Should_ParseMillisecondsClaim_When_ClaimContainsUnixMilliseconds()
    {
        // Arrange
        var expected = new DateTimeOffset(2026, 5, 17, 0, 0, 0, TimeSpan.Zero);
        var jwt = new JwtSecurityToken(claims: [new Claim("iat_ms", expected.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture))]);

        // Act
        var success = TokenHelper.TryGetUnixTimeUtc(jwt, "iat_ms", out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().Be(expected.UtcDateTime);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-number")]
    public void TryGetUnixTimeUtc_Should_ReturnFalse_When_ClaimIsMissingOrInvalid(string claimValue)
    {
        // Arrange
        var claims = string.IsNullOrEmpty(claimValue)
            ? []
            : new[] { new Claim("exp", claimValue) };
        var jwt = new JwtSecurityToken(claims: claims);

        // Act
        var success = TokenHelper.TryGetUnixTimeUtc(jwt, "exp", out var result);

        // Assert
        success.Should().BeFalse();
        result.Should().Be(default);
    }

    [Fact]
    public void ReplaceClaim_Should_RemoveExistingAliasAndAddSourceValue_When_SourceClaimExists()
    {
        // Arrange
        var source = new ClaimsIdentity([new Claim("source", "value-1")]);
        var claims = new List<Claim>
        {
            new("alias", "old")
        };

        // Act
        TokenHelper.ReplaceClaim(source, claims, "source", "alias");

        // Assert
        claims.Should().ContainSingle(x => x.Type == "alias" && x.Value == "value-1");
    }

    [Fact]
    public void ReplaceClaim_Should_KeepExistingClaims_When_SourceClaimIsMissing()
    {
        // Arrange
        var source = new ClaimsIdentity();
        var claims = new List<Claim>
        {
            new("alias", "old")
        };

        // Act
        TokenHelper.ReplaceClaim(source, claims, "source", "alias");

        // Assert
        claims.Should().ContainSingle(x => x.Type == "alias" && x.Value == "old");
    }

    [Fact]
    public void EnsureClaim_Should_NotDuplicateClaim_When_ClaimPairAlreadyExists()
    {
        // Arrange
        var identity = new ClaimsIdentity([new Claim("scope", "read")]);

        // Act
        TokenHelper.EnsureClaim(identity, "scope", "read");

        // Assert
        identity.Claims.Count(x => x.Type == "scope" && x.Value == "read").Should().Be(1);
    }

    [Fact]
    public void EnsureClaim_Should_AddClaim_When_ClaimPairIsMissing()
    {
        // Arrange
        var identity = new ClaimsIdentity();

        // Act
        TokenHelper.EnsureClaim(identity, "scope", "read");

        // Assert
        identity.Claims.Should().ContainSingle(x => x.Type == "scope" && x.Value == "read");
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("short-token", "short-token")]
    [InlineData("123456789012345", "123456789012")]
    public void SafeTokenPrefix_Should_ReturnBlankOrShortPrefix_When_TokenLengthVaries(
        string token,
        string expected)
    {
        // Act
        var result = TokenHelper.SafeTokenPrefix(token);

        // Assert
        result.Should().Be(expected);
    }
}
