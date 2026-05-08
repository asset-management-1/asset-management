namespace Haven.Application.Extensions;

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
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddTransient(typeof(Mediator.IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(Mediator.IPipelineBehavior<,>), typeof(InvalidationBehavior<,>));
        services.AddTransient(typeof(Mediator.IPipelineBehavior<,>), typeof(CachingBehavior<,>));
        services.AddDapperInfrastructure();
    }
}
