namespace Be.Haven.ApiCommon.Options.Swagger;

/// <summary>
/// Configures Swagger documents for each discovered API version (v1, v2, ...).
/// </summary>
public sealed class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    /// <summary>
    /// Injects the API version provider used to enumerate all API versions.
    /// </summary>
    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
        => _provider = provider;

    /// <summary>
    /// Creates one SwaggerDoc per API version using the version group name (e.g., "v1", "v2").
    /// </summary>
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var desc in _provider.ApiVersionDescriptions)
        {
            // Register Swagger document metadata for the current API version.
            options.SwaggerDoc(desc.GroupName, new OpenApiInfo
            {
                Title = TITLE,
                Version = desc.ApiVersion.ToString(),
                Description = DESCRIPTION,
                Contact = new OpenApiContact
                {
                    Name = Contact.NAME,
                    Email = Contact.EMAIL,
                    Url = new Uri(Contact.URL)
                },
                License = new OpenApiLicense
                {
                    Name = License.NAME,
                    Url = new Uri(License.URL)
                }
            });
        }
    }
}
