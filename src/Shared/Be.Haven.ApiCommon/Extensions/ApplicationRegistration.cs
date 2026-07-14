namespace Be.Haven.ApiCommon.Extensions;

public static class ApplicationRegistration
{
    /// <summary>
    /// Cached shared Swagger operation-search script content.
    /// </summary>
    private static readonly Lazy<string> SwaggerOperationSearchScript = new(LoadSwaggerOperationSearchScript);

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
    /// Configures and applies necessary application-level middlewares to the request pipeline.
    /// Adds the error handling middleware to manage exceptions and produce consistent error responses.
    /// </summary>
    /// <param name="app">The application builder used to configure the request pipeline.</param>
    public static void UseSwaggerConfiguration(this IApplicationBuilder app)
    {
        app.UseSwagger();

        // Keep the UI customization inside ApiCommon so each service gets the same Swagger search behavior.
        app.Map(
            OPERATION_SEARCH_SCRIPT_PATH,
            branch => branch.Run(WriteSwaggerOperationSearchScriptAsync));
        app.UseSwaggerUI(options =>
        {
            options.EnableFilter(); // Render Swagger UI's filter input; the shared script upgrades it to API text search.
            options.InjectJavascript(OPERATION_SEARCH_SCRIPT_PATH);
            options.DocExpansion(DocExpansion.List); // Collapse all endpoints by default.
            var provider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();
            foreach (var desc in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint(string.Format(ENDPOINT_URL, desc.GroupName), string.Format(ENDPOINT_NAME, desc.ApiVersion));
            }

            options.RoutePrefix = ROUTE_PREFIX; // Makes Swagger UI the default landing page.
            options.DefaultModelsExpandDepth(-1); // Disables schemas/models from expanding by default.
            options.EnableDeepLinking();
            options.DisplayRequestDuration(); // Shows the duration of each request in Swagger UI.
        });
    }

    /// <summary>
    /// Writes the embedded Swagger operation-search script to the response.
    /// </summary>
    /// <param name="context">The HTTP context for the script request.</param>
    /// <returns>A task that completes when the script response is written.</returns>
    private static Task WriteSwaggerOperationSearchScriptAsync(HttpContext context)
    {
        // The script is static and cached after the first request; only the response content type is set per request.
        context.Response.ContentType = JAVASCRIPT_CONTENT_TYPE;
        return context.Response.WriteAsync(SwaggerOperationSearchScript.Value, context.RequestAborted);
    }

    /// <summary>
    /// Loads the embedded Swagger operation-search script from the ApiCommon assembly.
    /// </summary>
    /// <returns>The JavaScript source code used by Swagger UI.</returns>
    private static string LoadSwaggerOperationSearchScript()
    {
        // Read from an embedded resource so API services do not need to copy or host their own Swagger UI asset.
        return LoadSwaggerOperationSearchScript(OPERATION_SEARCH_SCRIPT_RESOURCE_NAME);
    }

    private static string LoadSwaggerOperationSearchScript(string resourceName)
    {
        // Read from an embedded resource so API services do not need to copy or host their own Swagger UI asset.
        var assembly = typeof(ApplicationRegistration).Assembly;
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            throw new InvalidOperationException(string.Format(
                OPERATION_SEARCH_SCRIPT_RESOURCE_NOT_FOUND,
                resourceName));
        }

        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
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
            ResponseWriter = WriteHealthCheckResponseAsync
        });
    }

    private static async Task WriteHealthCheckResponseAsync(HttpContext context, HealthReport report)
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
}
