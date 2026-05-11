namespace Be.Haven.ApiCommon.Constants;

/// <summary>
/// Defines shared Swagger UI asset constants used by all Haven API services.
/// </summary>
public static class SwaggerUiConstants
{
    /// <summary>
    /// Request path that serves the shared Swagger operation-search script.
    /// </summary>
    public const string OPERATION_SEARCH_SCRIPT_PATH = "/swagger-assets/haven-operation-search.js";

    /// <summary>
    /// Embedded resource name for the shared Swagger operation-search script.
    /// </summary>
    public const string OPERATION_SEARCH_SCRIPT_RESOURCE_NAME =
        "Be.Haven.ApiCommon.Swagger.Assets.haven-operation-search.js";

    /// <summary>
    /// Content type returned for the shared Swagger operation-search script.
    /// </summary>
    public const string JAVASCRIPT_CONTENT_TYPE = "application/javascript; charset=utf-8";

    /// <summary>
    /// Error message used when the embedded Swagger operation-search script cannot be loaded.
    /// </summary>
    public const string OPERATION_SEARCH_SCRIPT_RESOURCE_NOT_FOUND =
        "Embedded Swagger operation-search script resource was not found: {0}.";
}
