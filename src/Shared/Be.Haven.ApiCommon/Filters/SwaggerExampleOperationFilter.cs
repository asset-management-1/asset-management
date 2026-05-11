namespace Be.Haven.ApiCommon.Filters;

/// <summary>
/// Applies request and response examples declared on controller actions to Swagger operations.
/// </summary>
public sealed class SwaggerExampleOperationFilter : IOperationFilter
{
    /// <summary>
    /// JSON serializer settings used to convert examples to OpenAPI-compatible JSON nodes.
    /// </summary>
    private static readonly JsonSerializerOptions ExampleSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Applies example metadata to an OpenAPI operation.
    /// </summary>
    /// <param name="operation">The operation being generated.</param>
    /// <param name="context">The Swagger operation-generation context.</param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Request examples are opt-in because not every endpoint has a body.
        var requestExample = context.MethodInfo.GetCustomAttribute<SwaggerRequestExampleAttribute>();
        if (requestExample is not null)
        {
            ApplyRequestExample(operation, requestExample.ProviderType);
        }

        // Error examples are shared by HTTP status code, then action-specific success examples are overlaid.
        ApplyStandardErrorExamples(operation);

        foreach (var responseExample in context.MethodInfo.GetCustomAttributes<SwaggerResponseExampleAttribute>())
        {
            ApplyResponseExample(operation, responseExample.StatusCode, responseExample.ProviderType);
        }

        // Field-level examples fill Try-it-out controls for path/query values and multipart form fields.
        foreach (var valueExample in context.MethodInfo.GetCustomAttributes<SwaggerValueExampleAttribute>())
        {
            ApplyValueExample(operation, valueExample);
        }
    }

    /// <summary>
    /// Applies a request-body example to all generated request content types.
    /// </summary>
    /// <param name="operation">The operation being generated.</param>
    /// <param name="providerType">The provider type that creates the example object.</param>
    private static void ApplyRequestExample(OpenApiOperation operation, Type providerType)
    {
        var requestContent = operation.RequestBody?.Content;
        if (requestContent is null)
        {
            return;
        }

        // Reuse the same example across JSON and multipart content entries when Swagger generated both.
        var example = BuildExampleNode(providerType);
        foreach (var mediaType in requestContent.Values)
        {
            mediaType.Example ??= example?.DeepClone();
        }
    }

    /// <summary>
    /// Applies a field-level example provider to parameters or request-body fields.
    /// </summary>
    /// <param name="operation">The operation being generated.</param>
    /// <param name="valueExample">The configured field-level example metadata.</param>
    private static void ApplyValueExample(OpenApiOperation operation, SwaggerValueExampleAttribute valueExample)
    {
        // Named providers target one parameter or field; object providers map each property by name.
        var example = BuildExampleNode(valueExample.ProviderType);
        if (!string.IsNullOrWhiteSpace(valueExample.Name))
        {
            ApplyValueExample(operation, valueExample.Name, example);
            return;
        }

        if (example is not JsonObject objectExample)
        {
            return;
        }

        foreach (var item in objectExample)
        {
            ApplyValueExample(operation, item.Key, item.Value);
        }
    }

    /// <summary>
    /// Applies one JSON value example to a matching parameter or request-body field.
    /// </summary>
    /// <param name="operation">The operation being generated.</param>
    /// <param name="name">The parameter or request-field name that receives the example.</param>
    /// <param name="example">The serialized example value.</param>
    private static void ApplyValueExample(OpenApiOperation operation, string name, JsonNode example)
    {
        // Parameters and request-body fields are handled separately because Swagger stores them in different model nodes.
        ApplyParameterValueExample(operation, name, example);
        ApplyRequestBodyValueExample(operation, name, example);
    }

    /// <summary>
    /// Applies one JSON value example to a matching OpenAPI parameter.
    /// </summary>
    /// <param name="operation">The operation being generated.</param>
    /// <param name="name">The parameter name to match.</param>
    /// <param name="example">The serialized example value.</param>
    private static void ApplyParameterValueExample(OpenApiOperation operation, string name, JsonNode example)
    {
        var parameters = operation.Parameters;
        if (parameters is null)
        {
            return;
        }

        // Match parameter names after normalizing separators so kebab-case routes can use CLR property names in attributes.
        var parameter = parameters
            .OfType<OpenApiParameter>()
            .FirstOrDefault(x => NamesMatch(x.Name, name));
        if (parameter is null)
        {
            return;
        }

        parameter.Example = example?.DeepClone();
    }

    /// <summary>
    /// Applies one JSON value example to matching request-body schema properties.
    /// </summary>
    /// <param name="operation">The operation being generated.</param>
    /// <param name="name">The request-field name to match.</param>
    /// <param name="example">The serialized example value.</param>
    private static void ApplyRequestBodyValueExample(OpenApiOperation operation, string name, JsonNode example)
    {
        var requestContent = operation.RequestBody?.Content;
        if (requestContent is null)
        {
            return;
        }

        // Each content type owns its own schema property nodes, so clone the value per match.
        foreach (var mediaType in requestContent.Values)
        {
            if (TryGetSchemaProperty(mediaType.Schema, name, out var schemaProperty))
            {
                schemaProperty.Example = example?.DeepClone();
            }
        }
    }

    /// <summary>
    /// Finds a request-body schema property by exact or case-insensitive name.
    /// </summary>
    /// <param name="schema">The request-body schema to inspect.</param>
    /// <param name="name">The schema property name to match.</param>
    /// <param name="schemaProperty">The matched schema property when found.</param>
    /// <returns><c>true</c> when a matching schema property is found; otherwise <c>false</c>.</returns>
    private static bool TryGetSchemaProperty(
        IOpenApiSchema schema,
        string name,
        out OpenApiSchema schemaProperty)
    {
        schemaProperty = null;
        var properties = schema?.Properties;
        if (properties is null)
        {
            return false;
        }

        if (properties.TryGetValue(name, out var exactProperty)
            && exactProperty is OpenApiSchema exactSchemaProperty)
        {
            schemaProperty = exactSchemaProperty;
            return true;
        }

        // Swagger may use CLR PascalCase for multipart forms, camelCase for JSON, or kebab-case for route-transformed names.
        var property = properties.FirstOrDefault(x =>
            NamesMatch(x.Key, name));
        schemaProperty = property.Value as OpenApiSchema;
        return schemaProperty is not null;
    }

    /// <summary>
    /// Compares OpenAPI field names while ignoring casing and common separators.
    /// </summary>
    /// <param name="left">The first parameter or schema-property name.</param>
    /// <param name="right">The second parameter or schema-property name.</param>
    /// <returns><c>true</c> when both names represent the same field; otherwise <c>false</c>.</returns>
    private static bool NamesMatch(string left, string right)
    {
        if (string.Equals(left, right, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Kebab-case route parameters should still match CLR property names supplied through nameof(...).
        return string.Equals(
            NormalizeOpenApiName(left),
            NormalizeOpenApiName(right),
            StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Removes separators from an OpenAPI field name for tolerant documentation-example matching.
    /// </summary>
    /// <param name="name">The OpenAPI parameter or schema-property name.</param>
    /// <returns>The normalized name used only for example matching.</returns>
    private static string NormalizeOpenApiName(string name)
    {
        return string.IsNullOrWhiteSpace(name)
            ? string.Empty
            : new string(name.Where(char.IsLetterOrDigit).ToArray());
    }

    /// <summary>
    /// Applies the shared error example for each documented error status code.
    /// </summary>
    /// <param name="operation">The operation being generated.</param>
    private static void ApplyStandardErrorExamples(OpenApiOperation operation)
    {
        var responses = operation.Responses;
        if (responses is null || responses.Count == 0)
        {
            return;
        }

        foreach (var response in responses)
        {
            if (!int.TryParse(response.Key, out var statusCode) || statusCode < StatusCodes.Status400BadRequest)
            {
                continue;
            }

            // Only documented error responses receive the shared response-envelope example.
            ApplyResponseExample(
                operation,
                statusCode,
                SwaggerStandardExampleFactory.Error(statusCode));
        }
    }

    /// <summary>
    /// Applies a provider-backed response example for one status code.
    /// </summary>
    /// <param name="operation">The operation being generated.</param>
    /// <param name="statusCode">The response status code that receives the example.</param>
    /// <param name="providerType">The provider type that creates the example object.</param>
    private static void ApplyResponseExample(OpenApiOperation operation, int statusCode, Type providerType)
    {
        // Action-specific examples are provider-backed so each success payload can stay close to its API module.
        ApplyResponseExample(operation, statusCode, BuildExampleNode(providerType));
    }

    /// <summary>
    /// Applies an object-backed response example for one status code.
    /// </summary>
    /// <param name="operation">The operation being generated.</param>
    /// <param name="statusCode">The response status code that receives the example.</param>
    /// <param name="example">The response example object.</param>
    private static void ApplyResponseExample(OpenApiOperation operation, int statusCode, object example)
    {
        // Shared standard examples are already materialized and only need JSON conversion.
        ApplyResponseExample(operation, statusCode, BuildExampleNode(example));
    }

    /// <summary>
    /// Applies a JSON node response example for one status code.
    /// </summary>
    /// <param name="operation">The operation being generated.</param>
    /// <param name="statusCode">The response status code that receives the example.</param>
    /// <param name="example">The response example node.</param>
    private static void ApplyResponseExample(OpenApiOperation operation, int statusCode, JsonNode example)
    {
        var responses = operation.Responses;
        if (responses is null
            || !responses.TryGetValue(statusCode.ToString(CultureInfo.InvariantCulture), out var response))
        {
            return;
        }

        var responseContent = response.Content;
        if (responseContent is null)
        {
            return;
        }

        // Swagger's OpenAPI model owns JsonNode instances, so each media type receives an independent clone.
        foreach (var mediaType in responseContent.Values)
        {
            mediaType.Example = example?.DeepClone();
        }
    }

    /// <summary>
    /// Builds a JSON node from an example provider type.
    /// </summary>
    /// <param name="providerType">The provider type that creates the example object.</param>
    /// <returns>The serialized example JSON node.</returns>
    private static JsonNode BuildExampleNode(Type providerType)
    {
        var providerTypeName = providerType.FullName ?? providerType.Name;

        // Provider type errors should surface during Swagger generation because they indicate bad API docs wiring.
        if (!typeof(ISwaggerExampleProvider).IsAssignableFrom(providerType))
        {
            throw new InvalidOperationException(string.Format(
                CultureInfo.InvariantCulture,
                SWAGGER_EXAMPLE_PROVIDER_INTERFACE_REQUIRED_FORMAT,
                providerTypeName,
                nameof(ISwaggerExampleProvider)));
        }

        if (Activator.CreateInstance(providerType, nonPublic: true) is not ISwaggerExampleProvider provider)
        {
            throw new InvalidOperationException(string.Format(
                CultureInfo.InvariantCulture,
                SWAGGER_EXAMPLE_PROVIDER_CREATION_FAILED_FORMAT,
                providerTypeName,
                nameof(ISwaggerExampleProvider)));
        }

        return BuildExampleNode(provider.GetExample());
    }

    /// <summary>
    /// Builds a JSON node from an example object.
    /// </summary>
    /// <param name="example">The example object to serialize.</param>
    /// <returns>The serialized example JSON node.</returns>
    private static JsonNode BuildExampleNode(object example)
    {
        // Serialize through System.Text.Json so examples match the API's camelCase/string-enum behavior.
        var json = System.Text.Json.JsonSerializer.Serialize(example, ExampleSerializerOptions);
        return JsonNode.Parse(json)
               ?? throw new InvalidOperationException(SWAGGER_EXAMPLE_JSON_NODE_REQUIRED);
    }
}
