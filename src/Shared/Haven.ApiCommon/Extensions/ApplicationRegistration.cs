namespace Haven.ApiCommon.Extensions;

public static class ApplicationRegistration
{
    /// <summary>
    /// Configures and applies necessary application-level middlewares to the request pipeline.
    /// Adds the error handling middleware to manage exceptions and produce consistent error responses.
    /// </summary>
    /// <param name="app">The application builder used to configure the request pipeline.</param>
    public static void UseSwaggerConfiguration(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.EnableFilter();                 // Show the search/filter box at the top of Swagger UI
            options.DocExpansion(DocExpansion.List); // Collapse all endpoints by default
            var provider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();
            foreach (var desc in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint(string.Format(ENDPOINT_URL, desc.GroupName), string.Format(ENDPOINT_NAME, desc.ApiVersion));
            }
            options.RoutePrefix = ROUTE_PREFIX; // Makes Swagger UI the default landing page
            options.DefaultModelsExpandDepth(-1); // Disables schemas/models from expanding by default
            options.EnableDeepLinking();
            options.DisplayRequestDuration(); // Shows the duration of each request in Swagger UI
        });
    }
}