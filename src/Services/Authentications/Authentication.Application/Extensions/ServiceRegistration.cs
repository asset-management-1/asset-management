namespace Authentication.Application.Extensions;

/// <summary>
/// Registers application-layer validators and pipeline behaviors for the authentication module.
/// </summary>
public static class ServiceRegistration
{
    /// <summary>
    /// Configures application validation before commands and queries reach their handlers.
    /// </summary>
    /// <param name="services">The service collection used to register application services.</param>
    public static void AddApplication(this IServiceCollection services)
    {
        // Discover FluentValidation rules before MediatR dispatches application commands and queries.
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Run request validation as the only Authentication pipeline concern currently backed by a query contract.
        services.AddTransient(typeof(Mediator.IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    }
}
