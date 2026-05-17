namespace Authentication.Infrastructure.Helpers;

/// <summary>
/// Provides Authentication-service helpers for JWT and client-session refresh-token issuance.
/// </summary>
internal static class AuthSessionHelper
{
    /// <summary>
    /// Builds the persisted client-session refresh-token state and login response for one session.
    /// </summary>
    /// <param name="request">The auth-session issue request.</param>
    /// <returns>The prepared session issue model.</returns>
    public static AuthSessionIssueModel BuildIssueModel(AuthSessionIssueRequestModel request)
    {
        // Build token timestamps and identifiers once so JWT and refresh state remain aligned.
        var issuedAt = DateTime.UtcNow;
        var expiresAt = issuedAt.AddMinutes(request.AuthOptions.AccessTokenMinutes);
        var jwtId = Guid.NewGuid().ToString("N");
        var refreshTokenBuildRequest = request.Adapt<AuthRefreshTokenBuildModel>();
        refreshTokenBuildRequest.SessionRefreshToken = request.SessionRefreshToken;
        refreshTokenBuildRequest.JwtId = jwtId;
        refreshTokenBuildRequest.IssuedAt = issuedAt;
        var refreshToken = ApplySessionRefreshToken(refreshTokenBuildRequest);

        var jwtBuildRequest = request.Adapt<AuthJwtBuildModel>();
        jwtBuildRequest.SessionRefreshToken = refreshToken;
        jwtBuildRequest.JwtId = jwtId;
        jwtBuildRequest.IssuedAt = issuedAt;
        jwtBuildRequest.ExpiresAt = expiresAt;
        var jwt = BuildJwt(jwtBuildRequest);

        var loginResponseBuildRequest = request.Adapt<AuthLoginResponseBuildModel>();
        loginResponseBuildRequest.Jwt = jwt;

        return new AuthSessionIssueModel
        {
            RefreshToken = refreshToken,
            LoginResponse = loginResponseBuildRequest.Adapt<LoginResponseDto>()
        };
    }

    /// <summary>
    /// Generates a cryptographically random refresh token.
    /// </summary>
    /// <returns>The raw refresh token value.</returns>
    public static string GenerateRefreshToken()
    {
        // Generate high-entropy refresh-token material before hashing it for persistence.
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    /// <summary>
    /// Hashes a raw refresh token for persistence and lookup.
    /// </summary>
    /// <param name="rawToken">The raw refresh token value.</param>
    /// <returns>The refresh token hash.</returns>
    public static string HashRefreshToken(string rawToken)
    {
        // Store and compare only the SHA-256 hash of refresh-token values.
        return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
    }

    /// <summary>
    /// Builds the signed JWT for one Haven authenticated session.
    /// </summary>
    /// <param name="request">The JWT build request.</param>
    /// <returns>The signed JWT.</returns>
    private static JwtSecurityToken BuildJwt(AuthJwtBuildModel request)
    {
        // Build and sign the Haven token from the normalized user identity.
        var claims = BuildBaseClaims(request.User, request.SessionRefreshToken, request.JwtId, request.IssuedAt);
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(request.AuthOptions.SecretKey)),
            SecurityAlgorithms.HmacSha256);

        return new JwtSecurityToken(
            issuer: request.AuthOptions.Issuer,
            audience: request.AuthOptions.Audiences.FirstOrDefault(),
            claims: claims,
            notBefore: request.IssuedAt,
            expires: request.ExpiresAt,
            signingCredentials: credentials);
    }

    /// <summary>
    /// Applies issue-time state to the client-session refresh token for persistence.
    /// </summary>
    /// <param name="request">The refresh-token build request.</param>
    /// <returns>The refresh token row to persist.</returns>
    private static RefreshToken ApplySessionRefreshToken(AuthRefreshTokenBuildModel request)
    {
        // Reuse the existing row when present so repeated login/refresh does not append rows.
        var refreshToken = request.SessionRefreshToken ?? new RefreshToken();
        var currentTokenHash = refreshToken.TokenHash;
        if (request.RenewSessionPublicId || !refreshToken.SessionPublicId.HasValue)
        {
            refreshToken.SessionPublicId = Guid.NewGuid();
        }

        // Mapster copies the latest client metadata; token state below remains explicitly service-owned.
        request.DeviceContext.Adapt(refreshToken);

        // Persist only token hashes; the raw refresh token is returned once to the caller.
        refreshToken.UserId = request.UserId;
        refreshToken.PreviousTokenHash = string.IsNullOrWhiteSpace(currentTokenHash) ? null : currentTokenHash;
        refreshToken.TokenHash = HashRefreshToken(request.RawRefreshToken);
        refreshToken.JwtId = request.JwtId;
        refreshToken.ExpiresAt = request.IssuedAt.AddDays(request.AuthOptions.RefreshTokenDays);
        refreshToken.LastUsedAt = request.IssuedAt;
        refreshToken.RevokedAt = null;
        refreshToken.ReplacedByTokenHash = null;
        refreshToken.IsDeleted = false;
        return refreshToken;
    }

    /// <summary>
    /// Builds the base JWT claims shared by all Haven access tokens.
    /// </summary>
    /// <param name="user">The authenticated user.</param>
    /// <param name="refreshToken">The active client-session refresh token.</param>
    /// <param name="jwtId">The generated JWT identifier.</param>
    /// <param name="issuedAt">The UTC timestamp when the access token is issued.</param>
    /// <returns>The base JWT claim list.</returns>
    private static List<Claim> BuildBaseClaims(
        User user,
        RefreshToken refreshToken,
        string jwtId,
        DateTime issuedAt)
    {
        // Haven access tokens carry the public user id as the canonical downstream identity.
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.PublicId.ToString()),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, jwtId),
            new(TokenClaimTypes.SESSION_ID, refreshToken.SessionPublicId?.ToString() ?? string.Empty),
            new(TokenClaimTypes.DEVICE_ID, refreshToken.DeviceId ?? string.Empty),
            new(TokenClaimTypes.DEVICE_NAME, refreshToken.DeviceName ?? string.Empty),
            new(TokenClaimTypes.DEVICE_TYPE, refreshToken.DeviceType ?? string.Empty),
            new(TokenClaimTypes.USER_AGENT, refreshToken.UserAgent ?? string.Empty),
            new(TokenClaimTypes.IP_ADDRESS, refreshToken.IpAddress ?? string.Empty),
            new(
                TokenClaimTypes.HAVEN_ISSUED_AT_MS,
                TokenHelper.ToUnixTimeMilliseconds(issuedAt).ToString(CultureInfo.InvariantCulture))
        };

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            // Include email only when present so external-only accounts without email can still authenticate.
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
        }

        return claims;
    }
}
