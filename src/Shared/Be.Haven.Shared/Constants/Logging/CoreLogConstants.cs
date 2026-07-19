namespace Be.Haven.Shared.Constants.Logging;

/// <summary>
/// Contains structured log templates used by shared Core infrastructure.
/// </summary>
public static class CoreLogConstants
{
    /// <summary>
    /// Contains structured logs emitted by distributed lock infrastructure.
    /// </summary>
    public static class DistributedLockLogs
    {
        /// <summary>
        /// Logged when Redis cannot complete lock acquisition.
        /// </summary>
        public const string DISTRIBUTED_LOCK_ACQUISITION_FAILED = "Failed to acquire distributed lock. LeaseDuration={LeaseDuration}.";

        /// <summary>
        /// Logged after a zero-wait lock attempt completes without exposing the resource key.
        /// </summary>
        public const string DISTRIBUTED_LOCK_ACQUISITION_COMPLETED = "Distributed lock acquisition completed. Acquired={Acquired}, LeaseDuration={LeaseDuration}.";

        /// <summary>
        /// Logged when the shared Redis connection was created but is not currently connected.
        /// </summary>
        public const string REDIS_CONNECTION_NOT_READY = "The shared Redis connection is not ready after initialization.";

        /// <summary>
        /// Logged when shared Redis infrastructure cannot be initialized.
        /// </summary>
        public const string REDIS_INITIALIZATION_FAILED = "Failed to initialize shared Redis cache and lock infrastructure.";
    }

    /// <summary>
    /// Contains structured logs emitted by the shared image optimization pipeline.
    /// </summary>
    public static class ImageOptimizationLogs
    {
        /// <summary>
        /// Logged after submitted content is decoded and accepted as a supported image.
        /// </summary>
        public const string IMAGE_CONTENT_VALIDATED = "Image content validation completed. InputFormat={InputFormat}, InputBytes={InputBytes}, Width={Width}, Height={Height}.";

        /// <summary>
        /// Logged after an image is decoded, normalized, resized when needed, and encoded.
        /// </summary>
        public const string IMAGE_OPTIMIZATION_COMPLETED =
            "Image optimization completed. InputFormat={InputFormat}, InputBytes={InputBytes}, OutputBytes={OutputBytes}, OriginalWidth={OriginalWidth}, OriginalHeight={OriginalHeight}, OutputWidth={OutputWidth}, OutputHeight={OutputHeight}, Quality={Quality}.";

        /// <summary>
        /// Logged when submitted content cannot be decoded as a supported image.
        /// </summary>
        public const string IMAGE_OPTIMIZATION_REJECTED =
            "Image optimization rejected invalid or unsupported content. InputBytes={InputBytes}.";
    }

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
        public const string LOG_JOB_LOCK_TRY_ONCE_FAILED = "Quartz: Lock try-once failed. LockKey={LockKey}, LeaseSeconds={LeaseSeconds}";

        /// <summary>
        /// Logged when the job starts waiting for the distributed lock within a configured wait window.
        /// Includes the lease duration, total wait time, and polling interval.
        /// </summary>
        public const string LOG_JOB_LOCK_WAITING_STARTED = "Quartz: Waiting for lock. LockKey={LockKey}, LeaseSeconds={LeaseSeconds}, WaitSeconds={WaitSeconds}, PollSeconds={PollSeconds}";

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
        public const string LOG_JOB_LOCK_ACQUIRED = "Job lock acquired. Job={JobKey}, Pod={Pod}, LockKey={LockKey}.";

        /// <summary>
        /// Log when a job execution is completed successfully.
        /// Placeholders: {JobKey}, {Pod}.
        /// </summary>
        public const string LOG_JOB_EXECUTION_COMPLETED = "Job execution completed. Job={JobKey}, Pod={Pod}.";

        /// <summary>
        /// Log when a job is cancelled due to application shutdown.
        /// Placeholders: {JobKey}, {Pod}.
        /// </summary>
        public const string LOG_JOB_CANCELLED_DURING_SHUTDOWN = "Job cancelled during shutdown. Job={JobKey}, Pod={Pod}.";

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
        public const string LOG_CACHE_VERSION_KEY_NOT_FOUND =
            "Cache version was not found. Epoch={Epoch}, DefaultVersion={DefaultVersion}.";

        /// <summary>
        /// Version value in cache is not a valid long.
        /// </summary>
        public const string LOG_CACHE_VERSION_INVALID_VALUE = "Cache version contained an invalid value. Epoch={Epoch}.";

        /// <summary>
        /// Version value in cache is non-positive (defensive case).
        /// </summary>
        public const string LOG_CACHE_VERSION_NON_POSITIVE_VALUE =
            "Cache version contained a non-positive value. Epoch={Epoch}, Version={Version}.";

        /// <summary>
        /// Successfully retrieved current version.
        /// </summary>
        public const string LOG_CACHE_VERSION_RETRIEVED =
            "Cache version retrieved. Epoch={Epoch}, Version={Version}.";

        /// <summary>
        /// Failed to read version from cache; caller should bypass cached payloads.
        /// </summary>
        public const string LOG_CACHE_VERSION_READ_FAILED =
            "Cache version read failed. Epoch={Epoch}. Cached payloads will be bypassed.";

        /// <summary>
        /// Successfully bumped version (INCR).
        /// </summary>
        public const string LOG_CACHE_VERSION_BUMPED =
            "Cache version advanced. Epoch={Epoch}, NewVersion={NewVersion}.";

        /// <summary>
        /// INCR returned non-positive version (defensive case), service will normalize.
        /// </summary>
        public const string LOG_CACHE_VERSION_INCR_NON_POSITIVE =
            "Cache version increment returned a non-positive value. Epoch={Epoch}, NewVersion={NewVersion}.";

        /// <summary>
        /// Cache version invalidation was asked to run without a valid group or epoch.
        /// </summary>
        public const string CACHE_VERSION_INVALIDATION_ARGUMENTS_MISSING = "Cache version invalidation requires a non-empty cache group and epoch.";

        /// <summary>
        /// Cache version invalidation did not advance beyond the previous version.
        /// </summary>
        public const string CACHE_VERSION_NOT_ADVANCED = "Cache version invalidation did not advance. CacheGroup={0}, CacheScope={1}, Epoch={2}, OldVersion={3}, NewVersion={4}.";

        /// <summary>
        /// Cache version cannot be read safely, so caller must bypass cached payloads.
        /// </summary>
        public const string CACHE_VERSION_READ_UNSAFE = "Cache version cannot be read safely. CacheGroup={0}, CacheScope={1}, Epoch={2}.";

        /// <summary>
        /// TTL was applied to the version key.
        /// </summary>
        public const string LOG_CACHE_VERSION_TTL_APPLIED =
            "Cache version expiration applied. Epoch={Epoch}, TtlSeconds={TtlSeconds}, Applied={Applied}.";

        /// <summary>
        /// Failed to set TTL on version key (cleanup only).
        /// </summary>
        public const string LOG_CACHE_VERSION_TTL_SET_FAILED =
            "Cache version expiration update failed. Epoch={Epoch}, TtlSeconds={TtlSeconds}, NewVersion={NewVersion}.";
    }

    /// <summary>
    /// Centralized log message templates for caching pipeline behaviors.
    /// Keep templates stable to make log search/alerting consistent.
    /// </summary>
    public static class CacheLogs
    {
        /// <summary>
        /// Logged when a serialized cache value is found without exposing its key or payload.
        /// </summary>
        public const string CACHE_VALUE_FETCHED = "Fetched value from cache.";

        /// <summary>
        /// Logged when a serialized cache read does not find a value.
        /// </summary>
        public const string CACHE_VALUE_NOT_FOUND = "Cache value was not found.";

        /// <summary>
        /// Logged after a serialized cache value is stored.
        /// </summary>
        public const string CACHE_VALUE_ADDED = "Added value to cache.";

        /// <summary>
        /// Logged after a serialized cache value is removed.
        /// </summary>
        public const string CACHE_VALUE_REMOVED = "Removed value from cache.";

        /// <summary>
        /// Logged after an atomic cache reservation attempt completes.
        /// </summary>
        public const string ATOMIC_CACHE_RESERVATION_COMPLETED =
            "Atomic cache reservation completed. Created={Created}.";

        /// <summary>
        /// Logged after an atomic cache counter advances.
        /// </summary>
        public const string ATOMIC_CACHE_COUNTER_INCREMENTED =
            "Atomic cache counter incremented. Count={Count}.";

        /// <summary>
        /// Logged after an atomic cache entry is removed.
        /// </summary>
        public const string ATOMIC_CACHE_ENTRY_REMOVED = "Atomic cache entry removed.";

        /// <summary>
        /// Logged after a cache-bypass marker is stored.
        /// </summary>
        public const string CACHE_BYPASS_MARKED = "Cache bypass marker stored.";

        /// <summary>
        /// Logged after the current cache-bypass state is resolved.
        /// </summary>
        public const string CACHE_BYPASS_STATUS_READ =
            "Cache bypass status resolved. ShouldBypass={ShouldBypass}.";

        /// <summary>
        /// Query caching was bypassed explicitly by request flag.
        /// </summary>
        public const string LOG_CACHE_BYPASSED = "Cache bypassed. RequestType={RequestType}";

        /// <summary>
        /// Cache entry was not found for the computed key.
        /// </summary>
        public const string LOG_CACHE_MISS = "Cache miss. RequestType={RequestType} Epoch={Epoch} Version={Version}.";

        /// <summary>
        /// Cache invalidation failed. Command completed but cache version was not bumped.
        /// RequestType={RequestType}
        /// </summary>
        public const string CACHE_INVALIDATION_FAILED = "Cache invalidation failed. Command completed but cache version was not bumped. RequestType={RequestType}";

        /// <summary>
        /// Cache invalidation version was bumped after a successful write command.
        /// </summary>
        public const string CACHE_VERSION_BUMPED =
            "Cache version bumped. CacheGroup={CacheGroup}, CacheScope={CacheScope}, Epoch={Epoch}, NewVersion={NewVersion}.";

        /// <summary>
        /// Cache invalidation failed for one logical cache target.
        /// </summary>
        public const string CACHE_TARGET_INVALIDATION_FAILED =
            "Cache target invalidation failed. RequestType={RequestType}, CacheGroup={CacheGroup}, CacheScope={CacheScope}.";

        /// <summary>
        /// Cached read was bypassed because the scope had a previous consistency-critical invalidation failure.
        /// </summary>
        public const string CACHE_SCOPE_BYPASSED_AFTER_INVALIDATION_FAILURE =
            "Cache scope bypassed after an invalidation failure.";

        /// <summary>
        /// Cache bypass marker write failed after a consistency-critical invalidation failure.
        /// </summary>
        public const string CACHE_BYPASS_MARK_FAILED = "Cache bypass marker write failed.";

        /// <summary>
        /// Cache bypass marker read failed; caller will continue with normal cache safety checks.
        /// </summary>
        public const string CACHE_BYPASS_READ_FAILED = "Cache bypass marker read failed.";

        /// <summary>
        /// Cached read was bypassed because the cache version could not be read safely.
        /// </summary>
        public const string CACHE_VERSION_READ_BYPASSED =
            "Cache version read failed; bypassing cache. RequestType={RequestType}.";

        /// <summary>
        /// Cached payload read failed, so the handler result was loaded directly.
        /// </summary>
        public const string CACHE_READ_BYPASSED =
            "Cache read failed; bypassing cache. RequestType={RequestType}.";

        /// <summary>
        /// Cache write failed after a fresh handler result was produced.
        /// </summary>
        public const string CACHE_WRITE_SKIPPED =
            "Cache write failed; returning fresh response without caching. RequestType={RequestType}.";

        /// <summary>
        /// Current-user scoped cache could not resolve a principal and falls back to unscoped behavior.
        /// </summary>
        public const string LOG_CACHE_CURRENT_USER_SCOPE_SKIPPED =
            "Current-user cache scope skipped because principal is not authenticated. RequestType={RequestType}";

        /// <summary>
        /// Current-user scoped cache resolved its user namespace from the authenticated principal.
        /// </summary>
        public const string LOG_CACHE_CURRENT_USER_SCOPE_RESOLVED =
            "Current-user cache scope resolved from authenticated principal. RequestType={RequestType}";
    }

    /// <summary>
    /// Centralized log message templates for private Cloudflare R2 object uploads.
    /// </summary>
    public static class R2StorageLogs
    {
        /// <summary>
        /// Logged after a private object upload completes without exposing the object key.
        /// </summary>
        public const string R2_UPLOAD_COMPLETED = "Cloudflare R2 upload completed. BucketName={BucketName}, FileSize={FileSize}.";

        /// <summary>
        /// Logged when a private object upload fails without exposing the object key.
        /// </summary>
        public const string R2_UPLOAD_FAILED =
            "Cloudflare R2 upload failed. BucketName={BucketName}, StatusCode={StatusCode}, ResponseContent={ResponseContent}.";

        /// <summary>
        /// Logged after a private object delete completes without exposing the object key.
        /// </summary>
        public const string R2_DELETE_COMPLETED = "Cloudflare R2 delete completed. BucketName={BucketName}.";

        /// <summary>
        /// Logged when a private object delete fails without exposing the object key.
        /// </summary>
        public const string R2_DELETE_FAILED =
            "Cloudflare R2 delete failed. BucketName={BucketName}, StatusCode={StatusCode}, ResponseContent={ResponseContent}.";
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
        /// Log when reading version from cache fails and callers should bypass cached payloads.
        /// </summary>
        public const string LOG_IN_MEMORY_CACHE_VERSION_READ_FAILED =
            "In-memory cache version read failed. Epoch={Epoch}.";

        /// <summary>
        /// Log when writing version to cache fails and we retry (best-effort).
        /// </summary>
        public const string LOG_IN_MEMORY_CACHE_VERSION_WRITE_FAILED =
            "In-memory cache version write failed. Attempt={Attempt}, MaxAttempts={MaxAttempts}, Epoch={Epoch}.";

        /// <summary>
        /// In-memory cache version write failed after all configured retry attempts.
        /// </summary>
        public const string IN_MEMORY_CACHE_VERSION_WRITE_EXHAUSTED =
            "In-memory cache version write failed after all retry attempts. Epoch={0}.";
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
        /// Logged when a connection string is initialized from the GCP Secret Manager.
        /// </summary>
        public const string LOG_INIT_FROM_GCP = "Connection string initialized from GCP Secret Manager. ConnectionName={ConnectionName}, SecretId={SecretId}";

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
        public const string SEND_FAILED =
            "SendGrid email send failed. StatusCode={StatusCode}, ResponseContent={ResponseContent}.";

        /// <summary>
        /// Log message when an unexpected exception occurs while sending email.
        /// </summary>
        public const string SEND_EXCEPTION =
            "Error occurred while sending email. StatusCode={StatusCode}, ResponseContent={ResponseContent}.";
    }
}
