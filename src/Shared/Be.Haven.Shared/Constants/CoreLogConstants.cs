namespace Be.Haven.Shared.Constants;

public static class CoreLogConstants
{
    /// <summary>
    /// Centralized log message templates for Quartz jobs.
    /// Keep templates stable to make log search/alerting consistent.
    /// </summary>
    public static class QuartzLogs
    {
        /// <summary>
        /// Logged when distributed locking is disabled for the job (or the lock service is not registered),
        /// meaning the job will proceed without attempting to acquire any lock.
        /// </summary>
        public const string LOG_JOB_LOCK_DISABLED = "Quartz: Distributed lock disabled. LockKey={LockKey}";

        /// <summary>
        /// Logged when the job is configured to "try once" (no waiting) and the lock cannot be acquired.
        /// The job execution will be skipped by the caller.
        /// </summary>
        public const string LOG_JOB_LOCK_TRY_ONCE_FAILED = "Quartz: Lock try-once failed. LockKey={LockKey}, TtlSeconds={TtlSeconds}";

        /// <summary>
        /// Logged when the job starts waiting for the distributed lock within a configured wait window.
        /// Includes TTL (lock expiration), total wait time, and polling interval.
        /// </summary>
        public const string LOG_JOB_LOCK_WAITING_STARTED = "Quartz: Waiting for lock. LockKey={LockKey}, TtlSeconds={TtlSeconds}, WaitSeconds={WaitSeconds}, PollSeconds={PollSeconds}";

        /// <summary>
        /// Debug-level log emitted on each polling attempt while waiting for the lock.
        /// Useful for troubleshooting lock contention without spamming production logs at Info level.
        /// </summary>
        public const string LOG_JOB_LOCK_WAITING_POLL = "Quartz: Waiting for lock (poll). LockKey={LockKey}, Attempt={Attempt}";

        /// <summary>
        /// Logged when the lock is successfully acquired after waiting (polling) for it to be released.
        /// Includes the total number of polling attempts before acquisition.
        /// </summary>
        public const string LOG_JOB_LOCK_ACQUIRED_AFTER_WAIT = "Quartz: Lock acquired after waiting. LockKey={LockKey}, Attempts={Attempts}";

        /// <summary>
        /// Logged when the configured lock wait window elapses and the lock is still held by another instance.
        /// The job execution will be skipped by the caller.
        /// </summary>
        public const string LOG_JOB_LOCK_WAITING_TIMEOUT = "Quartz: Lock wait timed out. LockKey={LockKey}, Attempts={Attempts}, WaitSeconds={WaitSeconds}";

        /// <summary>
        /// Logged when waiting for the lock is interrupted due to job cancellation (host shutdown or scheduler stop).
        /// The job execution will be skipped by the caller.
        /// </summary>
        public const string LOG_JOB_LOCK_WAITING_CANCELLED = "Quartz: Lock wait cancelled. LockKey={LockKey}, Attempts={Attempts}";
        
        /// <summary>
        /// Log template for indicating that a job execution was prevented
        /// because the job is explicitly disabled in the configuration settings.
        /// </summary>
        public const string LOG_JOB_DISABLED_BY_CONFIGURATION = "Job is disabled by configuration. JobKey={JobKey}";
        
        /// <summary>
        /// Log when a job successfully acquires the distributed lock.
        /// Placeholders: {JobKey}, {Pod}, {LockKey}.
        /// </summary>
        public const string LOG_JOB_LOCK_ACQUIRED ="Job lock acquired. Job={JobKey}, Pod={Pod}, LockKey={LockKey}.";

        /// <summary>
        /// Log when a job execution is completed successfully.
        /// Placeholders: {JobKey}, {Pod}.
        /// </summary>
        public const string LOG_JOB_EXECUTION_COMPLETED ="Job execution completed. Job={JobKey}, Pod={Pod}.";

        /// <summary>
        /// Log when a job is cancelled due to application shutdown.
        /// Placeholders: {JobKey}, {Pod}.
        /// </summary>
        public const string LOG_JOB_CANCELLED_DURING_SHUTDOWN ="Job cancelled during shutdown. Job={JobKey}, Pod={Pod}.";

        /// <summary>
        /// Log when a job execution fails with an exception.
        /// Placeholders: {JobKey}, {Pod}.
        /// </summary>
        public const string LOG_JOB_EXECUTION_FAILED ="Job execution failed. Job={JobKey}, Pod={Pod}.";
        
        /// <summary>
        /// Log when a job execution is requested (triggered by scheduler).
        /// Placeholders: {JobKey}, {Pod}, {LockKey}.
        /// </summary>
        public const string LOG_JOB_EXECUTION_REQUESTED = "Job execution requested. Job={JobKey}, Pod={Pod}, LockKey={LockKey}.";

        /// <summary>
        /// Log when a job is skipped because the distributed lock is already held by another instance.
        /// Placeholders: {JobKey}, {Pod}, {LockKey}.
        /// </summary>
        public const string LOG_JOB_SKIPPED_LOCK_HELD = "Job skipped because lock is held. Job={JobKey}, Pod={Pod}, LockKey={LockKey}.";

    }
    
    /// <summary>
    /// Centralized log message templates for cache versioning.
    /// Keep templates stable to make log search/alerting consistent.
    /// </summary>
    public static class CacheVersionLogs
    {
        /// <summary>
        /// Version key does not exist. Service will return default version.
        /// </summary>
        public const string LOG_CACHE_VERSION_KEY_NOT_FOUND = "CacheVersion: Version key not found. Epoch={Epoch}, Key={Key}. Returning default {DefaultVersion}.";

        /// <summary>
        /// Version value in Redis is not a valid long.
        /// </summary>
        public const string LOG_CACHE_VERSION_INVALID_VALUE = "CacheVersion: Invalid version value. Epoch={Epoch}, Key={Key}, RawValue={RawValue}. Returning default {DefaultVersion}.";

        /// <summary>
        /// Version value in Redis is non-positive (defensive case).
        /// </summary>
        public const string LOG_CACHE_VERSION_NON_POSITIVE_VALUE = "CacheVersion: Non-positive version value. Epoch={Epoch}, Key={Key}, Version={Version}. Returning default {DefaultVersion}.";

        /// <summary>
        /// Successfully retrieved current version.
        /// </summary>
        public const string LOG_CACHE_VERSION_RETRIEVED = "CacheVersion: Retrieved version. Epoch={Epoch}, Key={Key}, Version={Version}.";

        /// <summary>
        /// Failed to read version from Redis (fail-open).
        /// </summary>
        public const string LOG_CACHE_VERSION_READ_FAILED = "CacheVersion: Failed to read version. Epoch={Epoch}, Key={Key}. Returning default {DefaultVersion}.";

        /// <summary>
        /// Successfully bumped version (INCR).
        /// </summary>
        public const string LOG_CACHE_VERSION_BUMPED = "CacheVersion: Bumped version. Epoch={Epoch}, Key={Key}, NewVersion={NewVersion}.";

        /// <summary>
        /// INCR returned non-positive version (defensive case), service will normalize.
        /// </summary>
        public const string LOG_CACHE_VERSION_INCR_NON_POSITIVE = "CacheVersion: INCR returned non-positive version. Epoch={Epoch}, Key={Key}, NewVersion={NewVersion}. Normalizing to {DefaultVersion}.";

        /// <summary>
        /// TTL was applied to the version key.
        /// </summary>
        public const string LOG_CACHE_VERSION_TTL_APPLIED = "CacheVersion: TTL applied. Epoch={Epoch}, Key={Key}, TtlSeconds={TtlSeconds}, Applied={Applied}.";

        /// <summary>
        /// Failed to set TTL on version key (cleanup only).
        /// </summary>
        public const string LOG_CACHE_VERSION_TTL_SET_FAILED = "CacheVersion: Failed to set TTL. Epoch={Epoch}, Key={Key}, TtlSeconds={TtlSeconds}, NewVersion={NewVersion}.";
    }
    
    /// <summary>
    /// Centralized log message templates for caching pipeline behaviors.
    /// Keep templates stable to make log search/alerting consistent.
    /// </summary>
    public static class CacheLogs
    {
        /// <summary>
        /// Query caching was bypassed explicitly by request flag.
        /// </summary>
        public const string LOG_CACHE_BYPASSED = "Cache bypassed. RequestType={RequestType}";

        /// <summary>
        /// Cache entry was not found for the computed key.
        /// </summary>
        public const string LOG_CACHE_MISS = "Cache miss. RequestType={RequestType} Key={Key} Epoch={Epoch} Version={Version}";

        /// <summary>
        /// Cache invalidation failed. Command completed but cache version was not bumped.
        /// RequestType={RequestType}
        /// </summary>
        public const string CACHE_INVALIDATION_FAILED = "Cache invalidation failed. Command completed but cache version was not bumped. RequestType={RequestType}";

        /// <summary>
        /// Cache invalidation version was bumped after a successful write command.
        /// </summary>
        public const string CACHE_VERSION_BUMPED = "Cache version bumped. Epoch={Epoch} NewVersion={NewVersion}";
    }
    
    /// <summary>
    /// Centralized log message templates for operations related to
    /// retrieving and processing embedded Excel resources.
    /// Ensures consistency in logging patterns for troubleshooting
    /// and monitoring.
    /// </summary>
    public static class EmbeddedResourceExcelSourceProviderLogs
    {
        /// <summary>
        /// Logged when the process of retrieving an embedded Excel file is started.
        /// </summary>
        public const string LOG_GET_EXCEL_STARTED = "Get embedded excel started. Key={Key}";

        /// <summary>
        /// Logged when the provided Excel key is not found in the configured resource map.
        /// </summary>
        public const string LOG_EXCEL_KEY_NOT_CONFIGURED = "Excel key not configured. Key={Key}";

        /// <summary>
        /// Logged when the embedded resource stream cannot be found in the assembly.
        /// </summary>
        public const string LOG_RESOURCE_STREAM_NOT_FOUND = "Embedded excel resource stream not found. ResourceName={ResourceName}, AvailableResources={AvailableResources}";

        /// <summary>
        /// Logged after the embedded Excel file has been successfully loaded into memory.
        /// </summary>
        public const string LOG_EXCEL_STREAM_COPY_COMPLETED = "Embedded excel loaded successfully. ResourceName={ResourceName}, SizeBytes={SizeBytes}";
    }

    /// <summary>
    /// Centralized log message templates for handling in-memory cache version operations.
    /// These logs support consistent troubleshooting and monitoring of version read/write processes.
    /// </summary>
    public static class InMemoryCacheVersionLogs
    {
        /// <summary>
        /// Log when reading version from cache fails (fail-open to default version).
        /// </summary>
        public const string LOG_IN_MEMORY_CACHE_VERSION_READ_FAILED = "CacheVersion(InMemory): Read failed. Epoch={Epoch}, Key={Key}";

        /// <summary>
        /// Log when writing version to cache fails and we retry (best-effort).
        /// </summary>
        public const string LOG_IN_MEMORY_CACHE_VERSION_WRITE_FAILED = "CacheVersion(InMemory): Write failed (attempt {Attempt}/{Max}). Epoch={Epoch}, Key={Key}";
    }

    /// <summary>
    /// Centralized log and error message templates related to database connection handling.
    /// Provides standardized messages for consistent logging and error reporting.
    /// </summary>
    public static class DatabaseConnectionConstants
    {
        /// <summary>
        /// Logged when the DbConnectionHostService is starting its execution.
        /// Indicates the initialization of the database connection process for the specified connection name.
        /// </summary>
        public const string LOG_DB_CONN_HOST_STARTING = "DbConnectionHostService starting. ConnectionName={ConnectionName}";

        /// <summary>
        /// Logged when the DbConnectionHostService has successfully completed its execution.
        /// Indicates that the database connection process for the specified connection name has finished without issues.
        /// </summary>
        public const string LOG_DB_CONN_HOST_COMPLETED = "DbConnectionHostService completed. ConnectionName={ConnectionName}";

        /// <summary>
        /// Logged when the DbConnectionHostService fails during execution.
        /// Indicates that there was an issue in processing the database connection for the specified connection name.
        /// </summary>
        public const string LOG_DB_CONN_HOST_FAILED = "DbConnectionHostService failed. ConnectionName={ConnectionName}";

        /// <summary>
        /// Raised when the DbConnectionHostService fails to initialize the database connection string.
        /// Typically indicates an issue with resolving the connection string for the specified connection name.
        /// </summary>
        public const string ERR_DB_CONN_HOST_FAILED = "DbConnectionHostService failed to initialize DB connection string. ConnectionName={0}.";
        
        /// <summary>
        /// Logged when a connection string is initialized from the GCP Secret Manager.
        /// </summary>
        public const string LOG_INIT_FROM_GCP = "Connection string initialized from GCP Secret Manager. ConnectionName={ConnectionName}, SecretId={SecretId}, SecretVersion={SecretVersion}";

        /// <summary>
        /// Logged when a DB connection fails and the interceptor will refresh the connection string once.
        /// </summary>
        public const string LOG_DB_CONN_FAILED_REFRESHING_ONCE = "DB connection failed. Refreshing connection string once. ConnectionName={ConnectionName}, ConnectionId={ConnectionId}";

        /// <summary>
        /// Logged when refreshing the connection string fails after a DB connection failure.
        /// </summary>
        public const string LOG_DB_CONN_REFRESH_FAILED_AFTER_FAILURE = "Refresh connection string failed after DB connection failure. ConnectionName={ConnectionName}, ConnectionId={ConnectionId}";
        
        /// <summary>
        /// Logged when the connection string is refreshed from the source and the last-known-good value is updated.
        /// </summary>
        public const string LOG_CONNECTION_STRING_REFRESHED = "Connection string refreshed and LKG updated. ConnectionName={ConnectionName}";
        
        /// <summary>
        /// Logged when a connection string is initialized from the application configuration.
        /// </summary>
        public const string LOG_INIT_FROM_CONFIG = "Connection string initialized from configuration. ConnectionName={ConnectionName}";

        /// <summary>
        /// Raised when a required connection string '{0}' is missing from the application configuration.
        /// </summary>
        public const string ERR_MISSING_CONNECTION_STRING = "Missing connection string '{0}' in configuration.";

        /// <summary>
        /// Error message indicating that a connection name must be provided
        /// for initializing a database connection or related operations.
        /// </summary>
        public const string ERR_CONNECTION_NAME_REQUIRED = "Connection name is required.";

        /// <summary>
        /// Raised when the connection string '{0}' has not been initialized.
        /// </summary>
        public const string ERR_CONNECTION_NOT_INITIALIZED = "Connection string '{0}' has not been set. Ensure IConnectionStringProvider.InitializeAsync() is executed during application startup.";
    }
    
    /// <summary>
    /// Contains log message constants used by email services.
    /// </summary>
    public static class EmailLogs
    {
        /// <summary>
        /// Log message when required email configuration values are missing.
        /// </summary>
        public const string MISSING_CONFIGURATION = "Email configuration is missing. ApiKey and FromEmail are required.";

        /// <summary>
        /// Log message when the email request does not contain valid recipients.
        /// </summary>
        public const string INVALID_REQUEST = "Email request is invalid because it does not contain any valid recipient.";

        /// <summary>
        /// Log message when SendGrid accepts the email request successfully.
        /// </summary>
        public const string SEND_SUCCESS = "Email sent successfully via SendGrid.";

        /// <summary>
        /// Log message when SendGrid rejects or fails to process the email request.
        /// </summary>
        public const string SEND_FAILED = "SendGrid email send failed with status code {StatusCode}.";

        /// <summary>
        /// Log message when an unexpected exception occurs while sending email.
        /// </summary>
        public const string SEND_EXCEPTION = "Error occurred while sending email.";
    }
}