namespace Be.Haven.Shared.Constants.Errors;

/// <summary>
/// Contains client-safe and configuration error messages shared across infrastructure projects.
/// </summary>
public static class ErrorConstants
{
    /// <summary>
    /// Contains client-safe errors returned by shared API infrastructure.
    /// </summary>
    public static class ApiErrors
    {
        /// <summary>Message returned when an unexpected server failure occurs.</summary>
        public const string UNEXPECTED_SERVER_ERROR = "An unexpected error occurred.";

        /// <summary>Message returned when a required dependency is temporarily unavailable.</summary>
        public const string SERVICE_UNAVAILABLE_MESSAGE = "A required dependency is temporarily unavailable.";

        /// <summary>Message returned when one or more request validation rules fail.</summary>
        public const string VALIDATION_FAILURES_HAVE_OCCURRED = "One or more validation failures have occurred.";

        /// <summary>Message returned when the requested API version is unsupported.</summary>
        public const string MSG_VERSION_NOT_SUPPORTED_FORMAT = "API version is not supported. Path: {0}";

        /// <summary>Message returned when the request media type is unsupported.</summary>
        public const string MSG_UNSUPPORTED_MEDIA_TYPE = "Unsupported Media Type. Please set Content-Type correctly.";

        /// <summary>Message returned when the HTTP method is not allowed.</summary>
        public const string MSG_METHOD_NOT_ALLOWED = "Method not allowed for this endpoint.";

        /// <summary>Message returned when the requested representation is not acceptable.</summary>
        public const string MSG_NOT_ACCEPTABLE = "Not acceptable. Please check Accept header.";

        /// <summary>Message returned for an unclassified failed HTTP request.</summary>
        public const string MSG_REQUEST_FAILED = "Request failed.";

        /// <summary>Message returned when a requested resource is not found.</summary>
        public const string MSG_NOT_FOUND = "Resource not found.";

        /// <summary>Message returned when the caller exceeds the request-rate limit.</summary>
        public const string MSG_TOO_MANY_REQUESTS = "Too many requests.";

        /// <summary>Message returned when the request payload exceeds the permitted size.</summary>
        public const string MSG_PAYLOAD_TOO_LARGE = "Payload too large.";
    }

    /// <summary>
    /// Contains provider-neutral errors raised by shared Core helpers and services.
    /// </summary>
    public static class CoreErrors
    {
        /// <summary>Message used when backend-owned code generation exhausts all attempts.</summary>
        public const string UNIQUE_CODE_GENERATION_FAILED = "Could not generate a unique code after exhausting all random segment lengths.";

        /// <summary>Message used when a requested HTTP method is unsupported.</summary>
        public const string HTTP_METHOD_NOT_SUPPORTED = "HTTP method '{0}' is not supported.";

        /// <summary>Message used when an enum value is not defined.</summary>
        public const string NOT_DEFINE_IN_ENUM_ERROR = "{0} is not a valid enum.";

        /// <summary>Message used when a property selector is not a property expression.</summary>
        public const string ERROR_MUST_BE_EXPRESSION = "Selector must be a property expression.";

        /// <summary>Message used when JSON content cannot be deserialized.</summary>
        public const string INVALID_JSON_FORMAT_MESSAGE = "Invalid JSON format. Error: {0}. Content: {1}";

        /// <summary>Message used when an HTTP request object is unexpectedly null.</summary>
        public const string HTTP_REQUEST_DATA_NULL_ERROR = "HttpRequestData is null. Unable to create a valid HttpResponseData.";
    }

    /// <summary>
    /// Contains database and secret-configuration errors raised by shared infrastructure.
    /// </summary>
    public static class DatabaseErrors
    {
        /// <summary>Message used when a required secret identifier is missing.</summary>
        public const string SECRET_ID_REQUIRED = "SecretId is required.";

        /// <summary>Message used when a configured Secret Manager value cannot be loaded.</summary>
        public const string SECRET_MANAGER_VALUE_LOAD_FAILED = "Failed to load secret for config path {0} from secret {1} in {2}.";

        /// <summary>Message used when the configured database provider is unsupported.</summary>
        public const string ERROR_UNSUPPORTED_DATABASE_PROVIDER = "Unsupported database provider: {0}";

        /// <summary>Message used when a database connection string is not configured.</summary>
        public const string ERROR_CONNECTION_STRING_NOT_CONFIGURED = "Connection string is not configured.";

        /// <summary>Message used when a caller supplies a transaction without an active database connection.</summary>
        public const string DAPPER_TRANSACTION_CONNECTION_REQUIRED = "A supplied Dapper transaction must own an open database connection.";

        /// <summary>Message used when a row-lock query is invoked without an active database transaction.</summary>
        public const string ACTIVE_DATABASE_TRANSACTION_REQUIRED = "An active database transaction is required for row locking.";

        /// <summary>Message used when the database connection host cannot initialize a connection string.</summary>
        public const string ERR_DB_CONN_HOST_FAILED =
            "DbConnectionHostService failed to initialize DB connection string. ConnectionName={0}.";

        /// <summary>Message used when a named connection string is missing from configuration.</summary>
        public const string ERR_MISSING_CONNECTION_STRING = "Missing connection string '{0}' in configuration.";

        /// <summary>Message used when a connection name is not supplied.</summary>
        public const string ERR_CONNECTION_NAME_REQUIRED = "Connection name is required.";

        /// <summary>Message used when a named connection string has not been initialized.</summary>
        public const string ERR_CONNECTION_NOT_INITIALIZED =
            "Connection string '{0}' has not been set. Ensure IConnectionStringProvider.InitializeAsync() is executed during application startup.";
    }

    /// <summary>
    /// Contains query-shape and shared validation errors.
    /// </summary>
    public static class QueryErrors
    {
        /// <summary>Message used when an OrderBy expression has an invalid format.</summary>
        public const string ERROR_INVALID_ORDER_BY_STRING = "Invalid OrderBy string '{0}'. Order By Format: Property, Property2 ASC, Property2 DESC";

        /// <summary>Message used when an OrderBy property cannot be resolved.</summary>
        public const string ERROR_INVALID_PROPERTY = "Invalid Property. Order By Format: Property, Property2 ASC, Property2 DESC";

        /// <summary>Message used when an OData request omits required entity metadata.</summary>
        public const string ODATA_ENTITY_TYPE_ERROR = "EntityType and EntityName must be provided for all requests.";

        /// <summary>Message used when an OData query cannot be parsed or validated.</summary>
        public const string EDM_ODATA_QUERY_DB_OCCURED = "Failed to parse or validate OData query: {0}";

    }

    /// <summary>
    /// Contains client-safe and technical errors for third-party HTTP boundaries.
    /// </summary>
    public static class ThirdPartyErrors
    {
        /// <summary>Client-safe message returned when an upstream service rejects a request.</summary>
        public const string THIRD_PARTY_SERVICE_ERROR = "The external service could not process the request.";

        /// <summary>Message used when an outbound request exceeds its timeout.</summary>
        public const string ERROR_REQUEST_TIMEOUT = "Request timeout after {0}s. Error Message: {1}";

        /// <summary>Message used when a circuit breaker rejects an outbound request.</summary>
        public const string CIRCUIT_BROKEN_ERROR = "The system is currently overloaded. Please try again later: {0}";
    }

    /// <summary>
    /// Contains upload validation and object-storage operation errors.
    /// </summary>
    public static class ObjectStorageErrors
    {
        /// <summary>Message used when a required upload file is missing.</summary>
        public const string OBJECT_STORAGE_REQUIRED_FILE_MISSING_MESSAGE = "Required upload file is missing.";

        /// <summary>Message used when a batch upload fails after cleanup is attempted.</summary>
        public const string OBJECT_STORAGE_BATCH_UPLOAD_FAILED_MESSAGE = "Object-storage batch upload failed.";

        /// <summary>Message used when rollback cannot remove every newly uploaded object.</summary>
        public const string OBJECT_STORAGE_ROLLBACK_INCOMPLETE_MESSAGE =
            "Object-storage batch upload failed and rollback cleanup was incomplete.";
    }

    /// <summary>
    /// Contains Cloudflare R2 configuration and operation errors.
    /// </summary>
    public static class R2Errors
    {
        /// <summary>Message used when R2 storage settings are incomplete.</summary>
        public const string R2_OPTIONS_MISSING_MESSAGE = "Cloudflare R2 storage settings are not configured.";

        /// <summary>Message used when an R2 upload fails.</summary>
        public const string R2_UPLOAD_FAILED_MESSAGE = "Could not upload file to Cloudflare R2";

        /// <summary>Message used when an R2 object key is missing.</summary>
        public const string R2_OBJECT_KEY_REQUIRED_MESSAGE = "Object key is required.";
    }

    /// <summary>
    /// Contains Redis configuration and initialization errors.
    /// </summary>
    public static class RedisErrors
    {
        /// <summary>Message used when the Redis password secret is empty.</summary>
        public const string REDIS_PASSWORD_SECRET_EMPTY = "Redis password secret is empty.";

        /// <summary>Message used when a Redis connection cannot be established.</summary>
        public const string FAILED_TO_CONNECT_TO_REDIS = "Failed to connect to Redis.";

        /// <summary>Message used when distributed cache infrastructure cannot initialize.</summary>
        public const string ERROR_DISTRIBUTED_CACHE = "Distributed cache infrastructure could not be initialized.";

        /// <summary>Message used when the Redis host is missing.</summary>
        public const string ERROR_REDIS_HOST_MISSING = "Redis host is required when distributed cache is enabled.";

        /// <summary>Message used when the Redis port is invalid.</summary>
        public const string ERROR_REDIS_PORT_INVALID = "Redis port must be greater than zero.";

        /// <summary>Message used when cache options are missing.</summary>
        public const string ERROR_CACHE_OPTION_MISSING = "CacheOption is missing.";

        /// <summary>Client-safe message returned while login attempts remain locked.</summary>
        public const string ACCOUNT_LOCKED = "Account is locked. Please try again later.";
    }

    /// <summary>
    /// Contains authentication and authorization errors shared across API services.
    /// </summary>
    public static class AuthErrors
    {
        /// <summary>Message used when Haven token validation options are incomplete.</summary>
        public const string AUTH_OPTIONS_INVALID = "AuthSettings must include issuer, audience, and a valid secret for the current environment.";

        /// <summary>Message used when a Haven bearer token is expired.</summary>
        public const string TOKEN_EXPIRED = "Token expired.";

        /// <summary>Message used when a Haven bearer token is invalid.</summary>
        public const string INVALID_TOKEN = "Invalid token.";

        /// <summary>Message used when authentication reset state cannot be validated.</summary>
        public const string AUTH_STATE_UNAVAILABLE = "Authentication state is temporarily unavailable.";

        /// <summary>Message used when an NA bearer token is missing or malformed.</summary>
        public const string MISSING_NA_ACCESS_TOKEN = "Missing or invalid NA bearer token.";

        /// <summary>Message used when a caller lacks permission for a resource.</summary>
        public const string MISSING_PERMISSION = "You do not have permission to access this resource.";

        /// <summary>Message used when an NA bearer token is invalid.</summary>
        public const string INVALID_NA_ACCESS_TOKEN = "Invalid NA token. {0}";

        /// <summary>Message used when an NA bearer token is expired.</summary>
        public const string EXPIRED_NA_ACCESS_TOKEN = "NA token expired.";

        /// <summary>Message used for a general authentication failure.</summary>
        public const string AUTHENTICATION_FAIL = "Authentication failed.";
    }

    /// <summary>
    /// Contains Google Cloud Pub/Sub event processing errors.
    /// </summary>
    public static class EventBusErrors
    {
        /// <summary>Message used when a publisher cannot send a message.</summary>
        public const string GPSPUBLISHER_SERVICEPUBLISHASYNC_SENDMESSAGE_ERROREXCEPTION =
            "[GpsPublisherService] - PublishAsync - Send message Error";

        /// <summary>Message used when publisher topic metadata is missing.</summary>
        public const string MISSING_GCP_ATTRIBUTE_SETTING_TOPICID =
            "Missing GcpAttributeSetting/TopicId for message type '{0}'.";

        /// <summary>Message used when subscriber metadata is missing from an event type.</summary>
        public const string MISSING_GCP_ATTRIBUTE_SETTING_EXCEPTION =
            "Missing GcpAttributeSetting on event type '{0}'.";

        /// <summary>Message used when an event handler cannot be resolved from dependency injection.</summary>
        public const string HANDLER_NOT_RESOLVED_FROM_DI_SIMPLE = "Handler could not be resolved from DI.";

        /// <summary>Message used when subscriber execution is cancelled.</summary>
        public const string EXECUTE_ASYNC_CANCELED_MESSAGE =
            "GpsSubscriberService execution was canceled. This usually happens during application shutdown. The subscriber stopped gracefully but no further processing can continue.";

        /// <summary>Message used when subscriber execution fails unexpectedly.</summary>
        public const string EXECUTE_ASYNC_UNHANDLED_EXCEPTION_MESSAGE =
            "GpsSubscriberService encountered an unhandled exception during message processing. The service cannot continue and has been stopped.";

        /// <summary>Message used when an event handler is registered more than once.</summary>
        public const string HANDLER_ALREADY_REGISTERED = "Handler for event {0} is already registered.";

        /// <summary>Message used when an event type is registered more than once.</summary>
        public const string EVENT_TYPE_ALREADY_REGISTERED = "Event type {0} is already registered.";
    }

    /// <summary>
    /// Contains Google Cloud Storage operation errors.
    /// </summary>
    public static class GcpStorageErrors
    {
        /// <summary>Message used when a signed URL cannot be generated.</summary>
        public const string SIGNED_URL_FAILED = "Could not generate signed URL";

        /// <summary>Message used when an object cannot be uploaded.</summary>
        public const string UPLOAD_FAILED = "Could not upload file to GCP";
    }

    /// <summary>
    /// Contains embedded-resource lookup errors.
    /// </summary>
    public static class ResourceErrors
    {
        /// <summary>Message used when an Excel report key is not configured.</summary>
        public const string ERR_EXCEL_KEY_NOT_CONFIGURED = "Excel reportKey '{0}' not configured.";

        /// <summary>Message used when an embedded Excel resource cannot be found.</summary>
        public const string ERR_EMBEDDED_RESOURCE_NOT_FOUND =
            "Embedded excel resource not found: '{0}'. Available: {1}";
    }

    /// <summary>
    /// Contains errors raised at the Quartz execution boundary.
    /// </summary>
    public static class QuartzErrors
    {
        /// <summary>
        /// Context added when a concrete Quartz job fails unexpectedly.
        /// </summary>
        public const string QUARTZ_JOB_EXECUTION_FAILED = "Quartz job '{0}' failed on instance '{1}'.";
    }

    /// <summary>
    /// Contains errors raised by provider-neutral distributed lock infrastructure.
    /// </summary>
    public static class DistributedLockErrors
    {
        /// <summary>
        /// Message returned when the lock provider cannot coordinate a protected operation.
        /// </summary>
        public const string DISTRIBUTED_LOCK_UNAVAILABLE = "The distributed lock service is temporarily unavailable.";

        /// <summary>
        /// Message used when a caller supplies an invalid lock lease duration.
        /// </summary>
        public const string LOCK_LEASE_DURATION_INVALID = "The distributed lock lease duration must be greater than zero.";
    }
}
