namespace Be.Haven.ApiCommon.Swagger.Attributes;

/// <summary>
/// Declares the example provider used for an operation request body.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class SwaggerRequestExampleAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SwaggerRequestExampleAttribute"/> class.
    /// </summary>
    /// <param name="providerType">The provider type that creates the request example.</param>
    public SwaggerRequestExampleAttribute(Type providerType)
    {
        ProviderType = providerType;
    }

    /// <summary>
    /// Gets the provider type that creates the request example.
    /// </summary>
    public Type ProviderType { get; }
}

/// <summary>
/// Declares the typed example provider used for an operation request body.
/// </summary>
/// <typeparam name="TProvider">The provider type that creates the request example.</typeparam>
[AttributeUsage(AttributeTargets.Method)]
public sealed class SwaggerRequestExampleAttribute<TProvider> : SwaggerRequestExampleAttribute
    where TProvider : ISwaggerExampleProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SwaggerRequestExampleAttribute{TProvider}"/> class.
    /// </summary>
    public SwaggerRequestExampleAttribute()
        : base(typeof(TProvider))
    {
        // Generic attributes keep controller metadata concise while preserving provider type safety.
    }
}
