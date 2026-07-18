namespace Be.Haven.Tests.Common;

/// <summary>
/// Registers production Mapster configurations once before the test assembly executes.
/// </summary>
internal static class MapsterTestBootstrap
{
    /// <summary>
    /// Mirrors application startup scanning without allowing individual tests to mutate global configuration later.
    /// </summary>
    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void Initialize()
    {
        new BaseMappingConfig().Register(TypeAdapterConfig.GlobalSettings);

        TypeAdapterConfig.GlobalSettings.Scan(
            typeof(global::Haven.Application.Mappings.Properties.PropertyRequestMapping).Assembly,
            typeof(global::Haven.Infrastructure.Mappings.Rooms.RoomFieldMutationMapping).Assembly,
            typeof(Authentication.Application.Mappings.Authentications.AuthenticationRequestMapping).Assembly,
            typeof(Authentication.Infrastructure.Mappings.Authentications.AuthSessionMapping).Assembly);
    }
}
