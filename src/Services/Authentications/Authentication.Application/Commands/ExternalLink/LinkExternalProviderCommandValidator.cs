namespace Authentication.Application.Commands.ExternalLink;

public class LinkExternalProviderCommandValidator : AbstractValidator<LinkExternalProviderCommand>
{
    public LinkExternalProviderCommandValidator()
    {
        RuleFor(x => x.Provider)
            .Required();

        RuleFor(x => x.ExternalToken)
            .Required();
    }
}
