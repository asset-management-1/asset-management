namespace Authentication.Application.Dtos.Users;

/// <summary>
/// Represents the authenticated user information returned by the auth API.
/// </summary>
public class UserInfoResponse
{
    /// <summary>
    /// Username used to sign in.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Primary email address of the user.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Primary phone number of the user.
    /// </summary>
    public string PhoneNumber { get; set; }

    /// <summary>
    /// Display name resolved from the linked party.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Roles assigned to the user.
    /// </summary>
    public List<string> Roles { get; set; } = [];

    /// <summary>
    /// Permissions granted through user roles.
    /// </summary>
    public List<string> Permissions { get; set; } = [];

    /// <summary>
    /// Linked external providers.
    /// </summary>
    public List<ExternalProviderDto> ExternalProviders { get; set; } = [];
}
