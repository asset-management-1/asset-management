using Be.Haven.Core.Middleware;

namespace Be.Haven.Core.Extensions.DI;

public static class ApplicationRegistration
{
    /// <summary>
    /// Configures and applies necessary application-level middlewares to the request pipeline.
    /// Adds the error handling middleware to manage exceptions and produce consistent error responses.
    /// </summary>
    /// <param name="app">The application builder used to configure the request pipeline.</param>
    public static void UseApplicationMiddlewares(this IApplicationBuilder app)
    {
        app.UseSerilogRequestLogging(options => { options.IncludeQueryInRequestPath = true; });
        app.UseMiddleware<CorrelationHandlerMiddleware>();
        app.UseMiddleware<ErrorHandlerMiddleware>();
    }

    /// <summary>
    /// Adds middleware to expose a unified health check endpoint for the application.
    /// This endpoint reports the health status of the application and its dependencies
    /// in JSON format, suitable for monitoring tools or Kubernetes probes.
    /// </summary>
    /// <param name="app">The application builder used to configure the middleware pipeline.</param>
    public static void UseHealthChecks(this IApplicationBuilder app)
    {
        app.UseHealthChecks(HEALTH, new HealthCheckOptions
        {
            // Run all health checks
            Predicate = _ => true,

            // Map health status to HTTP codes
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            },

            // Custom JSON response
            ResponseWriter = async (context, report) =>
            {
                if (context.RequestAborted.IsCancellationRequested)
                {
                    return;
                }
                
                context.Response.ContentType = TEXT_JSON;

                var response = new
                {
                    status = report.Status.ToString(),
                    totalDuration = report.TotalDuration.ToString(),
                    checks = report.Entries.Select(entry => new
                    {
                        name = entry.Key,
                        status = entry.Value.Status.ToString(),
                        description = entry.Value.Description,
                        exception = entry.Value.Exception?.Message,
                        duration = entry.Value.Duration.ToString()
                    })
                };

                await context.Response.WriteAsJsonAsync(
                    response,
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = true
                    },
                    context.RequestAborted);
            }
        });
    }
}