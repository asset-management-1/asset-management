namespace Be.Haven.Shared.Constants.Cache;

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
    /// Contains Redis scripts shared by cache infrastructure services.
    /// </summary>
    public static class Scripts
    {
        /// <summary>
        /// Increments a counter and assigns its fixed expiration only when the key is first created.
        /// </summary>
        public const string INCREMENT_WITH_EXPIRATION = """
            local value = redis.call('INCR', KEYS[1])
            if value == 1 then
                redis.call('PEXPIRE', KEYS[1], ARGV[1])
            end
            return value
            """;
    }

    /// <summary>
    /// Contains login-attempt log and client-safe error messages backed by Redis or memory cache.
    /// </summary>
    public static class ErrorMessage
    {
        /// <summary>Logged when a user remains inside the login lock window.</summary>
        public const string USER_LOCKED =
            "User {UserName} is locked due to too many failed login attempts. Remaining lock time: {LockSeconds}s";

        /// <summary>Logged after a failed login attempt that has not yet reached the lock threshold.</summary>
        public const string USER_FAILED_ATTEMPT =
            "User {UserName} failed login attempt {FailedCount}/{MaxFailedAttempts}";

        /// <summary>Logged when the failed-attempt threshold creates a temporary account lock.</summary>
        public const string USER_LOCK_CREATED =
            "User {UserName} has been locked for {LockMinutes} minutes after {FailedCount} failed attempts.";

        /// <summary>Logged after a successful login clears failed attempts and lock state.</summary>
        public const string USER_RESET =
            "Login attempts and lock state reset for user {UserName}.";
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
        /// Redis key format for failed login attempts associated with one normalized username.
        /// </summary>
        public const string FAIL_KEY_PREFIX = "auth:login-attempt:fail:{0}";

        /// <summary>
        /// Redis key format for a temporary login lock associated with one normalized username.
        /// </summary>
        public const string LOCK_KEY_PREFIX = "auth:login-attempt:lock:{0}";

        /// <summary>
        /// Value stored in a login lock key while the lock window remains active.
        /// </summary>
        public const string LOCKED = "locked";
    }
}
