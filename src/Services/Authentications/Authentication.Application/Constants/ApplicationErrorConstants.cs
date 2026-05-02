namespace Authentication.Application.Constants;

public static class ApplicationErrorConstants
{
    public const string PASSWORD_COMPLEXITY_RULES = "Password must meet complexity rules (min 8 chars, include uppercase, lowercase, number, and special character).";

    public const string AUTH_INVALID_CREDENTIALS = "error_auth_invalid_credentials";

    public const string AUTH_UNAUTHORIZED = "error_unauthorized";

    public const string AUTH_USER_ALREADY_EXISTS = "error_auth_user_already_exists";

    public const string AUTH_USER_STATUS_NOT_FOUND = "error_auth_user_status_not_found";

    public const string AUTH_INVALID_REFRESH_TOKEN = "error_auth_invalid_refresh_token";

    public const string AUTH_FORBIDDEN_OPERATION = "error_auth_forbidden_operation";
}
