namespace Be.Haven.ApiCommon.Swagger;

/// <summary>
/// Provides a typed base for Swagger example providers.
/// </summary>
/// <typeparam name="TExample">The example object type.</typeparam>
public abstract class SwaggerExampleProvider<TExample> : ISwaggerExampleProvider
{
    /// <summary>
    /// Builds the example object that will be serialized into the OpenAPI document.
    /// </summary>
    /// <returns>The typed example object.</returns>
    public object GetExample()
    {
        // Keep the OpenAPI filter non-generic while allowing provider implementations to be strongly typed.
        return BuildExample();
    }

    /// <summary>
    /// Builds the typed Swagger example.
    /// </summary>
    /// <returns>The typed Swagger example.</returns>
    protected abstract TExample BuildExample();
}
