namespace Be.Haven.ApiCommon.Swagger.Attributes;

/// <summary>
/// Declares the example provider used for an operation response status code.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class SwaggerResponseExampleAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SwaggerResponseExampleAttribute"/> class.
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
    /// Initializes a new instance of the <see cref="SwaggerResponseExampleAttribute{TProvider}"/> class.
    /// </summary>
    /// <param name="statusCode">The HTTP response status code that receives the example.</param>
    public SwaggerResponseExampleAttribute(int statusCode)
        : base(statusCode, typeof(TProvider))
    {
        // Generic attributes keep controller metadata concise while preserving provider type safety.
    }
}
