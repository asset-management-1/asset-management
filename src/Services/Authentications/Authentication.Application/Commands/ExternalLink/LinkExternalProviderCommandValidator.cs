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
        // Provider is matched by code, so keep it present and bounded before token validation.
        RuleFor(x => x.Provider)
            .Required()
            .MaxLen(50)
            .Must(provider => provider is EXTERNAL_PROVIDER_GOOGLE or EXTERNAL_PROVIDER_FACEBOOK)
            .WithMessage(ApplicationErrorConstants.ExternalProviderErrors.INVALID_EXTERNAL_PROVIDER_MESSAGE);

        // External token can be large but must stay within a safe request boundary.
        RuleFor(x => x.ExternalToken)
            .Required()
            .MaxLen(4000);
    }
}
