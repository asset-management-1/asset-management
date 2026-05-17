namespace Be.Haven.ApiCommon.Swagger.Attributes;

/// <summary>
/// Declares value examples for operation parameters or request-body fields.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class SwaggerValueExampleAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SwaggerValueExampleAttribute"/> class.
    /// </summary>
    /// <param name="providerType">The provider type that creates one or more field examples.</param>
    public SwaggerValueExampleAttribute(Type providerType)
        : this(null, providerType)
    {
        // A provider without a target name is treated as an object whose properties map to fields.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SwaggerValueExampleAttribute"/> class.
    /// </summary>
    /// <param name="name">The parameter or request-field name that receives the value example.</param>
    /// <param name="providerType">The provider type that creates the value example.</param>
    public SwaggerValueExampleAttribute(string name, Type providerType)
    {
        Name = name;
        ProviderType = providerType;
    }

    /// <summary>
    /// Gets the parameter or request-field name that receives the value example.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the provider type that creates the value example.
    /// </summary>
    public Type ProviderType { get; }
}

/// <summary>
/// Declares a typed value-example provider for operation parameters or request-body fields.
/// </summary>
/// <typeparam name="TProvider">The provider type that creates the value example.</typeparam>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class SwaggerValueExampleAttribute<TProvider> : SwaggerValueExampleAttribute
    where TProvider : ISwaggerExampleProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SwaggerValueExampleAttribute{TProvider}"/> class.
    /// </summary>
    public SwaggerValueExampleAttribute()
        : base(typeof(TProvider))
    {
        // The provider object supplies field names and values for body or form examples.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SwaggerValueExampleAttribute{TProvider}"/> class.
    /// </summary>
    /// <param name="name">The parameter or request-field name that receives the value example.</param>
    public SwaggerValueExampleAttribute(string name)
        : base(name, typeof(TProvider))
    {
        // The provider value is applied directly to the named parameter or request field.
    }
}
