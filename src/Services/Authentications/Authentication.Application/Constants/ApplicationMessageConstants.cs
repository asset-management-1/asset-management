namespace Authentication.Application.Constants;

/// <summary>
/// Stores authentication application user-facing success and status messages grouped by owning workflow.
/// </summary>
public static class ApplicationMessageConstants
{
    /// <summary>
    /// Contains email subjects used by authentication application workflows.
    /// </summary>
    public static class EmailMessages
    {
        /// <summary>
        /// Email subject used when sending register verification OTP emails.
        /// </summary>
        public const string EMAIL_SUBJECT_VERIFY_ACCOUNT = "Verify your Haven account";

        /// <summary>
        /// Email subject used when sending forgot-password OTP emails.
        /// </summary>
        public const string EMAIL_SUBJECT_RESET_PASSWORD = "Reset your Haven password";

        /// <summary>
        /// Email subject used when sending change-email OTP emails to the new address.
        /// </summary>
        public const string EMAIL_SUBJECT_CHANGE_EMAIL = "Verify your new Haven email";

        /// <summary>
        /// Email subject used when notifying the previous address about a change-email request.
        /// </summary>
        public const string EMAIL_SUBJECT_CHANGE_EMAIL_SECURITY = "Security notice: email change requested";
    }

    /// <summary>
    /// Contains OTP and password-recovery success messages.
    /// </summary>
    public static class OtpMessages
    {
        /// <summary>
        /// Generic success message returned by forgot-password OTP requests.
        /// </summary>
        public const string FORGOT_PASSWORD_SUCCESS_MESSAGE = "If the account exists, an OTP has been sent.";

        /// <summary>
        /// Success message returned after OTP verification.
        /// </summary>
        public const string OTP_VERIFIED_SUCCESS_MESSAGE = "OTP verified successfully.";

        /// <summary>
        /// Success message returned after an OTP is sent.
        /// </summary>
        public const string OTP_SENT_MESSAGE = "OTP sent";
    }

    /// <summary>
    /// Contains password operation success messages.
    /// </summary>
    public static class PasswordMessages
    {
        /// <summary>
        /// Success message returned after a password change.
        /// </summary>
        public const string PASSWORD_CHANGED_SUCCESS_MESSAGE = "Password changed successfully.";
    }

    /// <summary>
    /// Contains session operation success messages.
    /// </summary>
    public static class SessionMessages
    {
        /// <summary>
        /// Success message returned after logout.
        /// </summary>
        public const string LOGOUT_SUCCESS_MESSAGE = "Logged out successfully.";
    }

    /// <summary>
    /// Contains external identity provider success messages.
    /// </summary>
    public static class ExternalProviderMessages
    {
        /// <summary>
        /// Success message returned after linking an external provider.
        /// </summary>
        public const string EXTERNAL_PROVIDER_LINKED_SUCCESS_MESSAGE = "External provider linked successfully.";

        /// <summary>
        /// Success message returned after unlinking an external provider.
        /// </summary>
        public const string EXTERNAL_PROVIDER_UNLINKED_SUCCESS_MESSAGE = "External provider unlinked successfully.";
    }

    /// <summary>
    /// Contains registration and context-switch success messages.
    /// </summary>
    public static class AccountMessages
    {
        /// <summary>
        /// Success message returned after registration is completed.
        /// </summary>
        public const string REGISTRATION_COMPLETED_SUCCESS_MESSAGE = "Registration completed successfully.";

        /// <summary>
        /// Success message returned after switching party context.
        /// </summary>
        public const string SWITCH_PARTY_SUCCESS_MESSAGE = "Party context switched successfully.";
    }

    /// <summary>
    /// Contains profile and email-change success messages.
    /// </summary>
    public static class ProfileMessages
    {
        /// <summary>
        /// Success message returned after current-user profile fields are updated.
        /// </summary>
        public const string USER_INFO_UPDATED_SUCCESS_MESSAGE = "User info updated successfully.";

        /// <summary>
        /// Success message returned after sending change-email OTP.
        /// </summary>
        public const string CHANGE_EMAIL_OTP_SENT_MESSAGE = "Change email OTP sent.";

        /// <summary>
        /// Success message returned after verifying and applying a new email.
        /// </summary>
        public const string CHANGE_EMAIL_COMPLETED_SUCCESS_MESSAGE = "Email changed successfully.";
    }

    /// <summary>
    /// Contains KYC success messages.
    /// </summary>
    public static class KycMessages
    {
        /// <summary>
        /// Success message returned after a KYC submission is accepted for review.
        /// </summary>
        public const string KYC_SUBMITTED_SUCCESS_MESSAGE = "KYC submitted for review.";
    }
}
