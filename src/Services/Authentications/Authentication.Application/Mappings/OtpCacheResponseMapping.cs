namespace Authentication.Application.Mappings;

/// <summary>
/// Registers mappings related to OTP cache payloads.
/// </summary>
public class OtpCacheResponseMapping : IRegister
{
    /// <summary>
    /// Registers Mapster configuration for OTP cache payload projection.
    /// </summary>
    /// <param name="config">The Mapster configuration instance.</param>
    public void Register(TypeAdapterConfig config)
    {
        // New OTP cache entries always start with zero failed verification attempts.
        config.NewConfig<OtpCacheRequestDto, OtpCacheResponseDto>()
            .Map(dest => dest.Code, src => src.OtpCode)
            .Map(dest => dest.Attempts, _ => 0);
    }
}
