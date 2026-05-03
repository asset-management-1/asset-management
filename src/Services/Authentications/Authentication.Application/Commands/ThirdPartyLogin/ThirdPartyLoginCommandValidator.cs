namespace Authentication.Application.Commands.ThirdPartyLogin;

public class ThirdPartyLoginCommandValidator : AbstractValidator<ThirdPartyLoginCommand>
{
    public ThirdPartyLoginCommandValidator()
    {
        RuleFor(x => x.Provider)
            .Required()
            .MaxLen(50);

        RuleFor(x => x.ExternalToken)
            .Required()
            .MaxLen(4000);
    }
}
