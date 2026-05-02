namespace Authentication.Application.Commands.ThirdPartyLogin;

public class ThirdPartyLoginCommandValidator : AbstractValidator<ThirdPartyLoginCommand>
{
    public ThirdPartyLoginCommandValidator()
    {
        RuleFor(x => x.LoginProvider)
            .Required()
            .MaxLen(50);

        RuleFor(x => x.ProviderKey)
            .Required()
            .MaxLen(255);

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}
