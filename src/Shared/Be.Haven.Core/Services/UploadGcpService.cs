namespace Be.Haven.Core.Services;

/// <summary>
/// Provides functionality for uploading files to the GCP upload service.
/// Responsible for preparing the request, serializing the payload,
/// attaching authentication headers, and delegating the call to the third-party API handler.
/// </summary>
public class UploadGcpService : IUploadGcpService
{
    private readonly IThirdPartyApiService _thirdPartyApiService;
    private readonly UploadGcpOptions _uploadGcpOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="UploadGcpService"/> class.
    /// </summary>
    /// <param name="thirdPartyApiService">
    /// Service responsible for executing outbound HTTP requests to third-party APIs.
    /// </param>
    /// <param name="uploadGcpOptions">
    /// Configuration settings containing GCP upload endpoint, base URL, and API key.
    /// </param>
    public UploadGcpService(
        IThirdPartyApiService thirdPartyApiService,
        IOptions<UploadGcpOptions> uploadGcpOptions)
    {
        _thirdPartyApiService = thirdPartyApiService;
        _uploadGcpOptions = uploadGcpOptions.Value;
    }

    /// <summary>
    /// Uploads a file to the configured GCP upload service.
    /// </summary>
    /// <param name="request">
    /// The <see cref="UploadGcpRequest"/> containing the file and optional metadata
    /// such as description, uploader identity, and thumbnail generation flag.
    /// </param>
    /// <returns>
    /// A <see cref="UploadGcpResponse"/> representing the result returned
    /// by the external GCP upload API.
    /// </returns>
    public async Task<UploadGcpResponse> UploadFilesToGcpAsync(UploadGcpRequest request)
    {
        // Prepare the third-party API request wrapper
        var req = new BaseThirdPartyApiRequest
        {
            HttpClientName = UPLOAD_GCP_SERVICE,
            Method = POST,
            BaseUrl = _uploadGcpOptions.BaseUrl,
            Endpoint = _uploadGcpOptions.EndPoints.UploadFile,
            ContentType = MULTIPART_FORM_DATA,
            QueryParameters = new Dictionary<string, string>
            {
                { UPLOAD_GCP_DESCRIPTION, request.Description },
                { UPLOAD_GCP_UPLOAD_BY, request.UploadedBy },
                { UPLOAD_GCP_THUMBNAIL, request.GenerateThumbnail.ToString() },
                { UPLOAD_GCP_RESOURCE_ID, request.ResourceId.ToString() }
            },
            Headers = new Dictionary<string, string>
            {
                { UPLOAD_GCP_API_KEY, _uploadGcpOptions.ApiKey }
            },
            File = request.File
        };

        // Execute the API call and deserialize the response
        var result = await _thirdPartyApiService
            .HandleApiData<BaseThirdPartyApiRequest, UploadGcpResponse>(req);

        return result;
    }

    /// <summary>
    /// Retrieves an image from GCP by its file identifier.
    /// </summary>
    /// <param name="fileId">
    /// The unique identifier of the file to be retrieved.
    /// </param>
    /// <returns>
    /// A <see cref="GetImageByFileIdResponse"/> containing the image data,
    /// metadata, and any related retrieval information.
    /// </returns>
    public async Task<GetImageByFileIdResponse> GetImageByFileIdAsync(Guid fileId)
    {
        // Prepare the third-party API request for retrieving image by fileId.
        var req = new BaseThirdPartyApiRequest
        {
            HttpClientName = UPLOAD_GCP_GET_IMAGE_BY_FILE_ID,
            Method = GET,
            BaseUrl = _uploadGcpOptions.BaseUrl,
            Endpoint = string.Format(_uploadGcpOptions.EndPoints.GetFileById, fileId),
            ContentType = TEXT_JSON,
            Headers = new Dictionary<string, string>
            {
                { UPLOAD_GCP_API_KEY, _uploadGcpOptions.ApiKey }
            },
        };

        // Execute the request through the shared API handler, which performs:
        // - HTTP execution
        // - Error handling
        // - JSON deserialization into GetImageByFileIdResponse
        var result = await _thirdPartyApiService
            .HandleApiData<BaseThirdPartyApiRequest, GetImageByFileIdResponse>(req);

        return result;
    }
}
