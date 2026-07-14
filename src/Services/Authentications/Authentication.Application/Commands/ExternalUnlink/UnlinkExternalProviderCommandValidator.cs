namespace Authentication.Application.Commands.ExternalUnlink;

/// <summary>
/// Validates the external-provider unlink payload.
/// </summary>
public class UnlinkExternalProviderCommandValidator : AbstractValidator<UnlinkExternalProviderCommand>
{
    /// <summary>
    /// Creates validation rules for external-provider unlink requests.
    /// </summary>
    public UnlinkExternalProviderCommandValidator()
    {
        // Provider code selects the external login to unlink from the current account.
        RuleFor(x => x.Provider)
            .Required()
            .MaxLen(50);
    }
}
