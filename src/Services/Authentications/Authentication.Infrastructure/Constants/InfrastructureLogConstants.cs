namespace Authentication.Infrastructure.Constants;

/// <summary>
/// Contains structured log-message constants used by the authentication infrastructure layer.
/// </summary>
public static class InfrastructureLogConstants
{
    /// <summary>
    /// Contains log messages related to register and logout flows.
    /// </summary>
    public static class SessionLogs
    {
        /// <summary>
        /// Logged after a client session is created.
        /// </summary>
        public const string CLIENT_SESSION_CREATED = "Client session {SessionPublicId} created for user {UserPublicId}.";

        /// <summary>
        /// Logged after a client session is replaced by a new login.
        /// </summary>
        public const string CLIENT_SESSION_REPLACED_BY_LOGIN = "Client session {SessionPublicId} replaced by login for user {UserPublicId}.";

        /// <summary>
        /// Logged after a client session is refreshed.
        /// </summary>
        public const string CLIENT_SESSION_REFRESHED = "Client session {SessionPublicId} refreshed for user {UserPublicId}.";

        /// <summary>
        /// Logged after the current client session is revoked.
        /// </summary>
        public const string CLIENT_SESSION_REVOKED = "Client session {SessionPublicId} revoked for user {UserPublicId}.";

        /// <summary>
        /// Logged after all active client sessions are revoked by a credential change.
        /// </summary>
        public const string CLIENT_SESSIONS_REVOKED_BY_CREDENTIAL_CHANGE = "All client sessions revoked by credential change for user {UserPublicId}.";

        /// <summary>
        /// Logged when an immediate previous refresh token is reused after rotation.
        /// </summary>
        public const string REFRESH_TOKEN_REUSE_REJECTED = "Refresh token reuse rejected for user {UserPublicId} and client session {SessionPublicId}.";

        /// <summary>
        /// Logged after a user completes register email verification.
        /// </summary>
        public const string REGISTER_COMPLETED = "User {UserPublicId} completed register email verification.";

        /// <summary>
        /// Logged when auth reset marker write to cache fails.
        /// </summary>
        public const string AUTH_RESET_CACHE_WRITE_FAILED = "Auth reset cache write failed for user {UserPublicId}.";

        /// <summary>
        /// Logged when best-effort auth reset marker cache seeding fails after token issue.
        /// </summary>
        public const string AUTH_RESET_CACHE_SEED_FAILED = "Auth reset cache seed failed after token issue for user {UserPublicId}.";
    }

    /// <summary>
    /// Contains log messages related to forgot-password and password-change flows.
    /// </summary>
    public static class PasswordLogs
    {
        /// <summary>
        /// Logged after a forgot-password change is completed.
        /// </summary>
        public const string FORGOT_PASSWORD_CHANGED = "Forgot-password change completed for user {UserPublicId}.";

        /// <summary>
        /// Logged after an authenticated user changes their password.
        /// </summary>
        public const string PASSWORD_CHANGED = "Password changed for user {UserPublicId}.";
    }

    /// <summary>
    /// Contains log messages related to external identity-provider flows.
    /// </summary>
    public static class ExternalProviderLogs
    {
        /// <summary>
        /// Logged after external provider login completes for a Haven account.
        /// </summary>
        public const string EXTERNAL_LOGIN_COMPLETED = "External login completed for provider {Provider} and user {UserPublicId}.";

        /// <summary>
        /// Logged after an external identity provider is linked to a local account.
        /// </summary>
        public const string PROVIDER_LINKED = "External provider {Provider} linked for user {UserPublicId}.";

        /// <summary>
        /// Logged after an external identity provider is unlinked from a local account.
        /// </summary>
        public const string PROVIDER_UNLINKED = "External provider {Provider} unlinked for user {UserPublicId}.";

        /// <summary>
        /// Logged when external token validation fails.
        /// </summary>
        public const string TOKEN_VALIDATION_FAILED = "External token validation failed for provider {Provider}.";
    }

    /// <summary>
    /// Contains log messages related to party-context switching flows.
    /// </summary>
    public static class ContextLogs
    {
        /// <summary>
        /// Logged after a user switches active party context.
        /// </summary>
        public const string CONTEXT_SWITCHED = "Party context {Context} activated for user {UserPublicId}.";

        /// <summary>
        /// Logged when switching to an already linked party context.
        /// </summary>
        public const string EXISTING_CONTEXT_ACTIVATED = "Existing party context activated for user {UserPublicId}.";

        /// <summary>
        /// Logged when switching creates a missing party context.
        /// </summary>
        public const string NEW_CONTEXT_CREATED = "New party context created for user {UserPublicId}.";
    }

    /// <summary>
    /// Contains log messages related to outbound authentication email delivery.
    /// </summary>
    public static class EmailLogs
    {
        /// <summary>
        /// Logged when OTP email delivery fails.
        /// </summary>
        public const string OTP_SEND_FAILED = "OTP email send failed for auth purpose {Purpose}.";

        /// <summary>
        /// Logged when a change-email security notification has no old email target.
        /// </summary>
        public const string CHANGE_EMAIL_SECURITY_NOTIFICATION_SKIPPED = "Change-email security notification skipped because old email is missing.";

        /// <summary>
        /// Logged when the old-email change-email security notification cannot be delivered.
        /// </summary>
        public const string CHANGE_EMAIL_SECURITY_NOTIFICATION_FAILED = "Change-email security notification email send failed.";
    }

    /// <summary>
    /// Contains log messages related to current-user profile and KYC flows.
    /// </summary>
    public static class UserLogs
    {
        /// <summary>
        /// Logged after current-user information is loaded.
        /// </summary>
        public const string USER_INFO_LOADED = "User info loaded for user {UserPublicId}.";

        /// <summary>
        /// Logged when an authenticated user flow cannot resolve the user-id claim.
        /// </summary>
        public const string CURRENT_USER_CLAIM_MISSING = "Current user id claim missing for authenticated user flow.";

        /// <summary>
        /// Logged when the current user public id does not match an active user row.
        /// </summary>
        public const string CURRENT_USER_NOT_FOUND = "Current user not found for user {UserPublicId}.";

        /// <summary>
        /// Logged after current-user profile information is updated.
        /// </summary>
        public const string USER_INFO_UPDATED = "User info updated for user {UserPublicId}.";

        /// <summary>
        /// Logged when avatar upload fails before profile persistence.
        /// </summary>
        public const string USER_AVATAR_UPLOAD_FAILED = "User avatar upload failed for user {UserPublicId}.";

        /// <summary>
        /// Logged after an optional avatar upload completes.
        /// </summary>
        public const string USER_AVATAR_UPLOAD_COMPLETED = "User avatar upload completed for user {UserPublicId}.";

        /// <summary>
        /// Logged when profile persistence fails after optional avatar upload.
        /// </summary>
        public const string USER_INFO_UPDATE_PERSISTENCE_FAILED = "User info update persistence failed for user {UserPublicId}. Uploaded objects will be cleaned up.";

        /// <summary>
        /// Logged after a change-email request is validated and prepared.
        /// </summary>
        public const string CHANGE_EMAIL_PREPARED = "Change-email request prepared for user {UserPublicId}.";

        /// <summary>
        /// Logged after a verified change-email request updates the account.
        /// </summary>
        public const string CHANGE_EMAIL_VERIFIED = "Change-email verified for user {UserPublicId}.";

        /// <summary>
        /// Logged after KYC metadata is persisted for manual admin review.
        /// </summary>
        public const string KYC_SUBMITTED = "KYC submitted for manual review by user {UserPublicId}.";

        /// <summary>
        /// Logged when a KYC submission is blocked because the account already has an active review state.
        /// </summary>
        public const string KYC_REUPLOAD_BLOCKED = "KYC re-upload blocked for user {UserPublicId} because current review state is not rejected.";

        /// <summary>
        /// Logged when KYC submission starts after the current user is resolved.
        /// </summary>
        public const string KYC_SUBMISSION_STARTED = "KYC submission started for user {UserPublicId}.";

        /// <summary>
        /// Logged after KYC master-data values are resolved in one batch.
        /// </summary>
        public const string KYC_MASTER_DATA_RESOLVED = "KYC master data resolved for user {UserPublicId}.";

        /// <summary>
        /// Logged after KYC party identifier data is staged for review.
        /// </summary>
        public const string KYC_IDENTIFIER_STAGED = "KYC identifier staged for user {UserPublicId}.";

        /// <summary>
        /// Logged when KYC metadata persistence fails after private file upload.
        /// </summary>
        public const string KYC_SUBMISSION_PERSISTENCE_FAILED = "KYC submission persistence failed for user {UserPublicId}. Uploaded objects will be cleaned up.";

        /// <summary>
        /// Logged when a KYC file upload fails before metadata persistence.
        /// </summary>
        public const string KYC_UPLOAD_FAILED = "KYC upload failed for user {UserPublicId}.";

        /// <summary>
        /// Logged after both private KYC document uploads complete.
        /// </summary>
        public const string KYC_UPLOAD_COMPLETED = "KYC private file upload completed for user {UserPublicId}.";

        /// <summary>
        /// Logged when uploaded object cleanup fails after persistence does not complete.
        /// </summary>
        public const string UPLOADED_OBJECT_CLEANUP_FAILED = "Uploaded object cleanup failed for user {UserPublicId}.";
    }

}
