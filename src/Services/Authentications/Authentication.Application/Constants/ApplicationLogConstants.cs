namespace Authentication.Application.Constants;

/// <summary>
/// Stores application-layer structured log messages for authentication workflows.
/// </summary>
public static class ApplicationLogConstants
{
    /// <summary>
    /// Contains local username/password login log messages.
    /// </summary>
    public static class LoginLogs
    {
        /// <summary>
        /// Logged after local login records an invalid credential attempt.
        /// </summary>
        public const string LOCAL_LOGIN_FLOW_INVALID_ATTEMPT_COUNTED = "Local login flow: Invalid credential attempt counted.";
    }

    /// <summary>
    /// Contains registration and registration-verification log messages.
    /// </summary>
    public static class RegisterLogs
    {
        /// <summary>
        /// Logged when register is blocked by the resend cooldown window.
        /// </summary>
        public const string REGISTER_FLOW_SKIPPED_COOLDOWN = "Register flow skipped: OTP cooldown is active.";

        /// <summary>
        /// Logged when register is blocked by the OTP request limit.
        /// </summary>
        public const string REGISTER_FLOW_SKIPPED_THROTTLED = "Register flow skipped: OTP request limit reached.";

        /// <summary>
        /// Logged when register OTP delivery fails and the register session is removed.
        /// </summary>
        public const string REGISTER_FLOW_ROLLBACK_OTP_SEND_FAILED = "Register flow rollback: OTP delivery failed and register session state was removed.";

        /// <summary>
        /// Logged after register cooldown is stored and the public response can be returned.
        /// </summary>
        public const string REGISTER_FLOW_COMPLETED = "Register flow completed.";

        /// <summary>
        /// Logged when register OTP state is removed after maximum failed attempts.
        /// </summary>
        public const string VERIFY_REGISTER_FLOW_OTP_LOCKED = "Verify register flow: OTP removed after maximum failed attempts.";

        /// <summary>
        /// Warning emitted when the registration consume marker cannot be released after account creation fails.
        /// </summary>
        public const string VERIFY_REGISTER_FLOW_CONSUME_CLEANUP_FAILED = "Verify register flow: Account creation failed and the OTP consume marker could not be released.";
    }

    /// <summary>
    /// Contains forgot-password and reset-session log messages.
    /// </summary>
    public static class ForgotPasswordLogs
    {
        /// <summary>
        /// Logged when forgot-password is blocked by cooldown or request limit but still returns the generic response.
        /// </summary>
        public const string FORGOT_PASSWORD_FLOW_SKIPPED_THROTTLED = "Forgot-password flow skipped: Cooldown or request limit reached; generic response returned.";

        /// <summary>
        /// Logged after forgot-password intentionally returns a generic response for a missing account.
        /// </summary>
        public const string FORGOT_PASSWORD_FLOW_MISSING_ACCOUNT_GENERIC_RESPONSE = "Forgot-password flow: Missing account branch returned generic response.";

        /// <summary>
        /// Logged when forgot-password OTP delivery fails and generated OTP state is removed.
        /// </summary>
        public const string FORGOT_PASSWORD_FLOW_ROLLBACK_OTP_SEND_FAILED = "Forgot-password flow rollback: OTP delivery failed and generated OTP state was removed.";

        /// <summary>
        /// Logged after forgot-password returns the generic public response.
        /// </summary>
        public const string FORGOT_PASSWORD_FLOW_COMPLETED_GENERIC = "Forgot-password flow completed with generic public response.";

        /// <summary>
        /// Logged after forgot-password OTP verification creates reset authority and removes obsolete OTP state.
        /// </summary>
        public const string VERIFY_FORGOT_PASSWORD_FLOW_COMPLETED = "Verify forgot-password OTP flow completed.";

        /// <summary>
        /// Logged when forgot-password OTP state is removed after maximum failed attempts.
        /// </summary>
        public const string VERIFY_FORGOT_PASSWORD_FLOW_OTP_LOCKED = "Verify forgot-password OTP flow: OTP removed after maximum failed attempts.";

        /// <summary>
        /// Warning emitted when obsolete OTP state could not be removed after reset authority was created.
        /// </summary>
        public const string VERIFY_FORGOT_PASSWORD_FLOW_OTP_CLEANUP_FAILED = "Verify forgot-password OTP flow: Reset session created, but obsolete OTP cleanup failed.";

        /// <summary>
        /// Warning emitted when the forgot-password consume marker cannot be released after reset-session creation fails.
        /// </summary>
        public const string VERIFY_FORGOT_PASSWORD_FLOW_CONSUME_CLEANUP_FAILED = "Verify forgot-password OTP flow: Reset-session creation failed and the OTP consume marker could not be released.";
    }

    /// <summary>
    /// Contains change-email and change-email verification log messages.
    /// </summary>
    public static class ChangeEmailLogs
    {
        /// <summary>
        /// Logged when change-email OTP delivery fails and session state is removed.
        /// </summary>
        public const string CHANGE_EMAIL_FLOW_ROLLBACK_OTP_SEND_FAILED = "Change-email flow rollback: OTP delivery failed and change-email session state was removed.";

        /// <summary>
        /// Logged after change-email OTP send flow completes.
        /// </summary>
        public const string CHANGE_EMAIL_FLOW_COMPLETED = "Change-email flow completed for user {UserPublicId}.";

        /// <summary>
        /// Logged when change-email OTP state is removed after maximum failed attempts.
        /// </summary>
        public const string VERIFY_CHANGE_EMAIL_FLOW_OTP_LOCKED = "Verify change-email flow: OTP removed after maximum failed attempts.";

        /// <summary>
        /// Warning emitted when the change-email consume marker cannot be released after persistence fails.
        /// </summary>
        public const string VERIFY_CHANGE_EMAIL_FLOW_CONSUME_CLEANUP_FAILED = "Verify change-email flow: Email persistence failed and the OTP consume marker could not be released.";

        /// <summary>
        /// Logged after change-email verification completes.
        /// </summary>
        public const string VERIFY_CHANGE_EMAIL_FLOW_COMPLETED = "Verify change-email flow completed for user {UserPublicId}.";

        /// <summary>
        /// Logged when old-email security notification cannot be delivered.
        /// </summary>
        public const string CHANGE_EMAIL_SECURITY_NOTIFICATION_FAILED = "Change-email security notification failed for user {UserPublicId}; continuing email-change flow.";
    }

    /// <summary>
    /// Contains current-user KYC log messages.
    /// </summary>
    public static class KycLogs
    {
        /// <summary>
        /// Logged after current-user KYC submission command flow is accepted for manual review.
        /// </summary>
        public const string KYC_SUBMITTED = "KYC submitted for manual review by user {UserPublicId}.";
    }

    /// <summary>
    /// Contains current-user profile and party-context log messages.
    /// </summary>
    public static class UserLogs
    {
        /// <summary>
        /// Logged after the current authenticated profile is loaded.
        /// </summary>
        public const string USER_INFO_LOADED = "Current-user profile loaded.";

        /// <summary>
        /// Logged after the current authenticated profile is updated.
        /// </summary>
        public const string USER_INFO_UPDATED = "Current-user profile updated.";

        /// <summary>
        /// Logged after the current client switches its active party context.
        /// </summary>
        public const string PARTY_CONTEXT_SWITCHED = "Current client party context switched.";
    }

    /// <summary>
    /// Contains session and token rotation log messages.
    /// </summary>
    public static class SessionLogs
    {
        /// <summary>
        /// Logged after the current authenticated session is revoked.
        /// </summary>
        public const string LOGOUT_COMPLETED = "Logout completed for user {UserPublicId} and session {SessionPublicId}.";

        /// <summary>
        /// Logged after refresh-token rotation returns a new token pair.
        /// </summary>
        public const string REFRESH_TOKEN_ROTATED = "Refresh-token rotation completed.";
    }

    /// <summary>
    /// Contains password mutation log messages.
    /// </summary>
    public static class PasswordLogs
    {
        /// <summary>
        /// Logged after forgot-password reset authority changes the password.
        /// </summary>
        public const string FORGOT_PASSWORD_CHANGED = "Forgot-password reset completed and prior sessions were revoked.";

        /// <summary>
        /// Logged after an authenticated password change rotates the current session.
        /// </summary>
        public const string PASSWORD_CHANGED = "Password changed for user {UserPublicId} and session {SessionPublicId}.";
    }

    /// <summary>
    /// Contains external authentication orchestration log messages.
    /// </summary>
    public static class ExternalLogs
    {
        /// <summary>
        /// Logged after an external provider is linked to the current account.
        /// </summary>
        public const string PROVIDER_LINKED = "External provider {Provider} linked for user {UserPublicId}.";

        /// <summary>
        /// Logged after an external provider is unlinked from the current account.
        /// </summary>
        public const string PROVIDER_UNLINKED = "External provider {Provider} unlinked for user {UserPublicId}.";

        /// <summary>
        /// Logged after external login resolves its existing-account or registration branch.
        /// </summary>
        public const string EXTERNAL_LOGIN_COMPLETED = "External login completed for provider {Provider}. RegistrationRequired={IsNewRegistration}.";

        /// <summary>
        /// Logged after first-time external registration creates the local account graph.
        /// </summary>
        public const string EXTERNAL_REGISTRATION_COMPLETED = "External registration completed for provider {Provider}.";
    }
}
