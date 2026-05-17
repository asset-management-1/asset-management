namespace Be.Haven.Core.Services;

public class JsonSerializerService : IJsonSerializerService
{
    // Standard camelCase serializer settings (default use)
    private readonly JsonSerializerSettings _jsonSerializerSettings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore,
        MissingMemberHandling = MissingMemberHandling.Ignore
    };

    // Custom settings to ignore JsonProperty and resolve original property names
    private readonly JsonSerializerSettings _ignorePropertySettings = new()
    {
        ContractResolver = new IgnorePropertyContractResolver(),
        NullValueHandling = NullValueHandling.Ignore,
        MissingMemberHandling = MissingMemberHandling.Ignore
    };

    // Settings to retain PascalCase property names (no naming strategy)
    private readonly JsonSerializerSettings _jsonSerializerToPascalSettings = new()
    {
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = null
        },
        NullValueHandling = NullValueHandling.Ignore,
        MissingMemberHandling = MissingMemberHandling.Ignore
    };

    /// <summary>
    /// Deserializes a JSON string into an object of type <typeparamref name="T"/> using camelCase mapping.
    /// </summary>
    /// <typeparam name="T">The type to deserialize into.</typeparam>
    /// <param name="json">The JSON string input.</param>
    /// <returns>The deserialized object of type <typeparamref name="T"/>.</returns>
    public T Deserialize<T>(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return default;
        }

        return JsonConvert.DeserializeObject<T>(json, _jsonSerializerSettings);
    }

    /// <summary>
    /// Validates a JSON string and deserializes it into an object of type <typeparamref name="T"/>
    /// If deserialization fails, returns a <see cref="GenericResponse{T}"/> containing error information.
    /// </summary>
    /// <typeparam name="T">The type to deserialize into.</typeparam>
    /// <param name="jsonString">The JSON string input to be validated and deserialized.</param>
    /// <returns>A <see cref="GenericResponse{T}"/> instance, containing either the deserialized object or error details.</returns>
    public GenericResponse<T> ValidateAndDeserialize<T>(string jsonString)
    {
        try
        {
            var deserializedData = JsonConvert.DeserializeObject<T>(jsonString, _jsonSerializerSettings);
            return new GenericResponse<T>(deserializedData);
        }

        catch (Exception ex)
        {
            return new GenericResponse<T>(errorMessage: string.Format(INVALID_JSON_FORMAT_MESSAGE, ex.Message, jsonString));
        }
    }

    /// <summary>
    /// Deserializes a JSON string into an object of type <typeparamref name="T"/>,
    /// ignoring attributes like [JsonProperty] and using original C# property names.
    /// </summary>
    /// <typeparam name="T">The type to deserialize into.</typeparam>
    /// <param name="json">The JSON string input.</param>
    /// <returns>The deserialized object of type <typeparamref name="T"/>.</returns>
    public T DeserializeIgnoreProperties<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json, _ignorePropertySettings);
    }

    /// <summary>
    /// Deserializes a JSON string into an object of type <typeparamref name="T"/> while retaining PascalCase property names.
    /// </summary>
    /// <typeparam name="T">The type to deserialize into.</typeparam>
    /// <param name="json">The JSON string input.</param>
    /// <returns>The deserialized object of type <typeparamref name="T"/>.</returns>
    public T DeserializeIgnoreToPascalProperties<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json, _jsonSerializerToPascalSettings);
    }

    /// <summary>
    /// Deserializes a JSON array into a typed list while retaining PascalCase property names.
    /// </summary>
    /// <typeparam name="T">The list element type.</typeparam>
    /// <param name="json">The JSON array text.</param>
    /// <returns>The deserialized list, or an empty list when the payload is blank.</returns>
    public List<T> DeserializeList<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        return JsonConvert.DeserializeObject<List<T>>(json, _jsonSerializerToPascalSettings) ?? [];
    }

    /// <summary>
    /// Serializes an object into a JSON string using camelCase naming strategy.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>A JSON string representation of the object.</returns>
    public string Serialize<T>(T obj)
    {
        if (obj is null)
        {
            return null;
        }

        return JsonConvert.SerializeObject(obj, _jsonSerializerSettings);
    }

    /// <summary>
    /// Serializes an object into a JSON string while preserving PascalCase property names.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>A JSON string with PascalCase property names.</returns>
    public string SerializeIgnoreToPascalProperties<T>(T obj)
    {
        return JsonConvert.SerializeObject(obj, _jsonSerializerToPascalSettings);
    }
}
