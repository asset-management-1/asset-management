namespace Authentication.Application.Commands.CompleteExternalRegistration;

/// <summary>
/// Validates the client-owned fields required to complete external registration.
/// </summary>
public class CompleteExternalRegistrationCommandValidator : AbstractValidator<CompleteExternalRegistrationCommand>
{
    /// <summary>
    /// Creates the external-registration validator.
    /// </summary>
    public CompleteExternalRegistrationCommandValidator()
    {
        // Provider must match one of the two mobile credential contracts supported by this release.
        RuleFor(x => x.Provider)
            .Required()
            .MaxLen(50)
            .Must(provider => provider is EXTERNAL_PROVIDER_GOOGLE or EXTERNAL_PROVIDER_FACEBOOK)
            .WithMessage(ApplicationErrorConstants.ExternalProviderErrors.INVALID_EXTERNAL_PROVIDER_MESSAGE);

        RuleFor(x => x.ExternalToken).Required().MaxLen(4000);

        RuleFor(x => x.FullName).Required().MaxLen(255);

        RuleFor(x => x.PhoneNumber).Required().MaxLen(50);

        RuleFor(x => x.PartyType).Required().MaxLen(50);
    }
}
