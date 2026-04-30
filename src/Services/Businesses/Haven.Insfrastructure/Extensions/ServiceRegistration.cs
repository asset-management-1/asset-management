using Be.Haven.Core.Extensions.DI;
using Be.Haven.Core.Interfaces.Services;
using Be.Haven.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Haven.Insfrastructure.Extensions;

public static class ServiceRegistration
{
    /// <summary>
    /// Registers the infrastructure layer services and dependencies to the specified service collection.
    /// </summary>
    /// <param name="services">The IServiceCollection to which the services will be added.</param>
    /// <param name="configuration">The IConfiguration instance containing configuration values.</param>
    public static void AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
    }
}