namespace Authentication.Infrastructure.Mappings.Authentications;

/// <summary>
/// Registers Mapster mappings for authentication session issue helper models.
/// </summary>
public class AuthSessionMapping : IRegister
{
    /// <summary>
    /// Registers Mapster configuration for client-session refresh-token and login-response models.
    /// </summary>
    /// <param name="config">The Mapster configuration instance.</param>
    public void Register(TypeAdapterConfig config)
    {
        // Map common issue input into the refresh-token build model; generated token timing stays helper-owned.
        config.NewConfig<AuthSessionIssueRequestModel, AuthRefreshTokenBuildModel>()
            .Map(dest => dest.UserId, src => src.User.Id)
            .Ignore(dest => dest.SessionRefreshToken)
            .Ignore(dest => dest.JwtId)
            .Ignore(dest => dest.IssuedAt);

        // Map common issue input into the JWT build model; the prepared refresh token and timestamps are set explicitly.
        config.NewConfig<AuthSessionIssueRequestModel, AuthJwtBuildModel>()
            .Ignore(dest => dest.SessionRefreshToken)
            .Ignore(dest => dest.JwtId)
            .Ignore(dest => dest.IssuedAt)
            .Ignore(dest => dest.ExpiresAt);

        // Map common issue input into the login-response build model; the signed JWT is produced after refresh state.
        config.NewConfig<AuthSessionIssueRequestModel, AuthLoginResponseBuildModel>()
            .Ignore(dest => dest.Jwt);

        // Login responses expose only serialized token output and expiry metadata.
        config.NewConfig<AuthLoginResponseBuildModel, LoginResponseDto>()
            .Map(dest => dest.AccessToken, src => src.JwtSecurityTokenHandler.WriteToken(src.Jwt))
            .Map(dest => dest.RefreshToken, src => src.RawRefreshToken)
            .Map(dest => dest.ExpiresIn, src => (int)TimeSpan.FromMinutes(src.AuthOptions.AccessTokenMinutes).TotalSeconds)
            .Map(dest => dest.TokenType, _ => TOKEN_TYPE_BEARER);
    }
}
