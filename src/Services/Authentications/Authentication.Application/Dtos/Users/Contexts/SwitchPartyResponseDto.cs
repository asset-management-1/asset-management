namespace Authentication.Application.Dtos.Users.Contexts;

/// <summary>
/// Represents the result of switching the current party context.
/// </summary>
public class SwitchPartyResponseDto
{
    /// <summary>
    /// Indicates whether the switch succeeded.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Success message returned to the client.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// The party context that is active after the switch.
    /// </summary>
    public PartyTypeEnum CurrentContext { get; set; }

    /// <summary>
    /// All available party contexts linked to the current user.
    /// </summary>
    public List<PartyTypeEnum> AvailableContexts { get; set; } = [];
}
