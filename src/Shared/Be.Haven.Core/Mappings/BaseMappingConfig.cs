namespace Be.Haven.Core.Mappings;

/// <summary>
/// Represents the configuration for mapping between the ApiRequest and BaseHttpRequest models.
/// </summary>
public class BaseMappingConfig : IRegister
{
    /// <summary>
    /// Registers the mapping rules between ApiRequest and BaseHttpRequest.
    /// </summary>
    /// <param name="config">The Mapster TypeAdapterConfig instance used to define mappings.</param>
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<BaseThirdPartyApiRequest, BaseHttpRequest>()
            .Ignore(dest => dest.File)
            .Ignore(dest => dest.RequestStream)

            // Maps ApiRequest.Endpoint to BaseHttpRequest.RequestUri
            .Map(dest => dest.RequestUri, src => src.Endpoint)

            // Maps ApiRequest.Content to BaseHttpRequest.RequestData
            .Map(dest => dest.RequestData, src => src.Content)

            // Maps ApiRequest.FormData to BaseHttpRequest.RequestFormData
            .Map(dest => dest.RequestFormData, src => src.FormData)

            // Direct mapping for AuthenticationValue property (same name and meaning)
            .Map(dest => dest.AuthenticationValue, src => src.AuthenticationValue)

            // Maps ApiRequest.BaseUrl to BaseHttpRequest.BaseAddress
            .Map(dest => dest.BaseAddress, src => src.BaseUrl)

            // Direct mapping for ContentType property (same name and meaning)
            .Map(dest => dest.ContentType, src => src.ContentType)

            // Maps ApiRequest.Headers to BaseHttpRequest.AdditionalHeaders
            .Map(dest => dest.AdditionalHeaders, src => src.Headers)

            // Maps ApiRequest.QueryParameters to BaseHttpRequest.AdditionalQueryParams
            .Map(dest => dest.AdditionalQueryParams, src => src.QueryParameters);
    }
}
