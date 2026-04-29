namespace Haven.Core.Interfaces.Services;

public interface IJsonSerializerService
{
    /// <summary>
    /// Deserializes the specified JSON string into an object of type T.
    /// </summary>
    T Deserialize<T>(string json);

    /// <summary>
    /// Deserializes the JSON string into the specified type, ignore custom property name mappings such as JsonProperty attributes.
    /// </summary>
    T DeserializeIgnoreProperties<T>(string json);

    /// <summary>
    /// Deserializes the specified JSON string into an object of type <typeparamref name="T"/>.
    /// This method ensures that PascalCase property names are retained during deserialization.
    /// </summary>
    /// <typeparam name="T">The type of object to deserialize the JSON into.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>An object of type <typeparamref name="T"/> with PascalCase property names.</returns>
    T DeserializeIgnoreToPascalProperties<T>(string json);

    /// <summary>
    /// Serializes the specified object into a JSON string.
    /// </summary>
    string Serialize<T>(T obj);

    /// <summary>
    /// Serializes the specified object into a JSON Pascal string.
    /// </summary>
    string SerializeIgnoreToPascalProperties<T>(T obj);

    /// <summary>
    /// Validates a JSON string and deserializes it into an object of type <typeparamref name="T"/>.
    /// If deserialization fails, returns a <see cref="GenericResponse{T}"/> containing error information.
    /// </summary>
    /// <typeparam name="T">The type to deserialize into.</typeparam>
    /// <param name="jsonString">The JSON string input to be validated and deserialized.</param>
    /// <returns>A <see cref="GenericResponse{T}"/> instance, containing either the deserialized object or error details.</returns>
    GenericResponse<T> ValidateAndDeserialize<T>(string jsonString);
}