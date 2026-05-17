namespace Be.Haven.Shared.Constants;

/// <summary>
/// Contains constants for Redis configuration and messages.
/// </summary>
public static class RedisConstants
{
    /// <summary>
    /// Environment variable name used to configure cache-related settings for the application.
    /// </summary>
    public const string CACHE_SETTING = "CacheSettings";

    /// <summary>
    /// Default cache-aside payload absolute expiration in seconds when CacheSettings is missing or invalid.
    /// </summary>
    public const int DEFAULT_ABSOLUTE_EXPIRATION_SECONDS = 10;

    /// <summary>
    /// Identifier for determining if the cache is stored in memory.
    /// </summary>
    public const string IS_IN_MEMORY = "IS_IN_MEMORY";

    /// <summary>
    /// Redis host environment variable.
    /// </summary>
    public const string REDIS_HOST = "REDIS_HOST";

    /// <summary>
    /// Redis port environment variable.
    /// </summary>
    public const string REDIS_PORT = "REDIS_PORT";

    /// <summary>
    /// Redis password environment variable.
    /// </summary>
    public const string REDIS_PASSWORD = "REDIS_PASSWORD";

    /// <summary>
    /// Redis SSL environment variable.
    /// </summary>
    public const string REDIS_SSL = "REDIS_SSL";

    /// <summary>
    /// Redis database number environment variable.
    /// </summary>
    public const string REDIS_DB_NUMBER = "REDIS_DB_NUMBER";

    /// <summary>
    /// Contains message information constants.
    /// </summary>
    public static class MessageInfo
    {
        /// <summary>
        /// Message indicating data was fetched from cache.
        /// </summary>
        public const string FETCHED_FROM_CACHE = "Fetched from Cache-> '{CacheKey}'.";

        /// <summary>
        /// Message indicating data was added to cache.
        /// </summary>
        public const string ADDED_TO_CACHE = "Added to Cache -> '{Key}'.";

        /// <summary>
        /// Constant message used to indicate that the Redis password secret is not provided or is empty.
        /// </summary>
        public const string REDIS_PASSWORD_SECRET_EMPTY = "Redis password secret is empty.";
    }

    /// <summary>
    /// Contains error message constants.
    /// </summary>
    public static class ErrorMessage
    {
        /// <summary>
        /// Error message for failed Redis connection.
        /// </summary>
        public const string FAILED_TO_CONNECT_TO_REDIS = "Failed to connect to Redis.";

        /// <summary>
        /// Error message for distributed cache issues.
        /// </summary>
        public const string ERROR_DISTRIBUTED_CACHE = "Error in AddDistributedCache: {ex.Message}";

        /// <summary>
        /// Error message thrown when the CacheOption section is missing or cannot be loaded from configuration.
        /// </summary>
        public const string ERROR_CACHE_OPTION_MISSING = "CacheOption is missing.";

        /// <summary>
        /// Message used when a user account is locked due to exceeding the maximum number of failed login attempts.
        /// </summary>
        public const string USER_LOCKED =
            "User {UserName} is locked due to too many failed login attempts. Remaining lock time: {LockSeconds}s";

        /// <summary>
        /// Message used when a user fails to log in (but is not yet locked).
        /// </summary>
        public const string USER_FAILED_ATTEMPT =
            "User {UserName} failed login attempt {FailedCount}/{MaxFailedAttempts}";

        /// <summary>
        /// Message used when a user is locked after reaching the maximum allowed attempts.
        /// </summary>
        public const string USER_LOCK_CREATED =
            "User {UserName} has been locked for {LockMinutes} minutes after {FailedCount} failed attempts.";

        /// <summary>
        /// Message used when a user's failed attempts and lock state have been reset (e.g., successful login).
        /// </summary>
        public const string USER_RESET =
            "Login attempts and lock state reset for user {UserName}.";

        /// <summary>
        /// Message shown when a user account is locked due to multiple failed login attempts.
        /// </summary>
        public const string ACCOUNT_LOCKED =
            "Account is locked. Please try again later.";
    }

    /// <summary>
    /// Contains environment-related constants.
    /// </summary>
    public static class EnvironmentVariables
    {
        /// <summary>
        /// Cache key environment variable.
        /// </summary>
        public const string CACHEKEY = "CacheKey";

        /// <summary>
        /// Bypass cache environment variable.
        /// </summary>
        public const string BYPASSCACHE = "BypassCache";

        /// <summary>
        /// Absolute expiration cache-control property name used by cacheable mediator requests.
        /// </summary>
        public const string ABSOLUTEEXPIRATION = "AbsoluteExpiration";

        /// <summary>
        /// Cache scope property name used by cacheable mediator requests.
        /// </summary>
        public const string CACHESCOPE = "CacheScope";

        /// <summary>
        /// The Redis key format for tracking failed login attempts.
        /// Example: <c>auth:login-attempt:fail:johndoe</c>.
        /// </summary>
        public const string FAIL_KEY_PREFIX = "auth:login-attempt:fail:{0}";

        /// <summary>
        /// The Redis key format for representing a locked account.
        /// Example: <c>auth:login-attempt:lock:johndoe</c>.
        /// </summary>
        public const string LOCK_KEY_PREFIX = "auth:login-attempt:lock:{0}";

        /// <summary>
        /// The value stored inside a lock key to indicate that the user is currently locked.
        /// </summary>
        public const string LOCKED = "locked";
    }
}
