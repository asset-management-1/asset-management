namespace Authentication.Api.Filters;

/// <summary>
/// Adds required client-device header examples to token-issuing Swagger operations.
/// </summary>
public sealed class ClientDeviceInfoSwaggerOperationFilter : IOperationFilter
{
    /// <summary>
    /// Device header examples displayed for endpoints that require client-device metadata.
    /// </summary>
    private static readonly IReadOnlyList<(string Name, string Description, string Example)> DeviceHeaderExamples =
    [
        (
            ClientDeviceHeaders.DEVICE_ID,
            "Stable client instance id for one app/browser install.",
            AuthenticationSwaggerExampleConstants.EXAMPLE_DEVICE_ID),
        (
            ClientDeviceHeaders.DEVICE_NAME,
            "Client display name for the current app/browser instance.",
            AuthenticationSwaggerExampleConstants.EXAMPLE_DEVICE_NAME),
        (
            ClientDeviceHeaders.DEVICE_TYPE,
            "Client device category, such as web, ios, android, or desktop.",
            AuthenticationSwaggerExampleConstants.EXAMPLE_DEVICE_TYPE),
        (
            ClientDeviceHeaders.USER_AGENT,
            "HTTP user-agent observed for the current request.",
            AuthenticationSwaggerExampleConstants.EXAMPLE_USER_AGENT)
    ];

    /// <summary>
    /// Applies required device-header parameters to operations decorated with <see cref="RequireClientDeviceInfoAttribute"/>.
    /// </summary>
    /// <param name="operation">The OpenAPI operation being generated.</param>
    /// <param name="context">The Swagger operation-generation context.</param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!RequiresClientDeviceInfo(context))
        {
            return;
        }

        // Token-issuing endpoints enforce these headers at runtime through RequireClientDeviceInfoAttribute.
        operation.Parameters ??= new List<IOpenApiParameter>();
        foreach (var header in DeviceHeaderExamples)
        {
            UpsertDeviceHeader(operation, header);
        }
    }

    /// <summary>
    /// Determines whether the current action requires client-device metadata.
    /// </summary>
    /// <param name="context">The Swagger operation-generation context.</param>
    /// <returns><c>true</c> when the action or controller has the device-info attribute; otherwise <c>false</c>.</returns>
    private static bool RequiresClientDeviceInfo(OperationFilterContext context)
    {
        // Support method-level use today and controller-level use if the API ever moves the attribute upward.
        return context.MethodInfo.IsDefined(typeof(RequireClientDeviceInfoAttribute), inherit: true)
               || context.MethodInfo.DeclaringType?.IsDefined(typeof(RequireClientDeviceInfoAttribute), inherit: true) == true;
    }

    /// <summary>
    /// Adds or updates one required client-device header parameter.
    /// </summary>
    /// <param name="operation">The OpenAPI operation being generated.</param>
    /// <param name="header">The header metadata and example value.</param>
    private static void UpsertDeviceHeader(
        OpenApiOperation operation,
        (string Name, string Description, string Example) header)
    {
        var parameter = operation.Parameters!
                                 .OfType<OpenApiParameter>()
                                 .FirstOrDefault(x => x.In == ParameterLocation.Header
                                                      && string.Equals(x.Name, header.Name, StringComparison.OrdinalIgnoreCase));
        if (parameter is null)
        {
            parameter = new OpenApiParameter
            {
                Name = header.Name,
                In = ParameterLocation.Header
            };
            operation.Parameters.Add(parameter);
        }

        // Keep Swagger Try-it-out aligned with runtime requirements and FE header examples.
        parameter.Required = true;
        parameter.Description = header.Description;
        parameter.Schema ??= new OpenApiSchema { Type = JsonSchemaType.String };
        parameter.Example = JsonValue.Create(header.Example);
    }
}
