namespace Be.Haven.Shared.Constants;

public static class AppConstants
{
    public static class SystemVariable
    {
        /// <summary>
        /// Represents the prefix used for identifying cache connection keys in the system.
        /// </summary>
        public const string CACHE_CONNECTION_KEY_PREFIX = "db_connection_string";
        
        /// <summary>
        /// Specifies the default duration that the system waits to acquire a lock before timing out.
        /// This value is used as a fallback when no custom lock wait timeout is configured.
        /// </summary>
        public static readonly TimeSpan DEFAULT_LOCK_WAIT_TIMEOUT = TimeSpan.Zero;

        /// <summary>
        /// Represents the default interval that the system waits before reattempting to acquire a lock.
        /// This value is used as a fallback when no custom lock poll delay is configured.
        /// </summary>
        public static readonly TimeSpan DEFAULT_LOCK_POLL_DELAY = TimeSpan.FromSeconds(2);
        
        /// <summary>
        /// Represents the default version value used as a fallback when no specific version is found or when a version-related value is invalid.
        /// </summary>
        public const long DEFAULT_VERSION = 1;

        /// <summary>
        /// Specifies the maximum number of retry attempts allowed when writing cached data fails.
        /// </summary>
        public const int MAX_WRITE_CACHED_RETRIES = 5;
        
        /// <summary>
        /// Constant representing the authentication scheme for National Address Bearer tokens.
        /// </summary>
        public const string HAVEN_BEARER = "HavenBearer";

        /// <summary>
        /// Constant representing the identifier for background jobs in the system.
        /// </summary>
        public const string BACKGROUND_JOBS = "BackgroundJobs";

        /// <summary>
        /// Prefix used for constructing version-specific cache keys.
        /// </summary>
        public const string VERSION_KEY_PREFIX = "ver:";

        /// <summary>
        /// Format used to build a user-scoped cache namespace.
        /// Usage: string.Format(USER_CACHE_SCOPE_FORMAT, userPublicId)
        /// </summary>
        public const string USER_CACHE_SCOPE_FORMAT = "user:{0}";

        /// <summary>
        /// Sentinel scope value used when a cacheable request should resolve to the current authenticated user at runtime.
        /// </summary>
        public const string CURRENT_USER_CACHE_SCOPE = "__current_user__";

        /// <summary>
        /// Format used to prepend a cache scope segment.
        /// Usage: string.Format(CACHE_SCOPE_SEGMENT_FORMAT, cacheScope)
        /// </summary>
        public const string CACHE_SCOPE_SEGMENT_FORMAT = ":{0}";
        
        /// <summary>
        /// Represents the slash ("/") character used as a constant in system variables.
        /// </summary>
        public const string SLASH = "/";

        /// <summary>
        /// Constant representing the symbol used to separate values or components within a string.
        /// </summary>
        public const string PIPE_SEPARATOR = "|";

        /// <summary>
        /// Represents the character used as a separator for values in a comma-delimited format.
        /// </summary>
        public const string COMMA_SEPARATOR = ",";

        /// <summary>
        /// Constant representing a separator used for single-line formatting purposes.
        /// </summary>
        public const string ONE_LINE_SEPARATOR = " | ";
        
        /// <summary>
        /// Constant representing an underscore character ("_") used as a separator or delimiter in string operations or configurations.
        /// </summary>
        public const string UNDER_SCORE = "_";

        /// <summary>
        /// Regex pattern that splits between a consecutive UPPERCASE block and the next
        /// </summary>
        public const string ACRONYM_BOUNDARY_PATTERN = @"([A-Z]+)([A-Z][a-z])";

        /// <summary>
        /// Defines the timeout duration for regular expression operations to prevent excessive execution time.
        /// </summary>
        public static readonly TimeSpan REGEX_TIMEOUT = TimeSpan.FromSeconds(3);

        /// <summary>
        /// Regex pattern that splits between a lowercase/number and an Uppercase letter.
        /// </summary>
        public const string LOWER_UPPER_BOUNDARY_PATTERN = @"([a-z0-9])([A-Z])";

        /// <summary>
        /// Regular expression pattern that matches any character that is not a letter (A-Z, a-z) or digit (0-9).
        /// Useful for detecting or removing special characters from a string.
        /// Example: This pattern will match '@', '#', '!', etc.
        /// </summary>
        public const string REGEX_SPECIAL_CHARACTER_PATTERN = @"[^A-Za-z0-9]";

        /// <summary>
        /// Replacement text for Regex.Replace using two capture groups: "$1-$2"
        /// </summary>
        public const string HYPHEN_REPLACEMENT = "$1-$2";

        /// <summary>
        /// Compile the regex patterns once for better runtime performance.
        /// </summary>
        public const RegexOptions REGEX_OPTS = RegexOptions.Compiled;

        /// <summary>
        /// The file name for the application's primary configuration file.
        /// Used when building the configuration for the application to load settings from a JSON file.
        /// </summary>
        public const string APPSETTING_JSON = "appsettings.json";

        /// <summary>
        /// Segment name used when composing secret version resource names.
        /// </summary>
        public const string VERSIONS_SEGMENT = "versions";

        /// <summary>
        /// Default page size hint when listing secret versions (server may override).
        /// </summary>
        public const int DEFAULT_PAGE_SIZE = 100;

        /// <summary>
        /// The configuration section name for Google Cloud Platform (GCP) settings.
        /// </summary>
        public const string GCP_SETTINGS = "GcpSettings";

        /// <summary>
        /// The configuration section name for Cloudflare R2 storage settings.
        /// </summary>
        public const string R2_STORAGE_SETTINGS = "R2StorageSettings";

        /// <summary>
        /// Configuration section name for NA settings (bound to <c>SsoInfoOptions</c>).
        /// </summary>
        public const string NA_INFO_SETTINGS = "NaInfoSettings";

        /// <summary>
        /// Configuration section name for NA settings (bound to <c>SsoInfoOptions</c>).
        /// </summary>
        public const string QUEUE_INFO_SETTINGS = "QueueInfoSettings";

        /// <summary>
        /// The default logging category for Microsoft-related logs.
        /// </summary>
        public const string LOGGER_MICROSOFT_CATEGORY = "Microsoft";

        /// <summary>
        /// Constant representing the logging category specific to Microsoft ASP.NET components.
        /// </summary>
        public const string LOGGER_MICROSOFT_ASPNETCORE = "Microsoft.AspNetCore";

        /// <summary>
        /// Constant representing the log category for Microsoft ASP.NET Core Hosting.
        /// </summary>
        public const string LOGGER_MICROSOFT_ASPNETCORE_HOSTING = "Microsoft.AspNetCore.Hosting";

        /// <summary>
        /// Constant representing the logging category for the Kestrel web server in Microsoft.AspNetCore.Server.
        /// </summary>
        public const string LOGGER_MICROSOFT_ASPNETCORE_SERVER_KESTREL = "Microsoft.AspNetCore.Server.Kestrel";

        /// <summary>
        /// Constant representing the logging category for Entity Framework Core database commands.
        /// </summary>
        public const string LOGGER_MICROSOFT_ENTITY_FRAMEWORK_DATABASE_COMMAND = "Microsoft.EntityFrameworkCore.Database.Command";

        /// <summary>
        /// Constant representing the logging category for Microsoft Entity Framework Core.
        /// </summary>
        public const string LOGGER_MICROSOFT_ENTITY_FRAMEWORK = "Microsoft.EntityFrameworkCore";

        /// <summary>
        /// The property name used to specify the service name in log entries.
        /// </summary>
        public const string LOGGER_SERVICE_PROPERTY = "service";

        /// <summary>
        /// The property name used to specify the environment in log entries.
        /// </summary>
        public const string LOGGER_ENV_PROPERTY = "env";

        /// <summary>
        /// Represents the string value for the latest version or state.
        /// </summary>
        public const string LATEST = "latest";

        /// <summary>
        /// The property name used to specify the correlation ID in log entries for distributed tracing.
        /// </summary>
        public const string CORRELATION_ID_PROPERTY = "CorrelationId";

        /// <summary>
        /// Constant representing the property name for storing or accessing the trace identifier, which is typically used for tracking and correlating log or request data throughout a system.
        /// </summary>
        public const string TRACE_ID_PROPERTY = "TraceId";
        
        /// <summary>
        /// OpenTelemetry tag key for the correlation ID.
        /// </summary>
        public const string OTEL_CORRELATION_ID_TAG = "correlation_id";

        /// <summary>
        /// OpenTelemetry tag key for the request timestamp.
        /// </summary>
        public const string OTEL_REQUEST_TIMESTAMP_TAG = "request.timestamp";

        /// <summary>
        /// Represents the header key used for tracking correlation ID in API requests.
        /// </summary>
        public const string X_CORRELATION_ID = "X-Correlation-ID";

        /// <summary>
        /// Constant representing the W3C Trace Context header name used for distributed tracing.
        /// </summary>
        public const string TRACEPARENT = "traceparent";

        /// <summary>
        /// Identifier for the "tracestate" system variable, used in distributed tracing contexts
        /// to pass vendor-specific trace information.
        /// </summary>
        public const string TRACE_ID = "trace_id";
        
        /// <summary>
        /// The UTC timestamp captured at the beginning of a request,
        /// typically stored in <c>HttpContext.Items</c> for later logging/metrics.
        /// </summary>
        public const string REQUEST_TIMESTAMP = "RequestTimestamp";

        /// <summary>
        /// OpenTelemetry resource attribute key indicating the logical environment
        /// where the service is running (e.g., "Development", "UAT", "Production").
        /// </summary>
        public const string OTL_SERVICE_ENVIRONMENT = "service.environment";

        /// <summary>
        /// Kept for compatibility with dashboards/queries that expect "deployment.environment".
        /// </summary>
        public const string OTL_DEPLOYMENT_ENVIRONMENT = "deployment.environment";
        
        public const string OTL_INSTANCE_ENVIRONMENT = "service.instance.id";

        /// <summary>
        /// Represents the name of the property used to track the request ID in GCP logging or tracing systems.
        /// </summary>
        public const string GCP_REQUEST_ID_PROPERTY = "RequestId";

        /// <summary>
        /// The default endpoint path for the application's unified health check.
        /// </summary>
        public const string HEALTH = "/health";

        /// <summary>
        /// The name identifier for the built-in self health check 
        /// that verifies the application process is alive.
        /// </summary>
        public const string SELF = "self";

        /// <summary>
        /// The default timeout duration, in seconds, for SQL commands.
        /// </summary>
        public const int DB_CONNECTION_INIT_TIMEOUT_SECONDS = 15;

        /// <summary>
        /// Specifies the default timeout duration, in seconds, for executing SQL commands.
        /// </summary>
        public const int SQL_COMMAND_TIMEOUT_SECONDS = 60;

        /// <summary>
        /// The maximum number of retry attempts for SQL operations in case of failure.
        /// </summary>
        public const int SQL_RETRY_COUNT = 5;

        /// <summary>
        /// Specifies the time-to-live (TTL) duration, in hours, for cached database connection information.
        /// This ensures that database connection details are refreshed periodically to maintain validity.
        /// </summary>
        public const int DB_CONNECTION_CACHE_TTL_HOURS = 4;
        
        /// <summary>
        /// Specifies the delay, in seconds, between retry attempts for SQL operations.
        /// </summary>
        public const int SQL_RETRY_DELAY_SECONDS = 10;

        /// <summary>
        /// Development environment-specific configuration file.
        /// Overrides settings from appsettings.json when running in Development.
        /// </summary>
        public const string APPSETTING_DEVELOPMENT_JSON = "appsettings.{0}.json";

        /// <summary>
        /// Format string for constructing the endpoint URL to access a Secret Manager in a specific region.
        /// </summary>
        public const string SECRET_MANAGER_ENDPOINT_FORMAT = "secretmanager.{0}.rep.googleapis.com";

        /// <summary>
        /// Format string for constructing the resource path of a Google Cloud secret, which includes the project ID and location.
        /// </summary>
        public const string SECRET_RESOURCE_PREFIX_FORMAT = "projects/{0}/locations/{1}/secrets";

        /// <summary>
        /// Environment variable name used to configure the retry mechanism for legacy system operations in Redis.
        /// </summary>
        public const string REDIS_RETRY_LEGACY_SYSTEM = "REDIS_RETRY_LEGACY_SYSTEM";

        /// <summary>
        /// Default retry value for the Redis legacy system.
        /// Used when the system requires a predefined number of retry attempts.
        /// </summary>
        public const int REDIS_RETRY_LEGACY_SYSTEM_DEFAULT_VALUE = 3;

        /// <summary>
        /// Redis key for managing delays in the legacy system.
        /// </summary>
        public const string REDIS_DELAY_LEGACY_SYSTEM = "REDIS_DELAY_LEGACY_SYSTEM";

        /// <summary>
        /// Default value for the delay duration in the legacy system when using Redis.
        /// </summary>
        public const int REDIS_DELAY_LEGACY_SYSTEM_DEFAULT_VALUE = 1000;

        /// <summary>
        /// Email SMTP user environment variable.
        /// </summary>
        public const string EMAIL_SMTP_USER = "EMAIL_SMTP_USER";

        /// <summary>
        /// Email SMTP password environment variable.
        /// </summary>
        public const string EMAIL_SMTP_PASSWORD = "EMAIL_SMTP_PASSWORD";

        /// <summary>
        /// Email from address environment variable.
        /// </summary>
        public const string EMAIL_FROM = "EMAIL_FROM";

        /// <summary>
        /// Email SMTP host environment variable.
        /// </summary>
        public const string EMAIL_SMTP_HOST = "EMAIL_SMTP_HOST";

        /// <summary>
        /// Email SMTP port environment variable.
        /// </summary>
        public const string EMAIL_SMTP_PORT = "EMAIL_SMTP_PORT";

        /// <summary>
        /// Email display name environment variable.
        /// </summary>
        public const string EMAIL_DISPLAY_NAME = "EMAIL_DISPLAY_NAME";

        /// <summary>
        /// SendGrid Key environment variable.
        /// </summary>
        public const string SENDGRID_API_KEY = "SENDGRID_API_KEY";

        /// <summary>
        /// Application/json environment variable.
        /// </summary>
        public const string TEXT_JSON = "application/json";

        /// <summary>
        /// The content type used when sending multipart form data,
        /// typically required for uploading files.
        /// </summary>
        public const string MULTIPART_FORM_DATA = "multipart/form-data";

        /// <summary>
        /// The form field name used when attaching a file in a multipart request.
        /// </summary>
        public const string FILE_NAME = "file";

        /// <summary>
        /// Represents the content type for application/x-www-form-urlencoded data.
        /// </summary>
        public const string APPLICATION_FORM_URLENCODED = "application/x-www-form-urlencoded";

        /// <summary>
        /// Key used to identify the filter data object in a system where filtering configurations or entities are required.
        /// </summary>
        public const string FILTER_OBJ_KEY = "filter_data_object";

        /// <summary>
        /// Represents the key used to identify the request body in a structured log or context.
        /// </summary>
        public const string REQUEST_BODY = "RequestBody";

        /// <summary>
        /// Constant representing the system variable key used to indicate whether the current process is an HTTP request.
        /// </summary>
        public const string IS_HTTP_REQUEST = "IsHttpRequest";

        /// <summary>
        /// Represents the key name used to store or retrieve the response body data in a structured format.
        /// </summary>
        public const string RESPONSE_BODY = "ResponseBody";

        /// <summary>
        /// Represents the HTTP request body content.
        /// </summary>
        public const string HTTP_BODY = "http_body";

        /// <summary>
        /// Application/json environment variable.
        /// </summary>
        public const string APPLICATION_XML = "application/xml";

        /// <summary>
        /// Text/Xml environment variable.
        /// </summary>
        public const string TEXT_XML = "text/xml";

        /// <summary>
        /// Represents the MIME type for plain text content.
        /// </summary>
        public const string TEXT_PLAIN = "text/plain";

        /// <summary>
        /// Entity Odata environment variable.
        /// </summary>
        public const string ENTITY_ODATA = "entity";

        /// <summary>
        /// Bearer environment variable.
        /// </summary>
        public const string BEARER = "Bearer";

        /// <summary>
        /// JWT environment variable.
        /// </summary>
        public const string JWT = "JWT";

        /// <summary>
        /// The name of the HTTP header used to pass authorization credentials.
        /// Typically utilized to include an access token or API key for authenticating requests.
        /// </summary>
        public const string AUTHORIZATION = "Authorization";

        /// <summary>
        /// Password environment variable.
        /// </summary>
        public const string PASSWORD_KEYVAULT = "password";

        /// <summary>
        /// Number cut url environment variable.
        /// </summary>
        public const int NUMBER_SEGMENTS = 2;
        
        /// <summary>
        /// Content-Type environment variable.
        /// </summary>
        public const string CONTENT_TYPE = "Content-Type";

        /// <summary>
        /// Represents the Basic Authentication format string.
        /// </summary>
        public const string BASIC_AUTHENTICATION = "Basic {0}";

        /// <summary>
        /// Represents the constant key for the Authorization header used in HTTP requests.
        /// </summary>
        public const string AUTHORIZATION_HEADER = "Authorization";

        /// <summary>
        /// API Key header
        /// </summary>
        public const string API_KEY_HEADER = "apiKey";

        /// <summary>
        /// HTTP Accept header constant used to specify media types that are acceptable for the response.
        /// </summary>
        public const string ACCEPT_HEADER = "Accept";

        /// <summary>
        /// Represents the HTTP header used to indicate the preferred language(s) for the response.
        /// </summary>
        public const string ACCEPT_LANGUAGE_HEADER = "Accept-Language";

        /// <summary>
        /// Represents the HTTP header key used to specify the version of the API accepted by the client.
        /// </summary>
        public const string ACCEPT_VERSION_HEADER = "Accept-Version";

        /// <summary>
        /// Azure SQL Server connection string environment variable.
        /// </summary>
        public const string SQL_CONNECTION_STRING_PASSWORD = "SQL_CONNECTION_STRING_PASSWORD";

        /// <summary>
        /// Azure SQL Server connection string environment variable.
        /// </summary>
        public const string SQL_CONNECTION_STRING_THIRD_PARTY_PASSWORD = "SQL_CONNECTION_STRING_THIRD_PARTY_PASSWORD";

        /// <summary>
        /// System environment variable.
        /// </summary>
        public const string SYSTEM_TEXT = "System";

        /// <summary>
        /// Padding seconds for a token
        /// </summary>
        public const int PADDING_SECONDS = 60;

        /// <summary>
        /// Default value environment
        /// </summary>
        public const int DEFAULT_VALUE = 60;

        /// <summary>
        /// Default value TOP filter ODATA environment
        /// </summary>
        public const int DEFAULT_VALUE_FILTER_TOP = 20;

        /// <summary>
        /// Default value environment
        /// </summary>
        public const int DEFAULT_RETRY_NUMBER = 5;

        /// <summary>
        /// Specifies whether the application should use local resources.
        /// </summary>
        public const string IS_USE_LOCAL = "IS_USE_LOCAL";

        /// <summary>
        /// Time for request run
        /// </summary>
        public const int TIME_RUN_REQUEST = 60;

        /// <summary>
        /// Represents the default name or identifier for the HTTP client used across the application.
        /// Typically utilized for making HTTP calls where a consistent client setup is required.
        /// </summary>
        public const string DEFAULT_CLIENT = "default-client";

        /// <summary>
        /// Represents the key used to specify the type of OAuth2.0 grant in client authentication requests.
        /// </summary>
        public const string GRANT_TYPE = "grant_type";

        /// <summary>
        /// Represents the system variable key used to specify the client identifier (CLIENT_ID)
        /// in authentication processes when making requests to third-party APIs.
        /// </summary>
        public const string CLIENT_ID = "client_id";

        /// <summary>
        /// Represents the identifier for the client secret used in authentication processes.
        /// This key is commonly required when making authorized API calls or obtaining access tokens.
        /// </summary>
        public const string CLIENT_SECRET = "client_secret";

        /// <summary>
        /// Represents the string constant "resource" used in system variables.
        /// </summary>
        public const string RESOURCE = "resource";

        /// <summary>
        /// Condition use token or not
        /// </summary>
        public const string IS_USE_TOKEN = "IS_USE_TOKEN";

        /// <summary>
        /// Time for refresh app configuration
        /// </summary>
        public const int TIME_REFRESH = 30;

        /// <summary>
        /// Retry circuit number
        /// </summary>
        public const int DURATION_OF_BREAK = 1;

        /// <summary>
        /// Default value environment
        /// </summary>
        public const int RETRY_NUMBER_CIRCUIT = 10;

        /// <summary>
        /// Second retry number enviroment
        /// </summary>
        public const int SECOND_RETRY_NUMBER = 2;

        /// <summary>
        /// Specifies the queryable method name used to apply sorting to a collection based on a specified key.
        /// Commonly used in dynamic LINQ processing or query extension methods.
        /// </summary>
        public const string QUERYABLE_ORDER_BY = "OrderBy";

        /// <summary>
        /// Represents the value used to apply an "OrderByDescending" sorting operation on a queryable collection.
        /// </summary>
        public const string QUERYABLE_ORDER_BY_DESCENDING = "OrderByDescending";

        /// <summary>
        /// Represents the method name used to perform a secondary ascending sort operation in LINQ queries.
        /// </summary>
        public const string QUERYABLE_THEN_BY = "ThenBy";

        /// <summary>
        /// Constant representing the method name 'ThenByDescending', typically used to apply descending order sorting in queryable extensions.
        /// </summary>
        public const string QUERYABLE_THEN_BY_DESCENDING = "ThenByDescending";

        /// <summary>
        /// Environment variable name used to specify the environment in which the ASP.NET Core application is running,
        /// such as Development, Staging, or Production.
        /// </summary>
        public const string ASPNETCORE_ENVIRONMENT = "ASPNETCORE_ENVIRONMENT";

        /// <summary>
        /// Represents the environment variable used to denote the development environment configuration.
        /// </summary>
        public const string ENVIRONMENT_DEVELOPMENT = "Development";

        /// <summary>
        /// Configuration key used to define the default database connection string in the application.
        /// </summary>
        public const string DEFAULT_CONNECTION = "DefaultConnection";

        /// <summary>
        /// Attribute key used in Pub/Sub messages for routing to the appropriate consumer.
        /// </summary>
        public const string ROUTING_KEY = "RoutingKey";

        /// <summary>
        /// Environment variable name that holds the Google Cloud Project ID.
        /// </summary>
        public const string GCP_PROJECT_ID = "GCP_PROJECT_ID";

        /// <summary>
        /// Represents the configuration key for login attempt settings.
        /// </summary>
        public const string LOGIN_AT_TEMPT_SETTINGS = "LoginAttemptSettings";

        /// <summary>
        /// Represents the named HTTP client configuration key used for the external email service.
        /// </summary>
        public const string EMAIL_SERVICE = "EmailService";

        /// <summary>
        /// Represents the configuration section name used to retrieve 
        /// email-related settings from the application configuration (e.g., appsettings.json).
        /// </summary>
        public const string EMAIL_SETTINGS = "EmailSettings";

        /// <summary>
        /// The configuration section name that contains settings for GCP upload operations.
        /// </summary>
        public const string UPLOAD_GCP_SETTINGS = "UploadGcpSettings";

        /// <summary>
        /// Constant representing the configuration section name for Quartz.NET scheduler settings.
        /// </summary>
        public const string QUARTZ_SETTINGS = "QuartzSettings";

        /// <summary>
        /// The name used to identify the GCP upload HttpClient or service instance
        /// within the dependency injection container.
        /// </summary>
        public const string UPLOAD_GCP_SERVICE = "UploadGcpService";

        /// <summary>
        /// The name used to identify the Cloudflare R2 object upload HTTP client.
        /// </summary>
        public const string UPLOAD_R2_OBJECT = "upload_r2_object";

        /// <summary>
        /// The name of the HTTP client configuration used for retrieving an image
        /// from GCP by its file identifier.
        /// </summary>
        public const string UPLOAD_GCP_GET_IMAGE_BY_FILE_ID = "GetImageByFileId";

        /// <summary>
        /// The key name used to retrieve the API key value for authenticating requests
        /// to the GCP upload service.
        /// </summary>
        public const string UPLOAD_GCP_API_KEY = "x-apikey";

        /// <summary>
        /// The form field name used to pass a textual description of the uploaded file.
        /// </summary>
        public const string UPLOAD_GCP_DESCRIPTION = "description";

        /// <summary>
        /// The form field name that specifies who performed the upload operation.
        /// Typically used for tracking or auditing purposes.
        /// </summary>
        public const string UPLOAD_GCP_UPLOAD_BY = "uploadedBy";

        /// <summary>
        /// The form field name that indicates whether a thumbnail should be generated
        /// for the uploaded file.
        /// Expected values are typically "true" or "false".
        /// </summary>
        public const string UPLOAD_GCP_THUMBNAIL = "generateThumbnail";

        /// <summary>
        /// The form field name that identifies the target resource ID associated
        /// with the uploaded file.
        /// This is usually required by GCP to bind the file to a specific entity.
        /// </summary>
        public const string UPLOAD_GCP_RESOURCE_ID = "resourceId";

        /// <summary>
        /// Environment variable name for Google Storage prefix.
        /// </summary>
        public const string GOOGLE_STORAGE_PREFIX = "https://storage.googleapis.com/";

        /// <summary>
        /// Default fallback text used when a product description or 
        /// any optional string field is missing, empty, or not provided 
        /// by the upstream CRM / external API.
        /// </summary>
        public const string DEFAULT_TEXT = "N/A";

        /// <summary>
        /// Default email address used within the system for configurations or testing purposes.
        /// </summary>
        public const string DEFAULT_EMAIL = "test123@gmail.com";

        /// <summary>
        /// Default Arabic placeholder text used when a product does not provide
        /// an Arabic description or when the value must be overridden for testing.
        /// This prevents empty or null Arabic fields from being sent to UPG.
        /// </summary>
        public const string DEFAULT_AR = "اختبار المنتج";
    }

    /// <summary>
    /// Contains constant strings representing standard OData keywords, attributes,
    /// and query parameters used for constructing and handling OData requests.
    /// </summary>
    public static class ODataConstant
    {
        /// <summary>
        /// The reserved path segment used to retrieve the OData metadata document.
        /// </summary>
        public const string METADATA_ATTRIBUTE = "$metadata";

        /// <summary>
        /// Custom query parameter used to specify the entity type in the request.
        /// </summary>
        public const string OBJECT_ATTRIBUTE = "entity";

        /// <summary>
        /// The OData $filter query option to filter results based on boolean expressions.
        /// </summary>
        public const string ODATA_FILTER = "$filter";

        /// <summary>
        /// The OData $orderby query option to sort the results by specified properties.
        /// </summary>
        public const string ODATA_ORDERBY = "$orderby";

        /// <summary>
        /// The OData $skip query option to skip a specified number of results.
        /// </summary>
        public const string ODATA_SKIP = "$skip";

        /// <summary>
        /// The OData $select query option to return only selected properties in the response.
        /// </summary>
        public const string ODATA_SELECT = "$select";

        /// <summary>
        /// The OData $expand query option to include related entities inline in the response.
        /// </summary>
        public const string ODATA_EXPAND = "$expand";

        /// <summary>
        /// The OData $count query option to include the total count of matching entities.
        /// </summary>
        public const string ODATA_COUNT = "$count";

        /// <summary>
        /// The OData $top query option to limit the number of results returned.
        /// </summary>
        public const string ODATA_TOP = "$top";

        /// <summary>
        /// Represents the HTTP header name used to specify OData preferences.
        /// </summary>
        public const string ODATA_PREFER = "Prefer";

        /// <summary>
        /// Represents the OData header value that instructs the API to include all annotations in the response.
        /// </summary>
        public const string ODATA_INCLUDE = "odata.include-annotations=\"*\" ";

        /// <summary>
        /// Defines the OData <c>Prefer</c> header value used to include
        /// all available OData annotations in the API response.
        /// </summary>
        public const string ODATA_PREFER_VALUE = "odata.include-annotations=\"*\", odata.maxpagesize={0}";
    }

    /// <summary>
    /// Code for system responses.
    /// </summary>
    public static class SystemCode
    {
        /// <summary>
        /// OK response code.
        /// </summary>
        public const int OK = 200;

        /// <summary>
        /// No Content response code.
        /// </summary>
        public const int NO_CONTENT = 204;

        /// <summary>
        /// Bad Request response code.
        /// </summary>
        public const int BAD_REQUEST = 400;

        /// <summary>
        /// Unauthorized response code.
        /// </summary>
        public const int UNAUTHORIZED = 401;

        /// <summary>
        /// Forbidden response code.
        /// </summary>
        public const int FORBIDDEN = 403;

        /// <summary>
        /// Not Found response code.
        /// </summary>
        public const int NOT_FOUND = 404;

        /// <summary>
        /// Internal Server Error response code.
        /// </summary>
        public const int INTERNAL_SERVER = 500;

        /// <summary>
        /// Too Many Requests response code.
        /// </summary>
        public const int MANY_REQUESTS = 429;

        /// <summary>
        /// Conflict Error response code.
        /// </summary>
        public const int CONFLICT_ERROR = 409;

        /// <summary>
        /// Bad gateway timeout.
        /// </summary>
        public const int BAD_GATEWAY_TIMEOUT = 504;
    }

    /// <summary>
    /// Code for method types.
    /// </summary>
    public static class MethodCode
    {
        /// <summary>
        /// POST-method code.
        /// </summary>
        public const string POST = "post";

        /// <summary>
        /// Method code representing a POST operation for SOAP-based services.
        /// </summary>
        public const string POST_SOAP = "post_soap";

        /// <summary>
        /// PUT method code.
        /// </summary>
        public const string PUT = "put";

        /// <summary>
        /// GET method code.
        /// </summary>
        public const string GET = "get";

        /// <summary>
        /// DELETE method code.
        /// </summary>
        public const string DELETE = "delete";
    }

    /// <summary>
    /// Provides predefined message codes for system responses, covering both
    /// success and error scenarios. These codes standardize messaging conventions
    /// across the application, ensuring consistent communication of status states
    /// such as success, validation errors, and various HTTP error conditions.
    /// </summary>
    public static class SystemMessageCode
    {
        /// <summary>
        /// Success OK message code.
        /// </summary>
        public const string OK = "success_ok";

        /// <summary>
        /// Success No Content message code.
        /// </summary>
        public const string NO_CONTENT = "success_no_content";

        /// <summary>
        /// Error Bad Request message code.
        /// </summary>
        public const string BAD_REQUEST = "error_bad_request";

        /// <summary>
        /// Error Unauthorized message code.
        /// </summary>
        public const string UNAUTHORIZED = "error_unauthorized";

        /// <summary>
        /// Error Forbidden message code.
        /// </summary>
        public const string FORBIDDEN = "error_forbidden";

        /// <summary>
        /// Error Not Found message code.
        /// </summary>
        public const string NOT_FOUND = "error_not_found";

        /// <summary>
        /// Error Internal Server message code.
        /// </summary>
        public const string INTERNAL_SERVER = "error_internal_server";

        /// <summary>
        /// Error Too Many Requests message code.
        /// </summary>
        public const string MANY_REQUESTS = "error_too_many_requests";

        /// <summary>
        /// Error Conflict message code.
        /// </summary>
        public const string CONFLICT_ERROR = "error_conflict";

        /// <summary>
        /// Error validation message code.
        /// </summary>
        public const string VALIDATION_ERROR = "Validation_failed";
        
        /// <summary>
        /// Error API version not supported message code.
        /// </summary>
        public const string API_VERSION_NOT_SUPPORTED = "error_api_version_not_supported";

        /// <summary>
        /// Error Method Not Allowed message code.
        /// </summary>
        public const string METHOD_NOT_ALLOWED = "error_method_not_allowed";

        /// <summary>
        /// Error Not Acceptable message code.
        /// </summary>
        public const string NOT_ACCEPTABLE = "error_not_acceptable";

        /// <summary>
        /// Error Unsupported Media Type message code.
        /// </summary>
        public const string UNSUPPORTED_MEDIA_TYPE = "error_unsupported_media_type";

        /// <summary>
        /// Error Service Unavailable message code.
        /// Used when a dependent service is temporarily unavailable (e.g. circuit breaker open).
        /// </summary>
        public const string SERVICE_UNAVAILABLE = "error_service_unavailable";
    }

    /// <summary>
    /// Contains constant strings for system messages utilized to describe error scenarios
    /// and provide detailed information about system operations and validations.
    /// </summary>
    public static class SystemMessage
    {
        /// <summary>
        /// Token used in the Serilog output template to render exception details.
        /// Removed when generating the base log line (prefix + message only).
        /// </summary>
        public const string LOG_TOKEN_EXCEPTION = " {NewLine}{Exception}";

        /// <summary>
        /// Token used in the Serilog output template to render the log message.
        /// Removed when generating stack trace lines so the message is not repeated for each line.
        /// </summary>
        public const string LOG_TOKEN_MESSAGE = " {Message:lj}";
        
        /// <summary>
        /// Log message indicating that an HTTP request failed due to a specific HTTP request exception.
        /// </summary>
        public const string LOG_HTTP_REQUEST_FAILED = "HTTP request failed for client. ClientName={ClientName}, HttpMethod={HttpMethod}, Uri={RequestUri}.";

        /// <summary>
        /// Represents the log message used to indicate that an HTTP request was blocked due to the circuit breaker being open.
        /// </summary>
        public const string LOG_HTTP_CIRCUIT_OPEN = "HTTP request blocked because circuit breaker is OPEN. Client={ClientName}, Method={Method}, Uri={Uri}";
        
        /// <summary>
        /// Log message indicating that an unexpected error occurred during an HTTP request.
        /// </summary>
        public const string LOG_HTTP_UNEXPECTED_ERROR = "Unexpected error while sending HTTP POST for client. ClientName={ClientName}, HttpMethod={HttpMethod}, Uri={RequestUri}.";

        /// <summary>
        /// Represents the log message template for HTTP request timeout events,
        /// providing information about the client name, HTTP method, URI,
        /// and the timeout duration in seconds.
        /// </summary>
        public const string LOG_HTTP_REQUEST_TIMEOUT = "HTTP request timeout for client. ClientName={ClientName}, HttpMethod={HttpMethod}, Uri={Uri}, TimeoutSeconds={TimeoutSeconds}, TimeoutCtsCancelled={TimeoutCtsCancelled}";
        
        /// <summary>
        /// Log message indicating that the database connection string is being resolved using GCP Secret Manager.
        /// </summary>
        public const string LOG_USING_GCP_SECRET = "Using GCP Secret Manager to resolve database connection string. SecretId={SecretId}, Version={SecretVersion}.";

        /// <summary>
        /// Log message indicating a failure to retrieve the database connection string from GCP Secret Manager.
        /// </summary>
        public const string LOG_GCP_SECRET_ERROR = "Failed to retrieve database connection string from GCP Secret Manager. SecretId={SecretId}, Version={SecretVersion}.";

        /// <summary>
        /// Log message indicating that the database connection has been successfully opened.
        /// </summary>
        public const string LOG_CONNECTION_OPENED = "Database connection opened successfully. Provider={Provider}.";

        /// <summary>
        /// Log message indicating that the connection string is missing or empty for the specified provider.
        /// </summary>
        public const string LOG_CONNECTION_STRING_MISSING = "Connection string is not configured or is empty for Provider={Provider}.";
        
        /// <summary>
        /// Log message template when changes have been successfully saved to the database context.
        /// </summary>
        public const string LOG_SAVED_CHANGES = "Saved {ChangesCount} change(s) for context {ContextName}.";

        /// <summary>
        /// Log message template when a transactional block completes successfully.
        /// </summary>
        public const string LOG_TRANSACTIONAL_BLOCK_COMPLETED = "Transactional block completed successfully for context {ContextName}.";

        /// <summary>
        /// Log message template when an error occurs during a transactional block and a rollback is triggered.
        /// </summary>
        public const string LOG_TRANSACTIONAL_BLOCK_ERROR = "Error while executing transactional block for context {ContextName}. Rolling back...";

        /// <summary>
        /// Exception message template used when a transactional block fails unexpectedly.
        /// </summary>
        public const string EX_TRANSACTIONAL_BLOCK_FAILED = "An error occurred while executing transactional block for context {0}.";
        
        /// <summary>
        /// Log message template indicating the start of a call to a third-party service,
        /// including the request type and serialized request data.
        /// </summary>
        public const string LOG_START_THIRD_PARTY_CALL = "Start calling third-party service with {RequestType} | Request={RequestJson}";

        /// <summary>
        /// Log message template indicating the conclusion of a third-party service call,
        /// including details about the request type, status code, and response content.
        /// </summary>
        public const string LOG_END_THIRD_PARTY_CALL = "End calling third-party service with {RequestType} | StatusCode={StatusCode} | Response={ResponseJson}";
        
        /// <summary>
        /// Log message template indicating the start of an HTTP request,
        /// including details such as the HTTP method, request path, action name,
        /// and request body arguments.
        /// </summary>
        public const string LOG_MSG_START = "HTTP {RequestMethod} {RequestPath} starting action {ActionName} with arguments {RequestBody}";

        /// <summary>
        /// Log message template indicating the completion of an HTTP request.
        /// Includes details such as the request method, request path,
        /// status code, request body, and response body.
        /// </summary>
        public const string LOG_MSG_FINISH = "HTTP {RequestMethod} {RequestPath} finished with status {StatusCode}. RequestBody={RequestBody} ResponseBody={ResponseBody}";
        
        /// <summary>
        /// Message thrown when the secretId is null/empty.
        /// </summary>
        public const string SECRET_ID_REQUIRED = "SecretId is required.";

        /// <summary>
        /// The message indicating that a request to retrieve a new token is being sent.
        /// </summary>
        public const string RETRIEVE_NEW_TOKEN = "Sending request to retrieve new token.";

        /// <summary>
        /// Log template when the existence check for a secret fails.
        /// </summary>
        public const string LOG_EXISTS_FAILED = "ExistsAsync check failed for {Secret}";

        /// <summary>
        /// Log template when version existence check fails.
        /// </summary>
        public const string LOG_VERSION_EXISTS_FAILED = "VersionExistsAsync check failed for {VersionName}";

        /// <summary>
        /// Log message indicating a value has been successfully retrieved from the cache.
        /// Includes the cache key referencing the retrieved value.
        /// </summary>
        public const string LOG_CACHE_VALUE_RETRIEVED = "Retrieved value from cache. Key={CacheKey}.";
        
        /// <summary>
        /// Log message indicating that a value was not found in the cache for the specified key.
        /// </summary>
        public const string LOG_CACHE_VALUE_NOT_FOUND = "Value not found in cache. Key={CacheKey}.";

        /// <summary>
        /// Error message indicating a failure while processing a response from a third-party service.
        /// Provides placeholders for the HTTP status code and reason phrase associated with the error.
        /// </summary>
        public const string PROCESS_HANDLE_API_RESPONSE_ERROR = "Error processing response from third-party service. Status Code: {0}, Reason: {1}, Message: {2}.";

        /// <summary>
        /// Represents the error message logged when there is a failure in processing the response
        /// from a third-party service. The message includes placeholders for the status code,
        /// reason, and additional details of the error.
        /// </summary>
        public const string LOG_PROCESS_HANDLE_API_RESPONSE_ERROR = "Error processing response from third-party service. Status Code: {StatusCode}, Reason: {ReasonPhrase}, Message: {Content}.";

        /// <summary>
        /// Error message indicating that the specified HTTP method is not supported.
        /// </summary>
        public const string HTTP_METHOD_NOT_SUPPORTED = "HTTP method '{0}' is not supported.";

        /// <summary>
        /// Error message template indicating that the specified database provider is unsupported.
        /// Accepts a placeholder for the database provider's name or type.
        /// </summary>
        public const string ERROR_UNSUPPORTED_DATABASE_PROVIDER = "Unsupported database provider: {0}";

        /// <summary>
        /// Error message indicating that the connection string is not configured.
        /// </summary>
        public const string ERROR_CONNECTION_STRING_NOT_CONFIGURED = "Connection string is not configured.";

        /// <summary>
        /// Error message indicating an invalid "OrderBy" string format.
        /// Provides guidance on the expected format: "Property, Property2 ASC, Property2 DESC".
        /// </summary>
        public const string ERROR_INVALID_ORDER_BY_STRING = "Invalid OrderBy string '{0}'. Order By Format: Property, Property2 ASC, Property2 DESC";

        /// <summary>
        /// Error message indicating that an invalid or unrecognized property was provided.
        /// Order By Format: Property, Property2 ASC, Property2 DESC.
        /// </summary>
        public const string ERROR_INVALID_PROPERTY = "Invalid Property. Order By Format: Property, Property2 ASC, Property2 DESC";

        /// <summary>
        /// Error message indicating that the object to validate cannot be null.
        /// </summary>
        public const string VALIDATION_OBJECT_NULL_ERROR = "The object to validate cannot be null.";

        /// <summary>
        /// Error JWT validation.
        /// </summary>
        public const string JWT_VALID_ERROR = "Invalid JWT token";

        /// <summary>
        /// Error EntityType and EntityName must be provided for all requests.
        /// </summary>
        public const string ODATA_ENTITY_TYPE_ERROR = "EntityType and EntityName must be provided for all requests.";

        /// <summary>
        /// Not a valid enum.
        /// </summary>
        public const string NOT_DEFINE_IN_ENUM_ERROR = "{0} is not a valid enum.";

        /// <summary>
        /// Error message indicating an issue occurred while calling query for Db.
        /// Typically used for logging and troubleshooting.
        /// </summary>
        public const string EDM_ODATA_QUERY_DB_OCCURED = "Failed to parse or validate OData query: {0}";

        /// <summary>
        /// Circuit breaker error.
        /// </summary>
        public const string CIRCUIT_BOKEN_ERROR = "The system is currently overloaded. Please try again later: {0}";

        /// <summary>
        /// Error message indicating that an issue occurred while creating or writing the response.
        /// </summary>
        public const string RESPONSE_CREATION_ERROR = "Error occurred while creating or writing the response.";

        /// <summary>
        /// Error message indicating that HttpRequestData is null, preventing the creation of a valid HttpResponseData.
        /// </summary>
        public const string HTTP_REQUEST_DATA_NULL_ERROR = "HttpRequestData is null. Unable to create a valid HttpResponseData.";

        /// <summary>
        /// Error message indicating that the object to validate cannot be null.
        /// </summary>
        public const string VALIDATION_FAILURES_HAVE_OCCURRED = "One or more validation failures have occurred.";

        /// <summary>
        /// Message indicating invalid JSON format, including the error details and the problematic content.
        /// </summary>
        public const string INVALID_JSON_FORMAT_MESSAGE = "Invalid JSON format. Error: {0}. Content: {1}";

        /// <summary>
        /// Error message indicating that the response has already started,
        /// and the error handler will not modify the response.
        /// </summary>
        public const string RESPONSE_ALREADY_STARTED = "The response has already started; the error handler will not modify the response.";

        /// <summary>
        /// Log message when starting the PublishAsync method in GpsPublisherService.
        /// The {Name} placeholder will be replaced with the type name (typeof(T).Name).
        /// </summary>
        public const string GPSPUBLISHER_SERVICE_PUBLISH_ASYNC_START = "[GpsPublisherService] PublishAsync - Start: {Name}";

        /// <summary>
        /// Log message when a message has been successfully published by the PublishAsync method in GpsPublisherService.
        /// The {MsgId} placeholder will be replaced with the message ID.
        /// </summary>
        public const string GPSPUBLISHER_PUBLISHASYNC_MESSAGE_PUBLISHED = "[GpsPublisherService] PublishAsync - Message is published, GCP-MessageId: {MsgId}";

        /// <summary>
        /// Log message used when an error occurs while sending a message in the PublishAsync method of GpsPublisherService.
        /// This constant is intended for use with LogError().
        /// </summary>
        public const string GPSPUBLISHER_SERVICEPUBLISHASYNC_SEND_MESSAGEERRORLOG = "[GpsPublisherService] PublishAsync - Send message Error";

        /// <summary>
        /// Exception message used when an error occurs while sending a message in the PublishAsync method of GpsPublisherService.
        /// This constant is intended for use when throwing exceptions.
        /// </summary>
        public const string GPSPUBLISHER_SERVICEPUBLISHASYNC_SENDMESSAGE_ERROREXCEPTION = "[GpsPublisherService] - PublishAsync - Send message Error";

        /// <summary>
        /// Error message when the GcpAttributeSetting or TopicId is missing for a message type.
        /// </summary>
        public const string MISSING_GCP_ATTRIBUTE_SETTING_TOPICID = "Missing GcpAttributeSetting/TopicId for message type '{0}'.";

        /// <summary>
        /// Message logged when the SubscriptionId for an event type is empty.
        /// </summary>
        public const string SKIP_EVENT_SUBSCRIPTIONID_EMPTY = "[GpsSubscriberClient] Skip event {EventType} because SubscriptionId is empty.";

        /// <summary>
        /// Message logged when a subscriber has successfully started.
        /// </summary>
        public const string SUBSCRIBER_STARTED = "[GpsSubscriberClient] Subscriber started for {Subscription}.";

        /// <summary>
        /// Message logged when the HandleEvent method retries after a transient error.
        /// </summary>
        public const string HANDLE_EVENT_RETRY = "[GpsSubscriberService] HandleEvent - Retry {RetryCount} after {TimeSpan}";

        /// <summary>
        /// Message logged when starting to process an incoming message.
        /// </summary>
        public const string HANDLE_EVENT_START = "[GpsSubscriberService] HandleEvent - Start processing message, GCP-MessageId: {MessageId}";

        /// <summary>
        /// Message logged after finishing the processing of an incoming message.
        /// </summary>
        public const string HANDLE_EVENT_FINISHED = "[GpsSubscriberService] HandleEvent - Finished processing message, GCP-MessageId: {MessageId}, Elapsed: {Elapsed}";

        /// <summary>
        /// Message logged when an error occurs while processing an incoming message.
        /// </summary>
        public const string HANDLE_EVENT_ERROR = "[GpsSubscriberService] HandleEvent Error";

        /// <summary>
        /// Exception message thrown when the GcpAttributeSetting attribute is missing on an event type.
        /// Use string.Format or interpolation to insert the event type name.
        /// </summary>
        public const string MISSING_GCP_ATTRIBUTE_SETTING_EXCEPTION = "Missing GcpAttributeSetting on event type '{0}'.";

        /// <summary>
        /// Message logged when ExecuteAsync is canceled (normal shutdown).
        /// </summary>
        public const string EXECUTE_ASYNC_CANCELED = "[GpsSubscriberClient] ExecuteAsync canceled.";

        /// <summary>
        /// Message logged when ExecuteAsync encounters an unhandled exception.
        /// </summary>
        public const string EXECUTE_ASYNC_UNHANDLED_EXCEPTION = "[GpsSubscriberClient] ExecuteAsync - Unhandled exception.";

        /// <summary>
        /// Simplified exception message used when the event handler cannot be resolved from the DI container.
        /// </summary>
        public const string HANDLER_NOT_RESOLVED_FROM_DI_SIMPLE = "Handler could not be resolved from DI.";

        /// <summary>
        /// Represents the cancellation message used when the execution of a background service, such as
        /// GcpSubscriberJob, is terminated. This typically occurs during application shutdown to signal that
        /// the subscriber stopped gracefully, and no further processing will continue.
        /// </summary>
        public const string EXECUTE_ASYNC_CANCELED_MESSAGE = "GpsSubscriberService execution was canceled. This usually happens during application shutdown. The subscriber stopped gracefully but no further processing can continue.";

        /// <summary>
        /// Represents the error message used when the GpsSubscriberService encounters
        /// an unhandled exception during asynchronous message processing, leading to
        /// the service shutdown.
        /// </summary>
        public const string EXECUTE_ASYNC_UNHANDLED_EXCEPTION_MESSAGE = "GpsSubscriberService encountered an unhandled exception during message processing. The service cannot continue and has been stopped.";

        /// <summary>
        /// Circuit breaker error.
        /// </summary>
        public const string CIRCUIT_BROKEN_ERROR = "The system is currently overloaded. Please try again later: {0}";

        /// <summary>
        /// Error message thrown when attempting to register a handler for an event 
        /// that already has a handler registered.
        /// Use string.Format(ERROR_MESSAGES.HANDLER_ALREADY_REGISTERED, eventName) to insert the event name.
        /// </summary>
        public const string HANDLER_ALREADY_REGISTERED = "Handler for event {0} is already registered.";

        /// <summary>
        /// Error message thrown when attempting to register an event type 
        /// that is already registered under the same key.
        /// Use string.Format(ERROR_MESSAGES.EVENT_TYPE_ALREADY_REGISTERED, eventName) to insert the event name.
        /// </summary>
        public const string EVENT_TYPE_ALREADY_REGISTERED = "Event type {0} is already registered.";

        /// <summary>
        /// Message indicating that authentication has failed during a process or operation.
        /// Typically used in logging to represent failed authentication attempts.
        /// </summary>
        public const string AUTHENTICATION_FAIL = "Authentication failed.";

        /// <summary>
        /// Login client to API exception
        /// </summary>
        public const string LOGIN_CLIENT_EXCEPTION = "Error Login client to api service exception: {0}";

        /// <summary>
        /// Message template for logging when object deletion fails.
        /// Includes contextual placeholders for object and bucket names along with the root error message.
        /// </summary>
        public const string DELETE_ERROR = "Error deleting object {ObjectName} from bucket {BucketName}: {ErrorMessage}";

        /// <summary>
        /// Message template for logging when signed URL generation fails.
        /// Includes placeholders for the target object and bucket.
        /// </summary>
        public const string SIGNED_URL_ERROR = "Error generating signed URL for {ObjectName} in {BucketName}: {ErrorMessage}";

        /// <summary>
        /// Message template for logging when file upload fails.
        /// Includes placeholders to identify the target object and bucket involved in the operation.
        /// </summary>
        public const string UPLOAD_ERROR = "Error uploading object {ObjectName} to bucket {BucketName}: {ErrorMessage}";

        /// <summary>
        /// Exception message for scenarios where a signed URL could not be generated.
        /// Should be thrown after logging the detailed failure using <see cref="SIGNED_URL_ERROR"/>.
        /// </summary>
        public const string SIGNED_URL_FAILED = "Could not generate signed URL";

        /// <summary>
        /// Exception message used when the upload to Google Cloud Storage fails.
        /// Works alongside <see cref="UPLOAD_ERROR"/> for detailed error logs.
        /// </summary>
        public const string UPLOAD_FAILED = "Could not upload file to GCP";

        /// <summary>
        /// Represents the message format indicating that the requested API version is not supported.
        /// </summary>
        public const string MSG_VERSION_NOT_SUPPORTED_FORMAT = "API version is not supported. Path: {0}";

        /// <summary>
        /// Represents a system message indicating that the media type provided in the request is unsupported.
        /// </summary>
        public const string MSG_UNSUPPORTED_MEDIA_TYPE = "Unsupported Media Type. Please set Content-Type correctly.";

        /// <summary>
        /// Represents the message indicating that the HTTP method used is not allowed for the specified endpoint.
        /// </summary>
        public const string MSG_METHOD_NOT_ALLOWED = "Method not allowed for this endpoint.";

        /// <summary>
        /// Represents a system message indicating that the request is not acceptable,
        /// </summary>
        public const string MSG_NOT_ACCEPTABLE = "Not acceptable. Please check Accept header.";

        /// <summary>
        /// Represents a message indicating the failure of a request.
        /// </summary>
        public const string MSG_REQUEST_FAILED = "Request failed.";

        /// <summary>
        /// Represents a message indicating that the requested resource was not found.
        /// </summary>
        public const string MSG_NOT_FOUND = "Resource not found.";

        /// <summary>
        /// Contains the message used to indicate that too many requests have been made to the server.
        /// </summary>
        public const string MSG_TOO_MANY_REQUESTS = "Too many requests.";

        /// <summary>
        /// Represents the system message indicating that the payload size exceeds the allowed limit.
        /// </summary>
        public const string MSG_PAYLOAD_TOO_LARGE = "Payload too large.";
        
        /// <summary>
        /// Defines the error message template for indicating a request timeout.
        /// </summary>
        public const string ERROR_REQUEST_TIMEOUT = "Request timeout after {0}s. Error Message: {1}";
        
        /// <summary>
        /// Log message for email sending failure.
        /// </summary>
        public const string ERROR_SENDING_EMAIL = "Error occurred while sending email to {Email}";

        /// <summary>
        /// Exception message for Selector must be a property expression.
        /// </summary>
        public const string ERROR_MUST_BE_EXPRESSION = "Selector must be a property expression.";

        /// <summary>
        /// Indicates that a request was canceled by the client.
        /// </summary>
        public const string REQUEST_CANCELED_BY_CLIENT = "Request was canceled by client. Path={Path}, Method={Method}";

        /// <summary>
        /// Indicates that an error response was skipped because the corresponding request was aborted.
        /// </summary>
        public const string SKIPPED_WRITING_ERROR_RESPONSE_BECAUSE_REQUEST_ABORTED = "Skipped writing error response because request was aborted. Path={Path}, Method={Method}";

        /// <summary>
        /// Represents an error message indicating that the request was canceled while attempting to write the error response.
        /// </summary>
        public const string REQUEST_CANCELED_WHILE_WRITING_ERROR_RESPONSE = "Request was canceled while writing error response. Path={Path}, Method={Method}";

        
        /// <summary>
        /// Exception message templates for ResourceFileProvider.
        /// </summary>
        public static class ResourceFileProviderErrors
        {
            /// <summary>
            /// Thrown when the provided key does not exist in the configured map.
            /// </summary>
            public const string ERR_EXCEL_KEY_NOT_CONFIGURED = "Excel reportKey '{0}' not configured.";

            /// <summary>
            /// Thrown when the expected embedded resource cannot be located in the assembly.
            /// </summary>
            public const string ERR_EMBEDDED_RESOURCE_NOT_FOUND = "Embedded excel resource not found: '{0}'. Available: {1}";
        }
    }
}
