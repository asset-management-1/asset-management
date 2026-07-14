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
        // Discover FluentValidation rules before MediatR dispatches application commands and queries.
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        RegisterCacheInvalidationPolicies(services);

        // Keep validation, cache invalidation, and read caching in a predictable pipeline order.
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
        // Inspect this assembly so each command-specific policy is registered against its closed generic contract.
        var assembly = Assembly.GetExecutingAssembly();

        // Discover concrete policy implementations without hand-maintaining every registration as features grow.
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

        // Register every discovered policy with the matching command contract consumed by the invalidation pipeline.
        foreach (var registration in policyRegistrations)
        {
            services.AddTransient(registration.ServiceType, registration.ImplementationType);
        }
    }
}
