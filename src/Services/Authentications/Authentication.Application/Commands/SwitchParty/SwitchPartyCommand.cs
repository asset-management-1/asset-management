namespace Authentication.Application.Commands.SwitchParty;

/// <summary>
/// Represents a request to switch the authenticated user to another party context.
/// </summary>
public class SwitchPartyCommand : ICommand<ResponseDto<SwitchPartyResponseDto>>
{
    /// <summary>
    /// Target party context. Accepted values: <c>Tenant</c>, <c>Landlord</c>.
    /// </summary>
    public PartyTypeEnum TargetContext { get; set; }
}
