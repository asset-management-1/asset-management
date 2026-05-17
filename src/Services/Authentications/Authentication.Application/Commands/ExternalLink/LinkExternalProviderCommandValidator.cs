namespace Authentication.Application.Commands.ExternalLink;

/// <summary>
/// Validates requests that link an external identity provider to the current account.
/// </summary>
public class LinkExternalProviderCommandValidator : AbstractValidator<LinkExternalProviderCommand>
{
    /// <summary>
    /// Creates validation rules for the provider name and external token payload.
    /// </summary>
    public LinkExternalProviderCommandValidator()
    {
        RuleFor(x => x.Provider)
            .Required();

        RuleFor(x => x.ExternalToken)
            .Required();
    }
}
