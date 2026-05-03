namespace Authentication.Application.Constants;

public static class ApplicationConstants
{
    public const string CONFIRM_PASSWORD_MUST_MATCH_PASSWORD = "ConfirmPassword must match Password.";

    public const string CONFIRM_PASSWORD_MUST_MATCH_NEW_PASSWORD = "ConfirmPassword must match NewPassword.";

    public const string REFRESH_TOKEN_REQUIRED_WHEN_LOGOUT_SINGLE_SESSION = "RefreshToken is required when LogoutAllSessions is false.";

    public const string UNAUTHORIZED_REQUEST_MESSAGE = "Unauthorized request.";
}
