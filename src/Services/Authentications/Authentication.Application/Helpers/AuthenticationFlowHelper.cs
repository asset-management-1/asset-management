namespace Authentication.Application.Helpers;

/// <summary>
/// Shared stateless helpers for authentication application flows.
/// </summary>
public static class AuthenticationFlowHelper
{
    /// <summary>
    /// Determines whether a user can sign in.
    /// </summary>
    /// <param name="user">The user entity loaded for authentication.</param>
    /// <returns><c>true</c> when the user is active and email-confirmed; otherwise <c>false</c>.</returns>
    public static bool CanLogin(User user)
    {
        // Local login requires an active, non-deleted, email-confirmed identity.
        return user is not null
               && !user.IsDeleted
               && user.Status is not null
               && string.Equals(user.Status.Code, ACTIVE_STATUS, StringComparison.OrdinalIgnoreCase)
               && user.EmailConfirmed;
    }

    /// <summary>
    /// Converts a normalized party context into the party-type value used by master data.
    /// </summary>
    /// <param name="context">The normalized party-context value.</param>
    /// <returns>The matching party-type master-data code, or <c>null</c> when unsupported.</returns>
    public static string ToMasterDataPartyTypeValue(string context)
    {
        // Request context values map to persisted party-type master-data codes.
        var partyType = ApiEnumContractMapper.ToPartyType(context);

        return partyType.HasValue ? partyType.Value.ToMasterDataCode() : null;
    }

    /// <summary>
    /// Converts a party-type master-data code into the API party-context value.
    /// </summary>
    /// <param name="partyType">The party-type master-data code.</param>
    /// <returns>The matching API party context, or <c>null</c> when unsupported.</returns>
    public static string ToPartyContextValue(string partyType)
    {
        // Persisted party-type codes map back to API context values for clients.
        var context = ApiEnumContractMapper.ToPartyType(partyType);

        return context.HasValue ? context.Value.ToString().ToLowerInvariant() : null;
    }

    /// <summary>
    /// Generates a numeric OTP using the configured authentication OTP length.
    /// </summary>
    /// <returns>A zero-padded numeric OTP string.</returns>
    public static string GenerateOtp()
    {
        // Use the shared OTP length so generated codes and validators stay aligned.
        var maxExclusive = (int)Math.Pow(10, OTP_LENGTH);

        return RandomNumberGenerator.GetInt32(0, maxExclusive).ToString($"D{OTP_LENGTH}");
    }

    /// <summary>
    /// Builds the cache key for an OTP payload.
    /// </summary>
    /// <param name="purpose">The OTP purpose code.</param>
    /// <param name="normalizedEmail">The normalized email address.</param>
    /// <returns>The OTP payload cache key.</returns>
    public static string BuildOtpKey(string purpose, string normalizedEmail)
    {
        // OTP payload keys stay purpose-scoped so independent auth flows do not collide.
        return string.Format(OTP_KEY_PATTERN, purpose, normalizedEmail);
    }

    /// <summary>
    /// Builds the cache key for the register session payload.
    /// </summary>
    /// <param name="normalizedEmail">The normalized email address.</param>
    /// <returns>The register session cache key.</returns>
    public static string BuildRegisterSessionKey(string normalizedEmail)
    {
        // Register sessions are email-scoped because verification happens before a user exists.
        return string.Format(REGISTER_SESSION_KEY_PATTERN, normalizedEmail);
    }

    /// <summary>
    /// Builds the cache key for an OTP cooldown marker.
    /// </summary>
    /// <param name="purpose">The OTP purpose code.</param>
    /// <param name="normalizedEmail">The normalized email address.</param>
    /// <returns>The OTP cooldown cache key.</returns>
    public static string BuildOtpCooldownKey(string purpose, string normalizedEmail)
    {
        // Cooldown markers expire independently from the OTP payload and session data.
        return string.Format(OTP_COOLDOWN_KEY_PATTERN, purpose, normalizedEmail);
    }

    /// <summary>
    /// Builds the cache key for OTP request rate limits.
    /// </summary>
    /// <param name="purpose">The OTP purpose code.</param>
    /// <param name="normalizedEmail">The normalized email address.</param>
    /// <returns>The OTP rate-limit cache key.</returns>
    public static string BuildOtpLimitKey(string purpose, string normalizedEmail)
    {
        // Rate-limit counters are purpose-scoped so different OTP flows do not share attempt windows.
        return string.Format(OTP_LIMIT_KEY_PATTERN, purpose, normalizedEmail);
    }

    /// <summary>
    /// Builds the cache key for forgot-password reset session data.
    /// </summary>
    /// <param name="normalizedEmail">The normalized email address.</param>
    /// <returns>The reset-session cache key.</returns>
    public static string BuildResetSessionKey(string normalizedEmail)
    {
        // Reset sessions use the verified email as the pre-authenticated lookup subject.
        return string.Format(RESET_SESSION_KEY_PATTERN, normalizedEmail);
    }

    /// <summary>
    /// Builds the cache key for the change-email session payload.
    /// </summary>
    /// <param name="currentUserPublicId">The current user's public identifier.</param>
    /// <returns>The change-email session cache key.</returns>
    public static string BuildChangeEmailSessionKey(Guid currentUserPublicId)
    {
        // Change-email sessions are user-scoped so target-email changes overwrite the same pending state.
        return string.Format(CHANGE_EMAIL_SESSION_KEY_PATTERN, currentUserPublicId);
    }

    /// <summary>
    /// Builds the cache key for the change-email cooldown marker.
    /// </summary>
    /// <param name="currentUserPublicId">The current user's public identifier.</param>
    /// <returns>The change-email cooldown cache key.</returns>
    public static string BuildChangeEmailCooldownKey(Guid currentUserPublicId)
    {
        // Change-email cooldown is user-scoped to throttle resends across target-email changes.
        return string.Format(CHANGE_EMAIL_COOLDOWN_KEY_PATTERN, currentUserPublicId);
    }
}
