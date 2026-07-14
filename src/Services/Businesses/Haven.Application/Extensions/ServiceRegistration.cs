namespace Haven.Application.Extensions;

/// <summary>
/// Registers Haven application validation, mediator behaviors, and cache policies.
/// </summary>
public static class ServiceRegistration
{
    /// <summary>
    /// Configures and registers application-specific services, including validation, mediation,
    /// and caching behaviors, within the dependency injection container.
    /// </summary>
    /// <param name="services">
    /// The IServiceCollection instance used to register application services.
    /// </param>
    public static void AddApplication(this IServiceCollection services)
    {
        // Discover request validators before the validation behavior is added to the mediator pipeline.
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Cache policies and mediator behaviors are registered in their execution order.
        RegisterCacheInvalidationPolicies(services);
        services.AddTransient(typeof(Mediator.IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(Mediator.IPipelineBehavior<,>), typeof(InvalidationBehavior<,>));
        services.AddTransient(typeof(Mediator.IPipelineBehavior<,>), typeof(CachingBehavior<,>));
    }

    /// <summary>
    /// Registers all cache invalidation policies defined in the Haven application assembly.
    /// </summary>
    /// <param name="services">The service collection that receives the policy registrations.</param>
    private static void RegisterCacheInvalidationPolicies(IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Cache policies opt in per command and are discovered the same way as Authentication policies.
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
