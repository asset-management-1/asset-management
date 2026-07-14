namespace Authentication.Application.Commands.SwitchParty;

/// <summary>
/// Validates the party-context switch payload.
/// </summary>
public class SwitchPartyCommandValidator : AbstractValidator<SwitchPartyCommand>
{
    /// <summary>
    /// Creates validation rules that restrict party switching to supported UI contexts.
    /// </summary>
    public SwitchPartyCommandValidator()
    {
        // Party switch is limited to supported account contexts exposed by the API enum.
        RuleFor(x => x.TargetContext)
            .ValidEnum();
    }
}
