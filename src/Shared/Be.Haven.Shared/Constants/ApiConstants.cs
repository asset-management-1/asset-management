namespace Be.Haven.Shared.Constants;

/// <summary>
/// Contains constant values used throughout the application for API-related configurations
/// such as versioning and other related identifiers.
/// </summary>
public static class ApiConstants
{
    /// <summary>
    /// Represents the version "1.0" of the API, intended for use in identifying or interacting
    /// with the initial version of the application's API structure.
    /// </summary>
    public const string API_VERSION_1 = "1.0";

    /// <summary>
    /// Represents the version "2.0" of the API, intended for use in identifying or interacting
    /// with the second version of the application's API structure.
    /// </summary>
    public const string API_VERSION_2 = "2.0";
    
    /// <summary>
    /// The route segment used as a prefix for Swagger API documentation endpoints.
    /// </summary>
    public const string SWAGGER_PATH_PREFIX = "/swagger";

    /// <summary>
    /// A constant character representing the lowercase prefix ('v') used to indicate
    /// </summary>
    public const char VERSION_CHAR_LOWER = 'v';

    /// <summary>
    /// A constant character representing the uppercase prefix ('V') used to indicate
    /// versioning in API routes or configurations.
    /// </summary>
    public const char VERSION_CHAR_UPPER = 'V';
    
    /// <summary>
    /// Contains constant values used for configuring Swagger documentation 
    /// in the Haven Portal API.
    /// </summary>
    public static class SwaggerConstants
    {
        /// <summary>
        /// The URL where the generated Swagger JSON can be accessed.
        /// </summary>
        public const string ENDPOINT_URL = "/swagger/{0}/swagger.json";

        /// <summary>
        /// The display name of the Swagger endpoint.
        /// </summary>
        public const string ENDPOINT_NAME = "Haven Portal API v{0}";

        /// <summary>
        /// The route prefix for serving the Swagger UI.
        /// An empty string means the UI is served at the application root.
        /// </summary>
        public const string ROUTE_PREFIX = "";

        /// <summary>
        /// Format string for generating XML documentation file names.
        /// </summary>
        public const string XML_DOC_FILE_FORMAT = "{0}.xml";

        /// <summary>
        /// The title displayed in the Swagger UI and documentation.
        /// </summary>
        public const string TITLE = "Haven Portal API";

        /// <summary>
        /// A short description of the API for Swagger documentation.
        /// </summary>
        public const string DESCRIPTION = "API documentation for the Haven Portal application.";

        /// <summary>
        /// Instructions for authenticating in Swagger UI using Bearer tokens.
        /// </summary>
        public const string AUTH_DESCRIPTION = "Use Authorization: Bearer <token>. Enter the JWT only—Swagger UI adds 'Bearer ' automatically. If disabled, include 'Bearer ' yourself.";

        /// <summary>
        /// Contains contact information for the API support team.
        /// </summary>
        public static class Contact
        {
            /// <summary>
            /// The name of the contact person or team for API support.
            /// </summary>
            public const string NAME = "Support Team";

            /// <summary>
            /// The email address for contacting API support.
            /// </summary>
            public const string EMAIL = "support@example.com";

            /// <summary>
            /// The URL with more details or support resources for the API.
            /// </summary>
            public const string URL = "https://example.com/support";
        }

        /// <summary>
        /// Contains licensing information for the API.
        /// </summary>
        public static class License
        {
            /// <summary>
            /// The name of the license under which the API is published.
            /// </summary>
            public const string NAME = "MIT";

            /// <summary>
            /// The URL pointing to the license details.
            /// </summary>
            public const string URL = "https://opensource.org/licenses/MIT";
        }
    }
}
