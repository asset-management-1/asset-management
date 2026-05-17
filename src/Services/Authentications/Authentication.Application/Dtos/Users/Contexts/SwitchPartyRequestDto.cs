namespace Authentication.Application.Dtos.Users.Contexts;

/// <summary>
/// Represents the requested party context to activate for the current user.
/// </summary>
public class SwitchPartyRequestDto
{
    /// <summary>
    /// Target party context, such as tenant or landlord.
    /// </summary>
    public string TargetContext { get; set; }
}
