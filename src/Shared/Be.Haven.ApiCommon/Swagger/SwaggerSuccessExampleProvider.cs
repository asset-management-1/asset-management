namespace Be.Haven.ApiCommon.Swagger;

/// <summary>
/// Provides a typed base for standardized success-envelope Swagger examples.
/// </summary>
/// <typeparam name="TData">The response data payload type.</typeparam>
public abstract class SwaggerSuccessExampleProvider<TData> : SwaggerExampleProvider<ResponseDto<TData>>
{
    /// <summary>
    /// Builds the standardized success response envelope.
    /// </summary>
    /// <returns>The typed success response example.</returns>
    protected override ResponseDto<TData> BuildExample()
    {
        // Every documented success response uses the same ResponseDto envelope and example metadata.
        return SwaggerStandardExampleFactory.BuildSuccessEnvelope(BuildData());
    }

    /// <summary>
    /// Builds the response data payload inside the success envelope.
    /// </summary>
    /// <returns>The response data payload example.</returns>
    protected abstract TData BuildData();
}
