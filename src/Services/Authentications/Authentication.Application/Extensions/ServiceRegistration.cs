namespace Authentication.Application.Extensions;

/// <summary>
/// Registers application-layer services, handlers, validators, and pipeline behaviors for the authentication module.
/// </summary>
public static class ServiceRegistration
{
    /// <summary>
    /// Configures and registers application-specific services, including validation, mediation,
    /// and caching behaviors, within the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection used to register application services.</param>
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        RegisterCacheInvalidationPolicies(services);

        services.AddTransient(typeof(Mediator.IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(Mediator.IPipelineBehavior<,>), typeof(InvalidationBehavior<,>));
        services.AddTransient(typeof(Mediator.IPipelineBehavior<,>), typeof(CachingBehavior<,>));
    }

    /// <summary>
    /// Registers all cache invalidation policies defined in the authentication application assembly.
    /// </summary>
    /// <param name="services">The service collection that receives the policy registrations.</param>
    private static void RegisterCacheInvalidationPolicies(IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        var policyRegistrations = assembly
            .DefinedTypes
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .SelectMany(type => type
                .ImplementedInterfaces
                .Where(@interface => @interface.IsGenericType
                                     && @interface.GetGenericTypeDefinition() == typeof(ICacheInvalidationPolicy<>))
                .Select(@interface => new
                {
                    ServiceType = @interface,
                    ImplementationType = type.AsType()
                }));

        foreach (var registration in policyRegistrations)
        {
            services.AddTransient(registration.ServiceType, registration.ImplementationType);
        }
    }
}
