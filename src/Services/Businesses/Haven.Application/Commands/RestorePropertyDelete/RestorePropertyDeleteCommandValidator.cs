namespace Haven.Application.Commands.RestorePropertyDelete;

/// <summary>
/// Validates the property restore-delete request.
/// </summary>
public class RestorePropertyDeleteCommandValidator : AbstractValidator<RestorePropertyDeleteCommand>
{
    /// <summary>
    /// Creates validation rules for property restore-delete.
    /// </summary>
    public RestorePropertyDeleteCommandValidator()
    {
        // Restore is route-scoped and cannot proceed without a public property identifier.
        RuleFor(x => x.PropertyPublicId).RequiredGuid();
    }
}
