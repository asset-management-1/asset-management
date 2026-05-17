namespace Authentication.Application.Constants;

/// <summary>
/// Stores application-layer structured log messages for authentication workflows.
/// </summary>
public static class ApplicationLogConstants
{
    /// <summary>
    /// Logged after local login confirms the username is not temporarily locked.
    /// </summary>
    public const string LOCAL_LOGIN_FLOW_STEP1_LOCK_CHECKED =
        "Local login flow Step1: Temporary login lock checked.";

    /// <summary>
    /// Logged after local login credentials are accepted and a token pair is issued.
    /// </summary>
    public const string LOCAL_LOGIN_FLOW_STEP2_CREDENTIALS_ACCEPTED =
        "Local login flow Step2: Credentials accepted and token pair issued.";

    /// <summary>
    /// Logged after local login clears failed-attempt state following a successful sign-in.
    /// </summary>
    public const string LOCAL_LOGIN_FLOW_STEP3_ATTEMPTS_RESET =
        "Local login flow Step3: Failed-attempt state reset.";

    /// <summary>
    /// Logged after local login records an invalid credential attempt.
    /// </summary>
    public const string LOCAL_LOGIN_FLOW_INVALID_ATTEMPT_COUNTED =
        "Local login flow: Invalid credential attempt counted.";

    /// <summary>
    /// Logged after local username/password login flow completes.
    /// </summary>
    public const string LOCAL_LOGIN_COMPLETED = "Local login completed.";

    /// <summary>
    /// Logged after external provider login flow completes.
    /// </summary>
    public const string EXTERNAL_LOGIN_COMPLETED = "External login completed for provider {Provider}.";

    /// <summary>
    /// Logged after a refresh token is exchanged for a new token pair.
    /// </summary>
    public const string REFRESH_TOKEN_ROTATED = "Refresh token rotated.";

    /// <summary>
    /// Logged after register request values are normalized and the party type is accepted.
    /// </summary>
    public const string REGISTER_FLOW_STEP1_REQUEST_NORMALIZED =
        "Register flow Step1: Request normalized and party type validated.";

    /// <summary>
    /// Logged when register is blocked by the resend cooldown window.
    /// </summary>
    public const string REGISTER_FLOW_SKIPPED_COOLDOWN =
        "Register flow skipped: OTP cooldown is active.";

    /// <summary>
    /// Logged when register is blocked by the OTP request limit.
    /// </summary>
    public const string REGISTER_FLOW_SKIPPED_THROTTLED =
        "Register flow skipped: OTP request limit reached.";

    /// <summary>
    /// Logged after register OTP request throttling accepts the request.
    /// </summary>
    public const string REGISTER_FLOW_STEP2_THROTTLE_ACCEPTED =
        "Register flow Step2: OTP throttle accepted.";

    /// <summary>
    /// Logged after pending registration payload is built for caching.
    /// </summary>
    public const string REGISTER_FLOW_STEP3_PENDING_BUILT =
        "Register flow Step3: Pending registration payload built.";

    /// <summary>
    /// Logged after the register session cache entry is overwritten.
    /// </summary>
    public const string REGISTER_FLOW_STEP4_SESSION_CACHED =
        "Register flow Step4: Register session cache overwritten.";

    /// <summary>
    /// Logged after register OTP email is sent successfully.
    /// </summary>
    public const string REGISTER_FLOW_STEP5_OTP_SENT =
        "Register flow Step5: Register OTP email sent.";

    /// <summary>
    /// Logged when register OTP delivery fails and the register session is removed.
    /// </summary>
    public const string REGISTER_FLOW_ROLLBACK_OTP_SEND_FAILED =
        "Register flow rollback: OTP delivery failed and register session state was removed.";

    /// <summary>
    /// Logged after register cooldown is stored and the public response can be returned.
    /// </summary>
    public const string REGISTER_FLOW_COMPLETED = "Register flow completed.";

    /// <summary>
    /// Logged after register session cache is loaded for email verification.
    /// </summary>
    public const string VERIFY_REGISTER_FLOW_STEP1_SESSION_LOADED =
        "Verify register flow Step1: Register session loaded.";

    /// <summary>
    /// Logged after register OTP is verified.
    /// </summary>
    public const string VERIFY_REGISTER_FLOW_STEP2_OTP_VERIFIED =
        "Verify register flow Step2: OTP verified.";

    /// <summary>
    /// Logged after register verification creates the account graph.
    /// </summary>
    public const string VERIFY_REGISTER_FLOW_STEP3_ACCOUNT_CREATED =
        "Verify register flow Step3: Account graph created.";

    /// <summary>
    /// Logged after register session cache state is removed.
    /// </summary>
    public const string VERIFY_REGISTER_FLOW_STEP4_SESSION_REMOVED =
        "Verify register flow Step4: Register session state removed.";

    /// <summary>
    /// Logged when an invalid register OTP attempt is stored for retry limiting.
    /// </summary>
    public const string VERIFY_REGISTER_FLOW_OTP_ATTEMPT_RECORDED =
        "Verify register flow: Failed OTP attempt recorded.";

    /// <summary>
    /// Logged when register OTP state is removed after maximum failed attempts.
    /// </summary>
    public const string VERIFY_REGISTER_FLOW_OTP_LOCKED =
        "Verify register flow: OTP removed after maximum failed attempts.";

    /// <summary>
    /// Logged after register email verification completes.
    /// </summary>
    public const string VERIFY_REGISTER_FLOW_COMPLETED =
        "Verify register flow completed.";

    /// <summary>
    /// Logged after the current user switches party context.
    /// </summary>
    public const string CONTEXT_SWITCHED = "User switched party context to {Context} for user {UserPublicId}.";

    /// <summary>
    /// Logged after an external provider is linked.
    /// </summary>
    public const string PROVIDER_LINKED = "External provider {Provider} linked for user {UserPublicId}.";

    /// <summary>
    /// Logged after an external provider is unlinked.
    /// </summary>
    public const string PROVIDER_UNLINKED = "External provider {Provider} unlinked for user {UserPublicId}.";

    /// <summary>
    /// Logged after logout command flow completes.
    /// </summary>
    public const string LOGOUT_COMPLETED = "Logout completed for user {UserPublicId}.";

    /// <summary>
    /// Logged after forgot-password request values are normalized.
    /// </summary>
    public const string FORGOT_PASSWORD_FLOW_STEP1_REQUEST_NORMALIZED =
        "Forgot-password flow Step1: Request normalized.";

    /// <summary>
    /// Logged when forgot-password is blocked by cooldown or request limit but still returns the generic response.
    /// </summary>
    public const string FORGOT_PASSWORD_FLOW_SKIPPED_THROTTLED =
        "Forgot-password flow skipped: Cooldown or request limit reached; generic response returned.";

    /// <summary>
    /// Logged after forgot-password OTP throttling accepts the request.
    /// </summary>
    public const string FORGOT_PASSWORD_FLOW_STEP2_THROTTLE_ACCEPTED =
        "Forgot-password flow Step2: OTP throttle accepted.";

    /// <summary>
    /// Logged after forgot-password account lookup finishes without exposing whether the account exists.
    /// </summary>
    public const string FORGOT_PASSWORD_FLOW_STEP3_ACCOUNT_LOOKUP_COMPLETED =
        "Forgot-password flow Step3: Account lookup completed.";

    /// <summary>
    /// Logged after forgot-password intentionally returns a generic response for a missing account.
    /// </summary>
    public const string FORGOT_PASSWORD_FLOW_MISSING_ACCOUNT_GENERIC_RESPONSE =
        "Forgot-password flow: Missing account branch returned generic response.";

    /// <summary>
    /// Logged after forgot-password OTP state is cached.
    /// </summary>
    public const string FORGOT_PASSWORD_FLOW_STEP4_OTP_CACHED =
        "Forgot-password flow Step4: Reset OTP cached.";

    /// <summary>
    /// Logged after forgot-password OTP email is sent successfully.
    /// </summary>
    public const string FORGOT_PASSWORD_FLOW_STEP5_OTP_SENT =
        "Forgot-password flow Step5: Forgot-password OTP email sent.";

    /// <summary>
    /// Logged when forgot-password OTP delivery fails and generated OTP state is removed.
    /// </summary>
    public const string FORGOT_PASSWORD_FLOW_ROLLBACK_OTP_SEND_FAILED =
        "Forgot-password flow rollback: OTP delivery failed and generated OTP state was removed.";

    /// <summary>
    /// Logged after forgot-password returns the generic public response.
    /// </summary>
    public const string FORGOT_PASSWORD_FLOW_COMPLETED_GENERIC =
        "Forgot-password flow completed with generic public response.";

    /// <summary>
    /// Logged after forgot-password OTP verification request values are normalized.
    /// </summary>
    public const string VERIFY_FORGOT_PASSWORD_FLOW_STEP1_REQUEST_NORMALIZED =
        "Verify forgot-password OTP flow Step1: Request normalized.";

    /// <summary>
    /// Logged after forgot-password OTP is verified and consumed.
    /// </summary>
    public const string VERIFY_FORGOT_PASSWORD_FLOW_STEP2_OTP_VERIFIED =
        "Verify forgot-password OTP flow Step2: OTP verified and consumed.";

    /// <summary>
    /// Logged after forgot-password reset session is opened.
    /// </summary>
    public const string VERIFY_FORGOT_PASSWORD_FLOW_STEP3_RESET_SESSION_CREATED =
        "Verify forgot-password OTP flow Step3: Reset session created.";

    /// <summary>
    /// Logged when an invalid forgot-password OTP attempt is stored for retry limiting.
    /// </summary>
    public const string VERIFY_FORGOT_PASSWORD_FLOW_OTP_ATTEMPT_RECORDED =
        "Verify forgot-password OTP flow: Failed OTP attempt recorded.";

    /// <summary>
    /// Logged when forgot-password OTP state is removed after maximum failed attempts.
    /// </summary>
    public const string VERIFY_FORGOT_PASSWORD_FLOW_OTP_LOCKED =
        "Verify forgot-password OTP flow: OTP removed after maximum failed attempts.";

    /// <summary>
    /// Logged after forgot-password reset session is confirmed before password change.
    /// </summary>
    public const string CHANGE_FORGOT_PASSWORD_FLOW_STEP1_RESET_SESSION_VALIDATED =
        "Change forgot-password flow Step1: Reset session validated.";

    /// <summary>
    /// Logged after forgot-password password change and session revocation complete.
    /// </summary>
    public const string CHANGE_FORGOT_PASSWORD_FLOW_STEP2_PASSWORD_CHANGED =
        "Change forgot-password flow Step2: Password changed and sessions revoked.";

    /// <summary>
    /// Logged after forgot-password reset session state is consumed.
    /// </summary>
    public const string CHANGE_FORGOT_PASSWORD_FLOW_STEP3_RESET_SESSION_CONSUMED =
        "Change forgot-password flow Step3: Reset session consumed.";

    /// <summary>
    /// Logged after forgot-password password change command flow completes.
    /// </summary>
    public const string FORGOT_PASSWORD_CHANGE_COMPLETED = "Forgot-password change completed.";

    /// <summary>
    /// Logged after authenticated password change succeeds.
    /// </summary>
    public const string PASSWORD_CHANGED = "Password changed for user {UserPublicId}.";

    /// <summary>
    /// Logged after current-user profile update command flow completes.
    /// </summary>
    public const string USER_INFO_UPDATED = "User info updated for user {UserPublicId}.";

    /// <summary>
    /// Logged after current-user info query flow completes.
    /// </summary>
    public const string USER_INFO_LOADED = "User info loaded.";

    /// <summary>
    /// Logged after change-email resolves the current user and confirms cooldown is clear.
    /// </summary>
    public const string CHANGE_EMAIL_FLOW_STEP1_CURRENT_USER_RESOLVED =
        "Change-email flow Step1: Current user resolved and cooldown cleared.";

    /// <summary>
    /// Logged after the new email is validated and normalized for ownership verification.
    /// </summary>
    public const string CHANGE_EMAIL_FLOW_STEP2_TARGET_PREPARED =
        "Change-email flow Step2: Target email prepared.";

    /// <summary>
    /// Logged after change-email session cache is overwritten.
    /// </summary>
    public const string CHANGE_EMAIL_FLOW_STEP3_SESSION_CACHED =
        "Change-email flow Step3: Change-email session cache overwritten.";

    /// <summary>
    /// Logged after change-email OTP is sent to the new email.
    /// </summary>
    public const string CHANGE_EMAIL_FLOW_STEP4_OTP_SENT =
        "Change-email flow Step4: Change-email OTP sent.";

    /// <summary>
    /// Logged when change-email OTP delivery fails and session state is removed.
    /// </summary>
    public const string CHANGE_EMAIL_FLOW_ROLLBACK_OTP_SEND_FAILED =
        "Change-email flow rollback: OTP delivery failed and change-email session state was removed.";

    /// <summary>
    /// Logged after best-effort old-email security notification is attempted.
    /// </summary>
    public const string CHANGE_EMAIL_FLOW_STEP5_SECURITY_NOTIFICATION_ATTEMPTED =
        "Change-email flow Step5: Old-email security notification attempted.";

    /// <summary>
    /// Logged after change-email cooldown state is stored.
    /// </summary>
    public const string CHANGE_EMAIL_FLOW_STEP6_COOLDOWN_SET =
        "Change-email flow Step6: Cooldown stored.";

    /// <summary>
    /// Logged after change-email OTP send flow completes.
    /// </summary>
    public const string CHANGE_EMAIL_FLOW_COMPLETED =
        "Change-email flow completed for user {UserPublicId}.";

    /// <summary>
    /// Logged after change-email session state is validated against the submitted email.
    /// </summary>
    public const string VERIFY_CHANGE_EMAIL_FLOW_STEP1_SESSION_VALIDATED =
        "Verify change-email flow Step1: Change-email session state validated.";

    /// <summary>
    /// Logged after change-email OTP is verified.
    /// </summary>
    public const string VERIFY_CHANGE_EMAIL_FLOW_STEP2_OTP_VERIFIED =
        "Verify change-email flow Step2: OTP verified.";

    /// <summary>
    /// Logged after the verified new email is persisted.
    /// </summary>
    public const string VERIFY_CHANGE_EMAIL_FLOW_STEP3_EMAIL_UPDATED =
        "Verify change-email flow Step3: Email updated.";

    /// <summary>
    /// Logged after change-email session cache state and cooldown are removed.
    /// </summary>
    public const string VERIFY_CHANGE_EMAIL_FLOW_STEP4_STATE_CLEANED =
        "Verify change-email flow Step4: Session state cleaned.";

    /// <summary>
    /// Logged when an invalid change-email OTP attempt is stored for retry limiting.
    /// </summary>
    public const string VERIFY_CHANGE_EMAIL_FLOW_OTP_ATTEMPT_RECORDED =
        "Verify change-email flow: Failed OTP attempt recorded.";

    /// <summary>
    /// Logged when change-email OTP state is removed after maximum failed attempts.
    /// </summary>
    public const string VERIFY_CHANGE_EMAIL_FLOW_OTP_LOCKED =
        "Verify change-email flow: OTP removed after maximum failed attempts.";

    /// <summary>
    /// Logged after change-email verification completes.
    /// </summary>
    public const string VERIFY_CHANGE_EMAIL_FLOW_COMPLETED =
        "Verify change-email flow completed for user {UserPublicId}.";

    /// <summary>
    /// Logged after current-user KYC submission command flow is accepted for manual review.
    /// </summary>
    public const string KYC_SUBMITTED = "KYC submitted for manual review by user {UserPublicId}.";

    /// <summary>
    /// Logged after the current user's tenant vehicles are loaded.
    /// </summary>
    public const string USER_VEHICLES_LOADED = "User vehicles loaded for user {UserPublicId}.";

    /// <summary>
    /// Logged after a tenant profile vehicle registration command completes.
    /// </summary>
    public const string USER_VEHICLE_REGISTERED = "User vehicle registered for user {UserPublicId}.";

    /// <summary>
    /// Logged after a tenant profile vehicle delete command completes.
    /// </summary>
    public const string USER_VEHICLE_DELETED = "User vehicle deleted for user {UserPublicId}.";

    /// <summary>
    /// Logged when old-email security notification cannot be delivered.
    /// </summary>
    public const string CHANGE_EMAIL_SECURITY_NOTIFICATION_FAILED =
        "Change-email security notification failed for user {UserPublicId}; continuing email-change flow.";
}
