namespace Be.Haven.ApiCommon.Swagger;

/// <summary>
/// Provides an OpenAPI example object for Swagger request or response documentation.
/// </summary>
public interface ISwaggerExampleProvider
{
    /// <summary>
    /// Builds the example object that will be serialized into the OpenAPI document.
    /// </summary>
    /// <returns>The request or response example object.</returns>
    object GetExample();
}
