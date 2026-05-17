namespace Authentication.Application.Mappings;

/// <summary>
/// Registers mappings related to external-provider response projections.
/// </summary>
public class ExternalProviderResponseMapping : IRegister
{
    /// <summary>
    /// Registers Mapster configuration for external-provider response projection.
    /// </summary>
    /// <param name="config">The Mapster configuration instance.</param>
    public void Register(TypeAdapterConfig config)
    {
        // Configured provider names map into unlinked response rows before linked state is merged in service code.
        config.NewConfig<string, ExternalProviderResponseDto>()
            .Map(dest => dest.Provider, src => src)
            .Map(dest => dest.IsLinked, _ => false);

        // Read-model rows represent linked providers; unlinked providers are added by the user service.
        config.NewConfig<LinkedExternalProviderReadModelDto, ExternalProviderResponseDto>()
            .Map(dest => dest.Provider, src => src.Provider)
            .Map(dest => dest.IsLinked, _ => true);
    }
}
