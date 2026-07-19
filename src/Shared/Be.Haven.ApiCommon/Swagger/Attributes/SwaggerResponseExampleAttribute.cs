namespace Be.Haven.ApiCommon.Swagger.Attributes;

/// <summary>
/// Declares the example provider used for an operation response status code.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class SwaggerResponseExampleAttribute : Attribute
{
    /// <summary>
    /// Initialises a new instance of the <see cref="SwaggerResponseExampleAttribute"/> class.
    /// </summary>
    /// <param name="statusCode">The HTTP response status code that receives the example.</param>
    /// <param name="providerType">The provider type that creates the response example.</param>
    public SwaggerResponseExampleAttribute(int statusCode, Type providerType)
    {
        StatusCode = statusCode;
        ProviderType = providerType;
    }

    /// <summary>
    /// Gets the HTTP response status code that receives the example.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Gets the provider type that creates the response example.
    /// </summary>
    public Type ProviderType { get; }

    /// <summary>
    /// Gets or sets the optional OpenAPI example name used when one status code has multiple valid response shapes.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the optional short label displayed for a named response example.
    /// </summary>
    public string Summary { get; set; }

    /// <summary>
    /// Gets or sets the optional explanation displayed for a named response example.
    /// </summary>
    public string Description { get; set; }
}

/// <summary>
/// Declares the typed example provider used for an operation response status code.
/// </summary>
/// <typeparam name="TProvider">The provider type that creates the response example.</typeparam>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class SwaggerResponseExampleAttribute<TProvider> : SwaggerResponseExampleAttribute
    where TProvider : ISwaggerExampleProvider
{
    /// <summary>
    /// Initialises a new instance of the <see cref="SwaggerResponseExampleAttribute{TProvider}"/> class.
    /// </summary>
    /// <param name="statusCode">The HTTP response status code that receives the example.</param>
    public SwaggerResponseExampleAttribute(int statusCode)
        : base(statusCode, typeof(TProvider))
    {
        // Generic attributes keep controller metadata concise while preserving provider type safety.
    }
}
